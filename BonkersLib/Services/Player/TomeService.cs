using Assets.Scripts._Data.Tomes;
using Assets.Scripts.Inventory__Items__Pickups;
using Assets.Scripts.Inventory__Items__Pickups.Stats;
using BonkersLib.Core;
using Il2CppSystem.Collections.Generic;

namespace BonkersLib.Services.Player;

public class TomeService
{
    private PlayerInventory _playerInventory => BonkersAPI.Player.Inventory;

    public TomeInventory TomeInventory => _playerInventory?.tomeInventory;
    public Dictionary<ETome, StatModifier> CurrentTomes => TomeInventory?.tomeUpgrade;
    public Dictionary<ETome, int> TomeLevel => TomeInventory?.tomeLevels;

    public TomeData GetTomeDataFromEnum(ETome eTome)
    {
        return BonkersAPI.Data.TomeData[eTome];
    }

    public int GetTomeLevel(ETome eTome)
    {
        return TomeLevel[eTome];
    }

    public List<StatModifier> GetTomeUpgradeOffer(TomeData td, ERarity rarity)
    {
        return td.GetUpgradeOffer(rarity);
    }

    public void AddTomeWithStats(TomeData td, List<StatModifier> statModifiers, ERarity rarity)
    {
        TomeInventory.AddTome(td, statModifiers, rarity);
    }


    public void AddTome(ETome eTome)
    {
        var td = GetTomeDataFromEnum(eTome);
        var statModifiers = GetTomeUpgradeOffer(td, ERarity.New);
        AddTomeWithStats(td, statModifiers, ERarity.New);
    }

    public bool RemoveTome(ETome eTome)
    {
        if (!CurrentTomes.Remove(eTome) || !TomeLevel.Remove(eTome))
            return false;

        BonkersAPI.Ui.RefreshUi();
        return true;
    }

    public void UpgradeWithRandomStats(ETome eTome, ERarity rarity)
    {
        var td = GetTomeDataFromEnum(eTome);
        var statModifiers = GetTomeUpgradeOffer(td, rarity);
        AddTomeWithStats(td, statModifiers, rarity);
    }
}