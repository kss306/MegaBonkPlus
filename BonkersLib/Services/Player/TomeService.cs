using Assets.Scripts._Data.Tomes;
using Assets.Scripts.Inventory__Items__Pickups;
using Assets.Scripts.Inventory__Items__Pickups.Stats;
using BonkersLib.Core;
using BonkersLib.Utils;
using Il2CppSystem.Collections.Generic;

namespace BonkersLib.Services.Player;

public class TomeService
{
    private PlayerInventory _playerInventory => BonkersAPI.Player.Inventory;

    public TomeInventory TomeInventory => MainThreadDispatcher.Evaluate(() => _playerInventory?.tomeInventory);

    public Dictionary<ETome, StatModifier> CurrentTomes =>
        MainThreadDispatcher.Evaluate(() => TomeInventory?.tomeUpgrade.ToSafeCopy());

    public Dictionary<ETome, int> TomeLevel =>
        MainThreadDispatcher.Evaluate(() => TomeInventory?.tomeLevels.ToSafeCopy());

    public TomeData GetTomeDataFromEnum(ETome eTome)
    {
        var tomeDataDict = BonkersAPI.Data.TomeData;
        return tomeDataDict.ContainsKey(eTome) ? tomeDataDict[eTome] : null;
    }

    public int GetTomeLevel(ETome eTome)
    {
        return MainThreadDispatcher.Evaluate(() =>
        {
            var levels = TomeInventory?.tomeLevels;
            if (levels != null && levels.ContainsKey(eTome))
            {
                return levels[eTome];
            }

            return 0;
        });
    }

    public List<StatModifier> GetTomeUpgradeOffer(TomeData td, ERarity rarity)
    {
        return MainThreadDispatcher.Evaluate(() => td.GetUpgradeOffer(rarity));
    }

    public void AddTomeWithStats(TomeData td, List<StatModifier> statModifiers, ERarity rarity)
    {
        MainThreadDispatcher.Enqueue(() =>
        {
            if (!BonkersAPI.Game.IsInGame || TomeInventory == null) return;

            TomeInventory.AddTome(td, statModifiers, rarity);
            ModLogger.LogDebug($"[TomeService] Added tome: {td.name}");
        });
    }

    public void AddTome(ETome eTome)
    {
        MainThreadDispatcher.Enqueue(() =>
        {
            if (!BonkersAPI.Game.IsInGame) return;

            var td = GetTomeDataFromEnum(eTome);

            if (!td)
            {
                ModLogger.LogDebug($"[TomeService] Could not find data for tome: {eTome}");
                return;
            }

            var statModifiers = td.GetUpgradeOffer(ERarity.New);

            if (TomeInventory != null)
            {
                TomeInventory.AddTome(td, statModifiers, ERarity.New);
                ModLogger.LogDebug($"[TomeService] Added tome via enum: {eTome}");
            }
        });
    }

    public bool RemoveTome(ETome eTome)
    {
        return MainThreadDispatcher.Evaluate(() =>
        {
            if (!BonkersAPI.Game.IsInGame || TomeInventory == null) return false;

            var upgrades = TomeInventory.tomeUpgrade;
            var levels = TomeInventory.tomeLevels;

            bool removedUpgrade = upgrades.Remove(eTome);
            bool removedLevel = levels.Remove(eTome);

            if (removedUpgrade || removedLevel)
            {
                BonkersAPI.Ui.RefreshUi();
                ModLogger.LogDebug($"[TomeService] Removed tome: {eTome}");
                return true;
            }

            return false;
        });
    }

    public void UpgradeWithRandomStats(ETome eTome, ERarity rarity)
    {
        MainThreadDispatcher.Enqueue(() =>
        {
            var td = GetTomeDataFromEnum(eTome);

            if (!td)
            {
                ModLogger.LogDebug($"[TomeService] Could not find TomeData for {eTome}");
                return;
            }

            var statModifiers = GetTomeUpgradeOffer(td, rarity);

            AddTomeWithStats(td, statModifiers, rarity);

            ModLogger.LogDebug($"[TomeService] Upgraded tome {eTome} with rarity {rarity}");
        });
    }
}