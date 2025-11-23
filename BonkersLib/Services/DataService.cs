using Assets.Scripts._Data.Tomes;
using Assets.Scripts.Inventory__Items__Pickups.Items;
using Assets.Scripts.Saves___Serialization.Progression.Achievements;
using BonkersLib.Core;
using BonkersLib.Utils;
using Il2CppSystem.Collections.Generic;

namespace BonkersLib.Services;

public class DataService
{
    public DataManager CurrentDataManager { get; private set; }

    internal void SetDataManager()
    {
        CurrentDataManager = AlwaysManager.Instance.dataManager;
    }

    public Dictionary<EItem, ItemData> ItemData =>
        MainThreadDispatcher.Evaluate(() => CurrentDataManager?.itemData.ToSafeCopy());

    public Dictionary<EWeapon, WeaponData> WeaponData =>
        MainThreadDispatcher.Evaluate(() => CurrentDataManager?.weapons.ToSafeCopy());

    public Dictionary<ETome, TomeData> TomeData =>
        MainThreadDispatcher.Evaluate(() => CurrentDataManager?.tomeData.ToSafeCopy());

    public Dictionary<ECharacter, CharacterData> CharacterData =>
        MainThreadDispatcher.Evaluate(() => CurrentDataManager?.characterData.ToSafeCopy());

    public Dictionary<string, MyAchievement> AchievementData =>
        MainThreadDispatcher.Evaluate(() => CurrentDataManager?.achievementsData.ToSafeCopy());
}