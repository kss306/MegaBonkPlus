using System;
using System.Text.Json;
using Assets.Scripts._Data.Tomes;
using Assets.Scripts.Inventory__Items__Pickups;
using Assets.Scripts.Inventory__Items__Pickups.Stats;
using Assets.Scripts.Menu.Shop;
using BonkersLib.Core;
using Il2CppSystem.Collections.Generic;
using MegaBonkPlusMod.Actions.Base;
using MegaBonkPlusMod.Utils;

namespace MegaBonkPlusMod.Actions.Inventory;

public class TomeAction : IAction
{
    public string Execute(JsonElement payload, ActionHandler actionHandler)
    {
        if (!BonkersAPI.Game.IsInGame)
            return "Cannot modify tomes: Not in game";

        if (BonkersAPI.Tome?.TomeInventory == null)
            return "Tome service or inventory not available";

        if (!payload.TryGetProperty("mode", out var modeElement) ||
            modeElement.ValueKind != JsonValueKind.String)
            return "Error: 'mode' is required";

        var mode = modeElement.GetString()?.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(mode))
            return "Error: 'mode' is empty";

        if (!payload.TryGetProperty("tome", out var tomeElement) ||
            tomeElement.ValueKind != JsonValueKind.String)
            return "Error: 'tome' (ETome name) is required";

        var tomeName = tomeElement.GetString();
        if (string.IsNullOrWhiteSpace(tomeName))
            return "Error: 'tome' is empty";

        if (!Enum.TryParse<ETome>(tomeName, true, out var eTome))
            return $"Error: Unknown tome '{tomeName}'";

        switch (mode)
        {
            case "add":
                return HandleAddTome(payload, eTome);

            case "remove":
            case "removetome":
                return HandleRemoveTome(eTome);

            default:
                return $"Error: Unknown mode '{mode}' (expected 'add' or 'remove')";
        }
    }


    private string HandleAddTome(
        JsonElement payload,
        ETome eTome)
    {
        JsonElement upgradeElement;
        var upgradeMode = "random";

        if (payload.TryGetProperty("upgrade", out upgradeElement) &&
            upgradeElement.ValueKind == JsonValueKind.Object)
        {
            if (upgradeElement.TryGetProperty("mode", out var upgradeModeElement) &&
                upgradeModeElement.ValueKind == JsonValueKind.String)
                upgradeMode = upgradeModeElement.GetString()?.Trim().ToLowerInvariant() ?? "random";
        }
        else
        {
            upgradeMode = "random";
            upgradeElement = default;
        }

        switch (upgradeMode)
        {
            case "random":
                return AddTomeWithRandomUpgrades(eTome, upgradeElement);

            case "custom":
                return AddTomeWithCustomStats(eTome, upgradeElement);

            default:
                BonkersAPI.Tome.AddTome(eTome);
                ModLogger.LogDebug($"[TomeAction] Added tome {eTome} with default AddTome behavior");
                return $"Added or upgraded tome '{eTome}' (default behavior)";
        }
    }

    private string AddTomeWithRandomUpgrades(
        ETome eTome,
        JsonElement upgradeElement)
    {
        var rarity = ERarity.New;
        if (upgradeElement.ValueKind == JsonValueKind.Object)
            if (upgradeElement.TryGetProperty("rarity", out var rarityElement) &&
                rarityElement.ValueKind == JsonValueKind.String)
            {
                var rarityStr = rarityElement.GetString();
                if (!string.IsNullOrWhiteSpace(rarityStr) &&
                    Enum.TryParse<ERarity>(rarityStr, true, out var parsedRarity))
                    rarity = parsedRarity;
            }

        var currentTomes = BonkersAPI.Tome.CurrentTomes;

        if (currentTomes == null || !currentTomes.TryGetValue(eTome, out var statModifier) ||
            statModifier == null)
            BonkersAPI.Tome.AddTome(eTome);

        var currentTome = BonkersAPI.Tome.GetTomeDataFromEnum(eTome);
        if (BonkersAPI.Tome.GetTomeLevel(eTome) >= 99)
            return $"Error: Cannot upgrade tome {eTome} (already at max level)";

        BonkersAPI.Tome.UpgradeWithRandomStats(eTome, rarity);

        ModLogger.LogDebug(
            $"[TomeAction] Added / upgraded tome {eTome} with random upgrades (rarity={rarity})");
        return $"Upgraded '{eTome}' {rarity}";
    }

    private string AddTomeWithCustomStats(
        ETome eTome,
        JsonElement upgradeElement)
    {
        if (upgradeElement.ValueKind != JsonValueKind.Object ||
            !upgradeElement.TryGetProperty("stats", out var statsElement) ||
            statsElement.ValueKind != JsonValueKind.Array)
            return "Error: For upgrade.mode='custom' you must provide 'upgrade.stats' as an array";

        List<StatModifier> statModifiers = new();

        foreach (var statJson in statsElement.EnumerateArray())
        {
            if (statJson.ValueKind != JsonValueKind.Object)
                continue;

            var modifier = TryCreateStatModifierFromJson(statJson);
            if (modifier != null) statModifiers.Add(modifier);

            if (statModifiers.Count >= 4)
                break;
        }

        if (statModifiers.Count == 0)
            return "Error: No valid custom stat modifiers provided (or all invalid)";

        var tomeData = BonkersAPI.Tome.GetTomeDataFromEnum(eTome);
        BonkersAPI.Tome.AddTomeWithStats(tomeData, statModifiers, ERarity.Legendary);

        ModLogger.LogDebug(
            $"[TomeAction] Added tome {eTome} with {statModifiers.Count} custom stat modifiers");
        return $"Added tome '{eTome}' with {statModifiers.Count} custom stat modifiers";
    }


    private string HandleRemoveTome(
        ETome eTome)
    {
        var currentTomes = BonkersAPI.Tome.CurrentTomes;
        var hasTome = currentTomes != null && currentTomes.ContainsKey(eTome);
        if (!hasTome)
            return $"Error: Tome '{eTome}' not in inventory";

        var removed = BonkersAPI.Tome.RemoveTome(eTome);
        if (!removed)
            return $"Error: Failed to remove tome '{eTome}'";

        ModLogger.LogDebug($"[TomeAction] Removed tome {eTome}");
        return $"Removed tome '{eTome}'";
    }

    private StatModifier TryCreateStatModifierFromJson(JsonElement statJson)
    {
        try
        {
            string statName = null;
            string opName = null;
            var value = 0f;

            if (statJson.TryGetProperty("stat", out var statNameElement) &&
                statNameElement.ValueKind == JsonValueKind.String)
                statName = statNameElement.GetString();

            if (statJson.TryGetProperty("operation", out var opElement) &&
                opElement.ValueKind == JsonValueKind.String)
                opName = opElement.GetString();

            if (statJson.TryGetProperty("value", out var valueElement))
            {
                if (valueElement.ValueKind == JsonValueKind.Number)
                    value = valueElement.GetSingle();
                else if (valueElement.ValueKind == JsonValueKind.String &&
                         float.TryParse(valueElement.GetString(), out var parsedFloat))
                    value = parsedFloat;
            }

            if (string.IsNullOrWhiteSpace(statName))
            {
                ModLogger.LogDebug("[TomeAction] StatModifier missing 'stat' field");
                return null;
            }

            if (string.IsNullOrWhiteSpace(opName))
            {
                ModLogger.LogDebug("[TomeAction] StatModifier missing 'operation' field");
                return null;
            }

            if (!Enum.TryParse<EStat>(statName, true, out var statEnum))
            {
                ModLogger.LogDebug($"[TomeAction] Unknown EStat '{statName}'");
                return null;
            }

            if (!Enum.TryParse<EStatModifyType>(opName, true, out var modifyEnum))
            {
                ModLogger.LogDebug($"[TomeAction] Unknown EStatModifyType '{opName}'");
                return null;
            }

            var modifier = new StatModifier
            {
                stat = statEnum,
                modifyType = modifyEnum,
                modification = value
            };

            return modifier;
        }
        catch (Exception ex)
        {
            ModLogger.LogDebug($"[TomeAction] Failed to parse StatModifier from json: {ex.Message}");
            return null;
        }
    }
}