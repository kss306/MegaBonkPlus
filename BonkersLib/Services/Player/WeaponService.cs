using Assets.Scripts.Inventory__Items__Pickups;
using Assets.Scripts.Inventory__Items__Pickups.Stats;
using Assets.Scripts.Inventory__Items__Pickups.Weapons;
using Assets.Scripts.Menu.Shop;
using BonkersLib.Core;
using Il2CppSystem.Collections.Generic;

namespace BonkersLib.Services.Player;

public class WeaponService
{
    private PlayerInventory _playerInventory => BonkersAPI.Player.Inventory;

    public WeaponInventory WeaponInventory => _playerInventory?.weaponInventory;
    public Dictionary<EWeapon, WeaponBase> CurrentWeapons => WeaponInventory?.weapons;

    public WeaponData GetWeaponDataFromEnum(EWeapon eWeapon)
    {
        return BonkersAPI.Data.WeaponData[eWeapon];
    }

    public WeaponData GetWeaponDataFromWeaponBase(WeaponBase wb)
    {
        return wb.weaponData;
    }

    public string GetWeaponNameFromWeaponBase(WeaponBase wb)
    {
        return wb.weaponData?.damageSourceName ?? "WeaponData not found";
    }

    public List<StatModifier> GetWeaponUpgradeOffer(WeaponData wd, ERarity rarity)
    {
        return wd.GetUpgradeOffer(rarity);
    }

    public List<EStat> GetValidUpgradeStats(EWeapon eWeapon)
    {
        var wd = GetWeaponDataFromEnum(eWeapon);
        if (!wd)
            return new List<EStat>();

        var upgradeData = wd.upgradeData;
        var allowedUpgradeModifiers = upgradeData?.upgradeModifiers;
        if (allowedUpgradeModifiers == null || allowedUpgradeModifiers.Count == 0)
            return new List<EStat>();

        var uniqueStats = new List<EStat>();
        for (var i = 0; i < allowedUpgradeModifiers.Count; i++)
        {
            var modifier = allowedUpgradeModifiers[i];
            if (modifier == null)
                continue;

            var stat = modifier.stat;
            if (!uniqueStats.Contains(stat)) uniqueStats.Add(stat);
        }

        return uniqueStats;
    }

    public bool AddWeaponWithStats(WeaponData wd, List<StatModifier> stats)
    {
        if (WeaponInventory == null || !wd)
            return false;

        var validStats = GetValidUpgradeStats(wd.eWeapon);

        foreach (var stat in stats)
            if (!validStats.Contains(stat.stat))
                return false;

        WeaponInventory.AddWeapon(wd, stats);
        return true;
    }


    public void AddWeapon(EWeapon eWeapon)
    {
        var wd = GetWeaponDataFromEnum(eWeapon);
        var statModifiers = GetWeaponUpgradeOffer(wd, ERarity.New);
        AddWeaponWithStats(wd, statModifiers);
    }

    public bool RemoveWeapon(EWeapon eWeapon)
    {
        if (!CurrentWeapons.Remove(eWeapon))
            return false;

        BonkersAPI.Ui.RefreshUi();
        return true;
    }

    public void UpgradeWithRandomStats(WeaponBase wb, ERarity rarity)
    {
        var wd = wb.weaponData;
        var upgrades = GetWeaponUpgradeOffer(wd, rarity);
        AddWeaponWithStats(wd, upgrades);
    }

    public bool DowngradeWeapon(WeaponBase wb)
    {
        if (wb?.upgrades is not { Count: > 1 })
            return false;

        wb.upgrades.RemoveAt(wb.upgrades.Count - 1);
        wb.level--;
        BonkersAPI.Ui.RefreshUi();
        return true;
    }

    public bool ClearWeaponUpgrades(WeaponBase wb)
    {
        if (wb?.upgrades is not { Count: > 1 })
            return false;

        wb.upgrades.RemoveRange(1, wb.upgrades.Count - 1);
        wb.level = 1;
        BonkersAPI.Ui.RefreshUi();
        return true;
    }
}