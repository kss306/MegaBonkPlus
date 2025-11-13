using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using BepInEx.Configuration;
using BonkersLib.Core;
using MegaBonkPlusMod.Actions.Base;
using MegaBonkPlusMod.Config;
using MegaBonkPlusMod.Utils;
using UnityEngine;

namespace MegaBonkPlusMod.Core;

public static class HotkeyManager
{
    public class HotkeyDefinition
    {
        public string Key { get; set; }
        public string ActionId { get; set; }
        public JsonElement Payload { get; set; }
    }

    public static bool IsEnabled { get; private set; } = true;
    private static readonly List<HotkeyDefinition> Hotkeys = new();
    private static ConfigEntry<string> _hotkeyConfigEntry;
    
    public static void Initialize(ConfigFile config)
    {
        _hotkeyConfigEntry = config.Bind(
            "Hotkeys",
            "Configuration",
            "[]",
            "Stores the hotkey configuration as a JSON string. Do not edit manually unless you know what you are doing.");
        
        LoadConfig();
    }

    private static void LoadConfig()
    {
        try
        {
            var configJson = _hotkeyConfigEntry.Value;
            if (string.IsNullOrWhiteSpace(configJson) || configJson == "[]")
            {
                ModLogger.LogDebug("[HotkeyManager] No hotkey config found or config is empty, using defaults.");
                IsEnabled = true;
                Hotkeys.Clear();
                return;
            }

            var storedConfig = JsonSerializer.Deserialize<StoredHotkeyConfig>(configJson);
            if (storedConfig != null)
            {
                IsEnabled = storedConfig.Enabled;
                Hotkeys.Clear();
                Hotkeys.AddRange(storedConfig.Hotkeys ?? new List<HotkeyDefinition>());
                ModLogger.LogDebug($"[HotkeyManager] Loaded {Hotkeys.Count} hotkeys from config file.");
            }
        }
        catch (Exception ex)
        {
            ModLogger.LogDebug($"[HotkeyManager] Error loading hotkey config, resetting to default: {ex.Message}");
            IsEnabled = true;
            Hotkeys.Clear();
        }
    }
    
    private static void SaveConfig()
    {
        try
        {
            var configToStore = new StoredHotkeyConfig { Enabled = IsEnabled, Hotkeys = Hotkeys };
            var configJson = JsonSerializer.Serialize(configToStore);
            _hotkeyConfigEntry.Value = configJson;
            ModLogger.LogDebug("[HotkeyManager] Hotkey config saved.");
        }
        catch (Exception ex)
        {
            ModLogger.LogDebug($"[HotkeyManager] Error saving hotkey config: {ex.Message}");
        }
    }

    public static void UpdateConfig(JsonElement payload)
    {
        try
        {
            IsEnabled = !payload.TryGetProperty("enabled", out var enabledElement) || enabledElement.GetBoolean();

            Hotkeys.Clear();

            if (!payload.TryGetProperty("hotkeys", out var hotkeysElement) ||
                hotkeysElement.ValueKind != JsonValueKind.Array)
            {
                ModLogger.LogDebug("[HotkeyManager] No hotkeys array in payload.");
                return;
            }

            foreach (var hotkeyJson in hotkeysElement.EnumerateArray())
            {
                if (!hotkeyJson.TryGetProperty("action", out var actionElement) ||
                    actionElement.ValueKind == JsonValueKind.Null)
                {
                    continue;
                }

                if (!hotkeyJson.TryGetProperty("key", out var keyElement) || keyElement.ValueKind == JsonValueKind.Null)
                {
                    continue;
                }

                string key = keyElement.GetString();
                string actionId = actionElement.GetProperty("id").GetString();
                JsonElement actionPayload = actionElement.GetProperty("payload").Clone();

                Hotkeys.Add(new HotkeyDefinition
                {
                    Key = key,
                    ActionId = actionId,
                    Payload = actionPayload
                });
            }

            ModLogger.LogDebug($"[HotkeyManager] Config updated. {Hotkeys.Count} hotkeys registered.");
            SaveConfig();
        }
        catch (Exception ex)
        {
            ModLogger.LogDebug($"[HotkeyManager] Error updating config: {ex.Message}");
        }
    }

    public static void CheckKeys(ActionHandler actionHandler)
    {
        if (!IsEnabled || Hotkeys.Count == 0 || !BonkersAPI.Game.IsInGame)
            return;

        foreach (var hotkey in Hotkeys)
        {
            if (Input.GetKeyDown(TranslateJsCodeToUnityKey(hotkey.Key)))
            {
                ModLogger.LogDebug($"[HotkeyManager] Hotkey pressed: {hotkey.Key} -> {hotkey.ActionId}");
                try
                {
                    JsonElement payloadToExecute = hotkey.Payload.Clone();

                    if (hotkey.ActionId.Equals("spawn_items", StringComparison.OrdinalIgnoreCase))
                        payloadToExecute = TransformSpawnItemPayload(hotkey.Payload);

                    actionHandler.HandleAction(hotkey.ActionId, payloadToExecute);
                }
                catch (Exception ex)
                {
                    ModLogger.LogDebug($"[HotkeyManager] Error executing action {hotkey.ActionId}: {ex.Message}");
                }
            }
        }
    }

    private static KeyCode TranslateJsCodeToUnityKey(string jsCode)
    {
        if (jsCode == "ControlLeft") jsCode = "LeftControl";
        if (jsCode == "ControlRight") jsCode = "RightControl";
        if (jsCode == "ShiftLeft") jsCode = "LeftShift";
        if (jsCode == "ShiftRight") jsCode = "RightShift";
        if (jsCode == "AltLeft") jsCode = "LeftAlt";
        if (jsCode == "AltRight") jsCode = "RightAlt";

        if (Enum.TryParse<KeyCode>(jsCode, true, out var keyCode))
        {
            return keyCode;
        }

        if (jsCode.StartsWith("Key") && jsCode.Length > 3)
        {
            if (Enum.TryParse<KeyCode>(jsCode.Substring(3), true, out keyCode))
                return keyCode;
        }

        if (jsCode.StartsWith("Digit") && jsCode.Length > 5)
        {
            if (Enum.TryParse<KeyCode>("Alpha" + jsCode.Substring(5), true, out keyCode))
                return keyCode;
        }

        ModLogger.LogDebug($"[HotkeyManager] Unmapped key: {jsCode}");
        return KeyCode.None;
    }

    private static JsonElement TransformSpawnItemPayload(JsonElement flatPayload)
    {
        try
        {
            string itemId = flatPayload.GetProperty("itemId").GetString();
            int quantity = flatPayload.GetProperty("quantity").GetInt32();

            string newJson = $@"
            {{
                ""items"": [
                    {{
                        ""id"": ""{itemId}"",
                        ""quantity"": {quantity}
                    }}
                ]
            }}";

            using (JsonDocument doc = JsonDocument.Parse(newJson))
            {
                return doc.RootElement.Clone();
            }
        }
        catch (Exception ex)
        {
            ModLogger.LogDebug($"[HotkeyManager] Failed to transform spawn_items payload: {ex.Message}");
            return JsonDocument.Parse("{}").RootElement;
        }
    }

    public static object GetCurrentConfig()
    {
        var hotkeysForFrontend = Hotkeys.Select(h => new
        {
            key = h.Key,
            action = new
            {
                id = h.ActionId,
                payload = h.Payload
            }
        }).ToList();
        
        return new
        {
            enabled = IsEnabled,
            hotkeys = hotkeysForFrontend
        };
    }
}