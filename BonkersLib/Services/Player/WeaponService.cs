using Assets.Scripts.Inventory__Items__Pickups;
using Assets.Scripts.Inventory__Items__Pickups.Stats;
using Assets.Scripts.Inventory__Items__Pickups.Weapons;
using Assets.Scripts.Menu.Shop;
using BonkersLib.Core;
using BonkersLib.Utils;
using Il2CppSystem.Collections.Generic;

namespace BonkersLib.Services.Player;

public class WeaponService
{
    private PlayerInventory _playerInventory => BonkersAPI.Player.Inventory;

    public WeaponInventory WeaponInventory =>
        MainThreadDispatcher.Evaluate(() => _playerInventory?.weaponInventory);

    public Dictionary<EWeapon, WeaponBase> CurrentWeapons =>
        MainThreadDispatcher.Evaluate(() => WeaponInventory?.weapons.ToSafeCopy());

    public WeaponData GetWeaponDataFromEnum(EWeapon eWeapon)
    {
        var weaponDataDict = BonkersAPI.Data.WeaponData;
        return weaponDataDict.ContainsKey(eWeapon) ? weaponDataDict[eWeapon] : null;
    }

    public WeaponData GetWeaponDataFromWeaponBase(WeaponBase wb)
    {
        return MainThreadDispatcher.Evaluate(() => wb?.weaponData);
    }

    public string GetWeaponNameFromWeaponBase(WeaponBase wb)
    {
        return MainThreadDispatcher.Evaluate(() =>
            wb?.weaponData?.damageSourceName ?? "WeaponData not found");
    }

    public List<StatModifier> GetWeaponUpgradeOffer(WeaponData wd, ERarity rarity)
    {
        return MainThreadDispatcher.Evaluate(() => wd ? wd.GetUpgradeOffer(rarity) : null);
    }

    public List<EStat> GetValidUpgradeStats(EWeapon eWeapon)
    {
        return MainThreadDispatcher.Evaluate(() =>
        {
            var wd = GetWeaponDataFromEnum(eWeapon);
            if (!wd) return new List<EStat>();

            var upgradeData = wd.upgradeData;
            var allowedUpgradeModifiers = upgradeData?.upgradeModifiers;

            var uniqueStats = new List<EStat>();

            if (allowedUpgradeModifiers == null || allowedUpgradeModifiers.Count == 0)
                return uniqueStats;

            for (var i = 0; i < allowedUpgradeModifiers.Count; i++)
            {
                var modifier = allowedUpgradeModifiers[i];
                if (modifier == null) continue;

                var stat = modifier.stat;
                if (!uniqueStats.Contains(stat))
                    uniqueStats.Add(stat);
            }

            return uniqueStats;
        });
    }

    public bool AddWeaponWithStats(WeaponData wd, List<StatModifier> stats)
    {
        return MainThreadDispatcher.Evaluate(() =>
        {
            if (!BonkersAPI.Game.IsInGame || WeaponInventory == null) return false;
            if (!wd) return false;

            var validStats = GetValidUpgradeStats(wd.eWeapon);

            foreach (var stat in stats)
            {
                if (!validStats.Contains(stat.stat))
                    return false;
            }

            WeaponInventory.AddWeapon(wd, stats);
            return true;
        });
    }

    public void AddWeapon(EWeapon eWeapon)
    {
        MainThreadDispatcher.Enqueue(() =>
        {
            var wd = GetWeaponDataFromEnum(eWeapon);
            if (!wd)
            {
                ModLogger.LogDebug($"[WeaponService] WeaponData not found for {eWeapon}");
                return;
            }

            var statModifiers = GetWeaponUpgradeOffer(wd, ERarity.New);
            AddWeaponWithStats(wd, statModifiers);

            ModLogger.LogDebug($"[WeaponService] Added weapon: {eWeapon}");
        });
    }

    public bool RemoveWeapon(EWeapon eWeapon)
    {
        return MainThreadDispatcher.Evaluate(() =>
        {
            var weapons = WeaponInventory?.weapons;

            if (weapons == null || !weapons.Remove(eWeapon))
                return false;

            BonkersAPI.Ui.RefreshUi();
            ModLogger.LogDebug($"[WeaponService] Removed weapon: {eWeapon}");
            return true;
        });
    }

    public void UpgradeWithRandomStats(WeaponBase wb, ERarity rarity)
    {
        MainThreadDispatcher.Enqueue(() =>
        {
            if (wb == null || !wb.weaponData) return;

            var wd = wb.weaponData;
            var upgrades = GetWeaponUpgradeOffer(wd, rarity);
            AddWeaponWithStats(wd, upgrades);

            ModLogger.LogDebug($"[WeaponService] Upgraded weapon: {wd.name}");
        });
    }

    public bool DowngradeWeapon(WeaponBase wb)
    {
        return MainThreadDispatcher.Evaluate(() =>
        {
            if (wb?.upgrades == null || wb.upgrades.Count <= 1)
                return false;

            wb.upgrades.RemoveAt(wb.upgrades.Count - 1);
            wb.level--;

            BonkersAPI.Ui.RefreshUi();
            ModLogger.LogDebug($"[WeaponService] Downgraded weapon: {GetWeaponNameFromWeaponBase(wb)}");
            return true;
        });
    }
    
    public bool ClearWeaponUpgrades(WeaponBase wb)
    {
        return MainThreadDispatcher.Evaluate(() =>
        {
            if (wb?.upgrades is not { Count: > 1 })
                return false;

            wb.upgrades.RemoveRange(1, wb.upgrades.Count - 1);
            wb.level = 1;
        
            BonkersAPI.Ui.RefreshUi();
        
            ModLogger.LogDebug($"[WeaponService] Cleared upgrades for weapon: {GetWeaponNameFromWeaponBase(wb)}");
            return true;
        });
    }
}