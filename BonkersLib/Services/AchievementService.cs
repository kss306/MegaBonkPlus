using Assets.Scripts.Saves___Serialization.Progression.Achievements;
using Assets.Scripts.Saves___Serialization.Progression.Unlocks;
using BonkersLib.Core;
using BonkersLib.Utils;
using Il2CppSystem.Collections.Generic;
using Il2CppSystem.Globalization;

namespace BonkersLib.Services;

public class AchievementService
{
    public Dictionary<string, MyAchievement> AllAchievements => BonkersAPI.Data.AchievementData;

    public void UnlockAll()
    {
        foreach (var achievemnt in AllAchievements)
            MyAchievements.TryUnlock(achievemnt.Key);
        
        ModLogger.LogDebug("Unlocked all achievements!");
    }
}