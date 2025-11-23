using Assets.Scripts.Saves___Serialization.Progression.Achievements;
using Assets.Scripts.Saves___Serialization.Progression.Unlocks;
using BonkersLib.Core;
using BonkersLib.Utils;
using Il2CppSystem.Collections.Generic;

namespace BonkersLib.Services;

public class AchievementService
{
    public Dictionary<string, MyAchievement> AllAchievements => BonkersAPI.Data.AchievementData;

    public void UnlockAll()
    {
        MainThreadDispatcher.Enqueue(() =>
        {
            var achievements = AllAchievements;

            if (achievements == null) return;

            foreach (var achievement in achievements)
            {
                MyAchievements.TryUnlock(achievement.Key);
            }

            ModLogger.LogDebug("Unlocked all achievements!");
        });
    }
}