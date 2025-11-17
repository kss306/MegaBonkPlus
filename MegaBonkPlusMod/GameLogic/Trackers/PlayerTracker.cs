using System;
using System.Collections.Generic;
using Assets.Scripts.Menu.Shop;
using BonkersLib.Core;
using MegaBonkPlusMod.GameLogic.Trackers.Base;
using MegaBonkPlusMod.Models;
using MegaBonkPlusMod.Utils;

namespace MegaBonkPlusMod.GameLogic.Trackers;

public class PlayerTracker : BaseTracker
{
    public PlayerTracker(float scanIntervalInSeconds) : base(scanIntervalInSeconds)
    {
    }

    protected override object BuildDataPayload()
    {
        var trackedObjects = new List<TrackedObjectDataModel>();

        if (!BonkersAPI.Game.IsInGame)
            return new ApiListResponseModel<TrackedObjectDataModel>(trackedObjects);

        var player = BonkersAPI.Player;
        var playerData = new TrackedObjectDataModel
        {
            InstanceId = player.InstanceId,
            Position = PositionDataModel.FromVector3(player.Position)
        };

        playerData.CustomProperties["character"] = player.CharacterName;

        var stats = player.StatsDict;

        playerData.CustomProperties["level"] = player.Level;

        var statsForFrontend = new Dictionary<string, object>();

        statsForFrontend["HP"] = $"{player.Health} / {player.MaxHealth}";
        statsForFrontend["Shield"] = $"{(int)player.Shield} / {(int)player.MaxShield}";

        statsForFrontend["HP Regen"] = stats[EStat.HealthRegen];
        statsForFrontend["Overheal"] = AddTimesStat(stats[EStat.Overheal]);
        statsForFrontend["Armor"] = AddPercentStat(stats[EStat.Armor]);
        statsForFrontend["Evasion"] = AddPercentStat(stats[EStat.Evasion]);
        statsForFrontend["Lifesteal"] = AddPercentStat(stats[EStat.Lifesteal]);
        statsForFrontend["Thorns"] = RoundInt(stats[EStat.Thorns]);

        statsForFrontend["Damage"] = AddTimesStat(stats[EStat.DamageMultiplier]);
        statsForFrontend["Crit Chance"] = AddPercentStat(stats[EStat.CritChance]);
        statsForFrontend["Crit Damage"] = AddTimesStat(stats[EStat.CritDamage], 2);
        statsForFrontend["Attack Speed"] = AddPercentStat(stats[EStat.AttackSpeed]);
        statsForFrontend["Projectile Count"] = RoundDownInt(stats[EStat.Projectiles]);
        statsForFrontend["Projectile Bounces"] = stats[EStat.ProjectileBounces];
        statsForFrontend["Evasion"] = AddPercentStat(stats[EStat.Evasion]);

        statsForFrontend["Size"] = AddTimesStat(stats[EStat.SizeMultiplier]);
        statsForFrontend["Projectile Speed"] = AddTimesStat(stats[EStat.ProjectileSpeedMultiplier]);
        statsForFrontend["Duration"] = AddTimesStat(stats[EStat.DurationMultiplier]);
        statsForFrontend["Damage to Elites"] = AddTimesStat(stats[EStat.EliteDamageMultiplier]);
        statsForFrontend["Knockback"] = AddTimesStat(stats[EStat.KnockbackMultiplier]);
        statsForFrontend["Movement Speed"] = AddTimesStat(stats[EStat.MoveSpeedMultiplier]);

        statsForFrontend["Extra Jumps"] = RoundInt(stats[EStat.ExtraJumps]);
        statsForFrontend["Jump Height"] = RoundInt(stats[EStat.JumpHeight]);
        statsForFrontend["Luck"] = AddPercentStat(stats[EStat.Luck]);
        statsForFrontend["Difficulty"] = AddPercentStat(stats[EStat.Difficulty]);

        statsForFrontend["Pickup Range"] = RoundInt(stats[EStat.PickupRange]);
        statsForFrontend["XP Gain"] = AddTimesStat(stats[EStat.XpIncreaseMultiplier]);
        statsForFrontend["Gold Gain"] = AddTimesStat(stats[EStat.GoldIncreaseMultiplier]);
        statsForFrontend["Silver Gain"] = AddTimesStat(stats[EStat.SilverIncreaseMultiplier]);
        statsForFrontend["Elite Spawn Increase"] = AddTimesStat(stats[EStat.EliteSpawnIncrease]);
        statsForFrontend["Powerup Multiplier"] = AddTimesStat(stats[EStat.PowerupBoostMultiplier]);
        statsForFrontend["Powerup Drop Chance"] = AddTimesStat(stats[EStat.PowerupChance]);

        playerData.CustomProperties["stats"] = statsForFrontend;

        trackedObjects.Add(playerData);

        return new ApiListResponseModel<TrackedObjectDataModel>(trackedObjects);
    }

    protected override void OnTrackerError(Exception ex)
    {
        ModLogger.LogDebug($"[PlayerTracker] Error: {ex.Message}");
    }

    private static string AddPercentStat(float value, float multiplier = 100)
    {
        var percent = value * multiplier;
        var rounded = (float)Math.Round(percent);
        return rounded + "%";
    }

    private static string AddTimesStat(float value, float multiplier = 1)
    {
        var times = value * multiplier;
        var rounded = (float)Math.Round(times, 1);
        return rounded.ToString("F1") + "x";
    }

    private static int RoundInt(float value)
    {
        return (int)Math.Round(value);
    }

    private static int RoundDownInt(float value)
    {
        return (int)Math.Floor(value);
    }
}