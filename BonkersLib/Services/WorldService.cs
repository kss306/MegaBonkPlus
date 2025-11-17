using System.Collections.Generic;
using Assets.Scripts.Actors.Enemies;
using Assets.Scripts.Inventory__Items__Pickups.Chests;
using Assets.Scripts.Inventory__Items__Pickups.Interactables;
using Assets.Scripts.Managers;
using BonkersLib.Core;
using BonkersLib.Enums;
using BonkersLib.Services.World;
using BonkersLib.Utils;
using UnityEngine;

namespace BonkersLib.Services;

public class WorldService
{
    private readonly WorldObjectCache _cache;

    internal WorldService()
    {
        _cache = new WorldObjectCache();
    }

    private EnemyManager _enemyManager => BonkersAPI.Game.EnemyManagerInstance;
    public bool AreEnemiesSpawning => _enemyManager?.enabledWaves ?? false;

    internal void Update()
    {
        if (BonkersAPI.Game.IsInGame && _cache.IsValid)
        {
            _cache.UpdateDynamicObjects();
            _cache.CleanupCompleted();
        }
    }

    internal void OnSceneChanged()
    {
        ModLogger.LogDebug("[WorldService] Scene changed, invalidating cache");
        _cache.Invalidate();
    }

    internal void OnGameStarted()
    {
        ModLogger.LogDebug("[WorldService] Game started, building cache");
        _cache.BuildCache();
    }

    public T GetComponentByInstanceId<T>(int instanceId) where T : Component
    {
        EnsureCacheValid();
        return _cache.GetComponentByInstanceId<T>(instanceId);
    }

    public IEnumerable<ChargeShrine> GetChargeShrines()
    {
        return GetCachedObjects<ChargeShrine>(WorldObjectTypeEnum.ChargeShrine);
    }

    public IEnumerable<InteractableShrineMoai> GetMoaiShrines()
    {
        return GetCachedObjects<InteractableShrineMoai>(WorldObjectTypeEnum.MoaiShrine);
    }

    public IEnumerable<InteractableShrineCursed> GetCursedShrines()
    {
        return GetCachedObjects<InteractableShrineCursed>(WorldObjectTypeEnum.CursedShrine);
    }

    public IEnumerable<InteractableShrineGreed> GetGreedShrines()
    {
        return GetCachedObjects<InteractableShrineGreed>(WorldObjectTypeEnum.GreedShrine);
    }

    public IEnumerable<InteractableShrineMagnet> GetMagnetShrines()
    {
        return GetCachedObjects<InteractableShrineMagnet>(WorldObjectTypeEnum.MagnetShrine);
    }

    public IEnumerable<InteractableShrineChallenge> GetChallengeShrines()
    {
        return GetCachedObjects<InteractableShrineChallenge>(WorldObjectTypeEnum.ChallengeShrine);
    }

    public IEnumerable<InteractableChest> GetChests()
    {
        return GetCachedObjects<InteractableChest>(WorldObjectTypeEnum.Chest);
    }

    public IEnumerable<OpenChest> GetOpenChests()
    {
        return GetCachedObjects<OpenChest>(WorldObjectTypeEnum.OpenChest);
    }

    public IEnumerable<InteractableShadyGuy> GetShadyGuys()
    {
        return GetCachedObjects<InteractableShadyGuy>(WorldObjectTypeEnum.ShadyGuy);
    }

    public IEnumerable<InteractableMicrowave> GetMicrowaves()
    {
        return GetCachedObjects<InteractableMicrowave>(WorldObjectTypeEnum.Microwave);
    }

    public IEnumerable<InteractableBossSpawner> GetBossSpawner()
    {
        return GetCachedObjects<InteractableBossSpawner>(WorldObjectTypeEnum.BossSpawner);
    }

    public IEnumerable<InteractableBossSpawnerFinal> GetBossSpawnerFinal()
    {
        return GetCachedObjects<InteractableBossSpawnerFinal>(WorldObjectTypeEnum.BossSpawnerFinal);
    }

    public IEnumerable<Enemy> GetBosses()
    {
        return GetCachedObjects<Enemy>(WorldObjectTypeEnum.Boss);
    }

    public Vector3? GetNearestChargeShrine()
    {
        return GetNearestObject(WorldObjectTypeEnum.ChargeShrine);
    }

    public Vector3? GetNearestMoaiShrine()
    {
        return GetNearestObject(WorldObjectTypeEnum.MoaiShrine);
    }

    public Vector3? GetNearestCursedShrine()
    {
        return GetNearestObject(WorldObjectTypeEnum.CursedShrine);
    }

    public Vector3? GetNearestGreedShrine()
    {
        return GetNearestObject(WorldObjectTypeEnum.GreedShrine);
    }

    public Vector3? GetNearestMagnetShrine()
    {
        return GetNearestObject(WorldObjectTypeEnum.MagnetShrine);
    }

    public Vector3? GetNearestChallengeShrine()
    {
        return GetNearestObject(WorldObjectTypeEnum.ChallengeShrine);
    }

    public Vector3? GetNearestChest()
    {
        return GetNearestObject(WorldObjectTypeEnum.Chest);
    }

    public Vector3? GetNearestOpenChest()
    {
        return GetNearestObject(WorldObjectTypeEnum.OpenChest);
    }

    public Vector3? GetNearestShadyGuy()
    {
        return GetNearestObject(WorldObjectTypeEnum.ShadyGuy);
    }

    public Vector3? GetNearestMicrowave()
    {
        return GetNearestObject(WorldObjectTypeEnum.Microwave);
    }

    public Vector3? GetNearestBossSpawner()
    {
        return GetNearestObject(WorldObjectTypeEnum.BossSpawner);
    }

    public Vector3? GetNearestBossSpawnerFinal()
    {
        return GetNearestObject(WorldObjectTypeEnum.BossSpawnerFinal);
    }

    public void KillAllEnemies()
    {
        if (!BonkersAPI.Game.IsInGame) return;

        var allEnemies = GetCachedObjects<Enemy>(WorldObjectTypeEnum.Enemy);
        foreach (var enemy in allEnemies)
            if (enemy)
                enemy.DiedNextFrame();
    }

    public void ToggleEnemySpawns()
    {
        if (!_enemyManager) return;

        _enemyManager.enabledWaves = !AreEnemiesSpawning;
        ModLogger.LogDebug($"[WorldService] Toggled enemy spawns to {AreEnemiesSpawning}");
    }

    public List<ItemData> GetEveryShadyItem()
    {
        var shadyGuys = GetShadyGuys();
        var items = new List<ItemData>();

        foreach (var shadyGuy in shadyGuys)
            if (shadyGuy && shadyGuy.items != null)
                foreach (var item in shadyGuy.items)
                    if (item)
                        items.Add(item);

        return items;
    }

    public IEnumerable<T> GetCachedObjects<T>(WorldObjectTypeEnum objectType) where T : Component
    {
        EnsureCacheValid();
        return _cache.GetCachedObjects<T>(objectType);
    }

    private Vector3? GetNearestObject(WorldObjectTypeEnum objectType)
    {
        EnsureCacheValid();
        return _cache.GetNearestObject(objectType, BonkersAPI.Player.Position);
    }

    private void EnsureCacheValid()
    {
        if (!_cache.IsValid && BonkersAPI.Game.IsInGame)
        {
            ModLogger.LogDebug("[WorldService] Cache invalid, rebuilding...");
            _cache.BuildCache();
        }
    }
}