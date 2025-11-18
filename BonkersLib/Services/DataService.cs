using Assets.Scripts._Data.Tomes;
using Assets.Scripts.Inventory__Items__Pickups.Items;
using Assets.Scripts.Saves___Serialization.Progression.Achievements;
using Il2CppSystem.Collections.Generic;

namespace BonkersLib.Services;

public class DataService
{
    public DataManager CurrentDataManager { get; private set; }
    public Dictionary<EItem, ItemData> ItemData => CurrentDataManager.itemData;
    public Dictionary<EWeapon, WeaponData> WeaponData => CurrentDataManager.weapons;
    public Dictionary<ETome, TomeData> TomeData => CurrentDataManager.tomeData;
    public Dictionary<ECharacter, CharacterData> CharacterData => CurrentDataManager.characterData;
    public Dictionary<string, MyAchievement> AchievementData => CurrentDataManager.achievementsData;

    internal void SetDataManager()
    {
        CurrentDataManager = AlwaysManager.Instance.dataManager;
    }
}