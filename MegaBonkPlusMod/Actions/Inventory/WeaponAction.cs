using System;
using System.Text.Json;
using Assets.Scripts.Inventory__Items__Pickups;
using Assets.Scripts.Inventory__Items__Pickups.Stats;
using Assets.Scripts.Menu.Shop;
using BonkersLib.Core;
using Il2CppSystem.Collections.Generic;
using MegaBonkPlusMod.Actions.Base;
using MegaBonkPlusMod.Utils;

namespace MegaBonkPlusMod.Actions.Inventory;

public class WeaponAction : IAction
{
    public string Execute(JsonElement payload, ActionHandler actionHandler)
    {
        if (!BonkersAPI.Game.IsInGame)
            return "Cannot modify weapons: Not in game";

        if (BonkersAPI.Weapon?.WeaponInventory == null)
            return "Weapon service or inventory not available";

        if (!payload.TryGetProperty("mode", out var modeElement) ||
            modeElement.ValueKind != JsonValueKind.String)
            return "Error: 'mode' is required";

        var mode = modeElement.GetString()?.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(mode))
            return "Error: 'mode' is empty";

        if (!payload.TryGetProperty("weapon", out var weaponElement) ||
            weaponElement.ValueKind != JsonValueKind.String)
            return "Error: 'weapon' (EWeapon name) is required";

        var weaponName = weaponElement.GetString();
        if (string.IsNullOrWhiteSpace(weaponName))
            return "Error: 'weapon' is empty";

        if (!Enum.TryParse<EWeapon>(weaponName, true, out var eWeapon))
            return $"Error: Unknown weapon '{weaponName}'";

        switch (mode)
        {
            case "add":
                return HandleAddWeapon(payload, eWeapon);

            case "remove":
            case "removeweapon":
                return HandleRemoveWeapon(eWeapon);

            case "downgrade":
            case "downgradeweapon":
                return HandleDowngradeWeapon(eWeapon);

            case "clear":
            case "clearupgrades":
                return HandleClearWeaponUpgrades(eWeapon);

            case "options":
                return GetWeaponUpgradeOptions(eWeapon);

            default:
                return $"Error: Unknown mode '{mode}' (expected 'add', 'remove', 'downgrade', 'clear' or 'options')";
        }
    }


    private string HandleAddWeapon(
        JsonElement payload,
        EWeapon eWeapon)
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
                return AddWeaponWithRandomUpgrades(eWeapon, upgradeElement);

            case "custom":
                return AddWeaponWithCustomStats(eWeapon, upgradeElement);

            default:
                BonkersAPI.Weapon.AddWeapon(eWeapon);
                ModLogger.LogDebug($"[WeaponAction] Added weapon {eWeapon} with default AddWeapon behavior");
                return $"Added or upgraded weapon '{eWeapon}' (default behavior)";
        }
    }

    private string AddWeaponWithRandomUpgrades(
        EWeapon eWeapon,
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

        var currentWeapons = BonkersAPI.Weapon.CurrentWeapons;

        if (currentWeapons == null || !currentWeapons.TryGetValue(eWeapon, out var weaponBase) ||
            weaponBase == null)
        {
            BonkersAPI.Weapon.AddWeapon(eWeapon);
            return $"Added '{eWeapon}'";
        }

        if (currentWeapons == null || !currentWeapons.TryGetValue(eWeapon, out var wb) || wb == null)
            return $"Error: Failed to add or retrieve weapon '{eWeapon}'";

        BonkersAPI.Weapon.UpgradeWithRandomStats(wb, rarity);

        ModLogger.LogDebug(
            $"[WeaponAction] Upgraded weapon {eWeapon} with random upgrades (rarity={rarity})");
        return $"Upgraded '{eWeapon}' {rarity}";
    }

    private string AddWeaponWithCustomStats(
        EWeapon eWeapon,
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

        var weaponData = BonkersAPI.Weapon.GetWeaponDataFromEnum(eWeapon);
        var success = BonkersAPI.Weapon.AddWeaponWithStats(weaponData, statModifiers);

        if (!success) return GetWeaponUpgradeOptions(eWeapon);

        ModLogger.LogDebug(
            $"[WeaponAction] Added weapon {eWeapon} with {statModifiers.Count} custom stat modifiers");
        return $"Added weapon '{eWeapon}' with {statModifiers.Count} custom stat modifiers";
    }


    private string HandleRemoveWeapon(
        EWeapon eWeapon)
    {
        var currentWeapons = BonkersAPI.Weapon.CurrentWeapons;
        var hasWeapon = currentWeapons != null && currentWeapons.ContainsKey(eWeapon);
        if (!hasWeapon)
            return $"Error: Weapon '{eWeapon}' not in inventory";

        var removed = BonkersAPI.Weapon.RemoveWeapon(eWeapon);
        if (!removed)
            return $"Error: Failed to remove weapon '{eWeapon}'";

        ModLogger.LogDebug($"[WeaponAction] Removed weapon {eWeapon}");
        return $"Removed weapon '{eWeapon}'";
    }


    private string HandleDowngradeWeapon(
        EWeapon eWeapon)
    {
        var currentWeapons = BonkersAPI.Weapon.CurrentWeapons;
        if (currentWeapons == null || !currentWeapons.TryGetValue(eWeapon, out var wb) || wb == null)
            return $"Error: Weapon '{eWeapon}' not in inventory";

        var success = BonkersAPI.Weapon.DowngradeWeapon(wb);
        if (!success)
            return $"Error: Could not downgrade weapon '{eWeapon}' (probably already at minimum level)";

        return $"Downgraded weapon '{eWeapon}' to level {wb.level}";
    }

    private string HandleClearWeaponUpgrades(
        EWeapon eWeapon)
    {
        var currentWeapons = BonkersAPI.Weapon.CurrentWeapons;
        if (currentWeapons == null || !currentWeapons.TryGetValue(eWeapon, out var wb) || wb == null)
            return $"Error: Weapon '{eWeapon}' not in inventory";

        var success = BonkersAPI.Weapon.ClearWeaponUpgrades(wb);
        if (!success)
            return $"Error: Could not clear upgrades for weapon '{eWeapon}'";

        return $"Cleared upgrades for weapon '{eWeapon}' (reset to level 1)";
    }

    private string GetWeaponUpgradeOptions(EWeapon eWeapon)
    {
        var weaponService = BonkersAPI.Weapon;
        if (weaponService == null)
            return JsonSerializer.Serialize(new
            {
                weapon = eWeapon.ToString(),
                error = "Weapon service not available",
                allowedStats = Array.Empty<string>()
            });

        var validStats = weaponService.GetValidUpgradeStats(eWeapon);
        if (validStats == null || validStats.Count == 0)
            return JsonSerializer.Serialize(new
            {
                weapon = eWeapon.ToString(),
                error = "No specific upgrade modifiers defined for this weapon",
                allowedStats = Array.Empty<string>()
            });

        var statNames = new System.Collections.Generic.List<string>();
        foreach (var stat in validStats)
        {
            var name = stat.ToString();
            if (!statNames.Contains(name)) statNames.Add(name);
        }

        var payload = new
        {
            weapon = eWeapon.ToString(),
            allowedStats = statNames.ToArray()
        };

        return JsonSerializer.Serialize(payload);
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
                ModLogger.LogDebug("[WeaponAction] StatModifier missing 'stat' field");
                return null;
            }

            if (string.IsNullOrWhiteSpace(opName))
            {
                ModLogger.LogDebug("[WeaponAction] StatModifier missing 'operation' field");
                return null;
            }

            if (!Enum.TryParse<EStat>(statName, true, out var statEnum))
            {
                ModLogger.LogDebug($"[WeaponAction] Unknown EStat '{statName}'");
                return null;
            }

            if (!Enum.TryParse<EStatModifyType>(opName, true, out var modifyEnum))
            {
                ModLogger.LogDebug($"[WeaponAction] Unknown EStatModifyType '{opName}'");
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
            ModLogger.LogDebug($"[WeaponAction] Failed to parse StatModifier from json: {ex.Message}");
            return null;
        }
    }
}