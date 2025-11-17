using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Actors.Enemies;
using Assets.Scripts.Inventory__Items__Pickups.Chests;
using Assets.Scripts.Inventory__Items__Pickups.Interactables;
using BonkersLib.Core;
using BonkersLib.Enums;
using BonkersLib.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BonkersLib.Services.World;

public class WorldObjectCache
{
    private readonly Dictionary<WorldObjectTypeEnum, List<Component>> _cachedObjects = new();
    private readonly Dictionary<int, Component> _instanceIdLookup = new();

    public bool IsValid { get; private set; }

    internal void BuildCache()
    {
        if (!BonkersAPI.Game.IsInGame)
        {
            ModLogger.LogDebug("[WorldObjectCache] Cannot build cache - not in game");
            return;
        }

        _cachedObjects.Clear();
        _instanceIdLookup.Clear();

        CacheObjectType<ChargeShrine>(WorldObjectTypeEnum.ChargeShrine);
        CacheObjectType<InteractableShrineMoai>(WorldObjectTypeEnum.MoaiShrine);
        CacheObjectType<InteractableShrineCursed>(WorldObjectTypeEnum.CursedShrine);
        CacheObjectType<InteractableShrineGreed>(WorldObjectTypeEnum.GreedShrine);
        CacheObjectType<InteractableShrineMagnet>(WorldObjectTypeEnum.MagnetShrine);
        CacheObjectType<InteractableShrineChallenge>(WorldObjectTypeEnum.ChallengeShrine);

        CacheObjectType<InteractableChest>(WorldObjectTypeEnum.Chest);
        CacheObjectType<InteractableShadyGuy>(WorldObjectTypeEnum.ShadyGuy);
        CacheObjectType<InteractableMicrowave>(WorldObjectTypeEnum.Microwave);
        CacheObjectType<InteractableBossSpawner>(WorldObjectTypeEnum.BossSpawner);
        CacheObjectType<InteractableBossSpawnerFinal>(WorldObjectTypeEnum.BossSpawnerFinal);

        IsValid = true;

        var totalCached = _cachedObjects.Sum(x => x.Value.Count);
        ModLogger.LogDebug(
            $"[WorldObjectCache] Built cache with {totalCached} objects, {_instanceIdLookup.Count} in lookup");
    }

    internal void UpdateDynamicObjects()
    {
        CacheObjectType<OpenChest>(WorldObjectTypeEnum.OpenChest);
        CacheObjectType<Enemy>(WorldObjectTypeEnum.Enemy);

        var enemies = GetCachedObjects<Enemy>(WorldObjectTypeEnum.Enemy);
        var bosses = enemies.Where(e => e && e.IsBoss()).ToList();
        _cachedObjects[WorldObjectTypeEnum.Boss] = bosses.Cast<Component>().ToList();
    }

    internal void CleanupCompleted()
    {
        CleanupCompletedShrines<ChargeShrine>(
            WorldObjectTypeEnum.ChargeShrine,
            shrine => shrine.completed,
            "completed charge shrines"
        );

        CleanupCompletedShrines<InteractableShrineGreed>(
            WorldObjectTypeEnum.GreedShrine,
            shrine => shrine.done,
            "done greed shrines"
        );
    }

    private void CleanupCompletedShrines<T>(
        WorldObjectTypeEnum objectType,
        Func<T, bool> shouldRemove,
        string logName) where T : Component
    {
        if (!_cachedObjects.TryGetValue(objectType, out var objects))
            return;

        var toRemove = objects.Where(obj =>
        {
            if (!obj || !obj) return true;
            var typed = obj as T;
            return typed && shouldRemove(typed);
        }).ToList();

        foreach (var obj in toRemove)
        {
            objects.Remove(obj);
            if (obj && obj.gameObject) _instanceIdLookup.Remove(obj.gameObject.GetInstanceID());
        }

        if (toRemove.Count > 0) ModLogger.LogDebug($"[WorldObjectCache] Removed {toRemove.Count} {logName}");
    }

    internal void Invalidate()
    {
        _cachedObjects.Clear();
        _instanceIdLookup.Clear();
        IsValid = false;
    }

    private void CacheObjectType<T>(WorldObjectTypeEnum objectType) where T : Component
    {
        var objects = Object.FindObjectsOfType<T>()
            .Where(obj => obj && obj.gameObject)
            .Cast<Component>()
            .ToList();

        _cachedObjects[objectType] = objects;

        foreach (var obj in objects)
            if (obj && obj.gameObject)
                _instanceIdLookup[obj.gameObject.GetInstanceID()] = obj;
    }

    public T GetComponentByInstanceId<T>(int instanceId) where T : Component
    {
        if (_instanceIdLookup.TryGetValue(instanceId, out var component))
        {
            if (component && component) return component as T;

            _instanceIdLookup.Remove(instanceId);
        }

        return null;
    }

    public IEnumerable<T> GetCachedObjects<T>(WorldObjectTypeEnum objectType) where T : Component
    {
        if (_cachedObjects.TryGetValue(objectType, out var objects))
            return objects
                .Where(obj => obj && obj)
                .Cast<T>();

        return Enumerable.Empty<T>();
    }

    public Vector3? GetNearestObject(WorldObjectTypeEnum objectType, Vector3 referencePosition)
    {
        if (!_cachedObjects.TryGetValue(objectType, out var objects) || objects.Count == 0)
            return null;

        var nearest = objects
            .Where(obj => obj && obj)
            .OrderBy(obj => Vector3.Distance(referencePosition, obj.transform.position))
            .FirstOrDefault();

        return nearest?.transform.position;
    }
}