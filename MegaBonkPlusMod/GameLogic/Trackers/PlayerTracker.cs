using System;
using System.Collections.Generic;
using Assets.Scripts.Actors.Player;
using Assets.Scripts.Menu.Shop;
using BepInEx.Logging;
using MegaBonkPlusMod.Models;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MegaBonkPlusMod.GameLogic.Trackers
{
    public class PlayerTracker : BaseTracker
    {
        private MyPlayer _player;
        private Transform _playerTransform;
        private GameManager _gameManager;

        public PlayerTracker(ManualLogSource logger, float scanIntervalInSeconds) : base(logger, scanIntervalInSeconds)
        {
            Logger.LogInfo("PlayerTracker (Echtzeit, Super-Slim) initialisiert.");
        }

        public override string ApiRoute => "/api/tracker/player";

        protected override object BuildDataPayload()
        {
            var trackedObjects = new List<TrackedObjectData>();
            if (!_playerTransform)
            {
                _player = Object.FindObjectOfType<MyPlayer>();
                if (_player)
                {
                    _playerTransform = _player.transform;
                }
            }

            if (!_gameManager)
            {
                _gameManager = Object.FindObjectOfType<GameManager>();
            }

            if (_playerTransform)
            {
                var playerData = new TrackedObjectData
                {
                    InstanceId = _player.gameObject.GetInstanceID(), 
                    Position = PositionData.FromVector3(_playerTransform.position)
                };

                try
                {
                    var stats = _player.inventory?.playerStats?.stats;
                    var playerHealth = _player.inventory?.playerHealth;
                    var playerXp = _player.inventory?.playerXp;

                    if (playerXp != null) playerData.CustomProperties["level"] = playerXp.level;

                    if (stats != null && playerHealth != null)
                    {
                        var statsForFrontend = new Dictionary<string, object>();

                        statsForFrontend["HP"] = $"{playerHealth.hp} / {playerHealth.maxHp}";
                        statsForFrontend["Shield"] = $"{(int)playerHealth.shield} / {(int)playerHealth.maxShield}";

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
                    }
                    else
                    {
                        Logger.LogWarning("PlayerTracker: 'rawStats' konnte nicht gefunden werden (Pfad ist null).");
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError($"PlayerTracker: Fehler beim Lesen der Stats: {ex.Message}");
                }

                playerData.CustomProperties["character"] = _player.character.ToString();
                
                if (_gameManager)
                {
                    playerData.CustomProperties["stageTime"] = _gameManager.totalStageTime;
                    playerData.CustomProperties["timeAlive"] = _gameManager.GetAliveTime();
                    playerData.CustomProperties["bossCurses"] = _gameManager.bossCurses;

                    trackedObjects.Add(playerData);
                }

                trackedObjects.Add(playerData);
            }


            return new ApiListResponse<TrackedObjectData>(trackedObjects);
        }

        protected override void OnTrackerError(Exception ex)
        {
            Logger.LogWarning($"PlayerTracker-Fehler: {ex.Message}");
            _playerTransform = null;
            _player = null;
        }

        private static string AddPercentStat(float value, float multiplier = 100)
        {
            float percent = value * multiplier;
            float rounded = (float)Math.Round(percent);
            return rounded + "%";
        }

        private static string AddTimesStat(float value, float multiplier = 1)
        {
            float times = value * multiplier;
            float rounded = (float)Math.Round(times, 1);
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
}