using Assets.Scripts.Actors.Player;
using Assets.Scripts.Inventory__Items__Pickups;
using Assets.Scripts.Inventory__Items__Pickups.Stats;
using Assets.Scripts.Inventory__Items__Pickups.Weapons;
using BonkersLib.Core;
using BonkersLib.Utils;
using Il2CppSystem.Collections.Generic;

namespace BonkersLib.Services.Player;

public class WeaponService
{
    private MyPlayer _player => BonkersAPI.Game.PlayerController;
    private PlayerInventory _playerInventory => BonkersAPI.Player.Inventory;

    public WeaponInventory WeaponInventory => _playerInventory?.weaponInventory;
    public Dictionary<EWeapon, WeaponBase> CurrentWeapons => WeaponInventory?.weapons;

    public WeaponData GetWeaponData(WeaponBase wb)
        => wb.weaponData;

    public string GetWeaponNameFromWeaponBase(WeaponBase wb)
        => wb.weaponData?.damageSourceName ?? "WeaponData not found";
    
    public List<StatModifier> GetWeaponUpgradeOffer(WeaponData wd, ERarity rarity)
        => wd.GetUpgradeOffer(rarity);

    public void AddWeaponWithStats(WeaponData wd, List<StatModifier> stats)
        => WeaponInventory.AddWeapon(wd, stats);

    public void UpgradeWithRandomStats(WeaponBase wb, ERarity rarity, int level)
    {
        if (wb.level >= level)
            return;

        WeaponData wd = wb.weaponData;
        int levelsToUpgrade = level - wb.level;
        
        for (int i = 0; i < levelsToUpgrade; i++)
        {
            List<StatModifier> upgrades = GetWeaponUpgradeOffer(wd, rarity);
            AddWeaponWithStats(wd, upgrades);
        }
    }
    
    public bool DowngradeWeapon(WeaponBase wb)
    {
        if (wb?.upgrades is not { Count: > 1 })
            return false;
        
        wb.upgrades.RemoveAt(wb.upgrades.Count - 1);
        wb.level--;
        BonkersAPI.Ui.RefreshUi();
        ModLogger.LogDebug($"Downgraded Weapon {GetWeaponNameFromWeaponBase(wb)} to level {wb.level}");
        return true;
    }
    
    public bool ClearWeaponUpgrades(WeaponBase wb)
    {
        if (wb?.upgrades is not { Count: > 1 })
            return false;
        
        wb.upgrades.RemoveRange(1, wb.upgrades.Count - 1);
        wb.level = 1;
        BonkersAPI.Ui.RefreshUi();
        ModLogger.LogDebug($"Downgraded Weapon {GetWeaponNameFromWeaponBase(wb)} to level 1");
        return true;
    }
}