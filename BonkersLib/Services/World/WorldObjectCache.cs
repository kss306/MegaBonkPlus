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
    private class SafeObjectData
    {
        public Component ComponentRef;
        public Vector3 Position;
        public int InstanceId;
    }

    private readonly Dictionary<WorldObjectTypeEnum, List<SafeObjectData>> _cachedObjects = new();
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

        RefreshCacheForType<ChargeShrine>(WorldObjectTypeEnum.ChargeShrine);
        RefreshCacheForType<InteractableShrineMoai>(WorldObjectTypeEnum.MoaiShrine);
        RefreshCacheForType<InteractableShrineCursed>(WorldObjectTypeEnum.CursedShrine);
        RefreshCacheForType<InteractableShrineGreed>(WorldObjectTypeEnum.GreedShrine);
        RefreshCacheForType<InteractableShrineMagnet>(WorldObjectTypeEnum.MagnetShrine);
        RefreshCacheForType<InteractableShrineChallenge>(WorldObjectTypeEnum.ChallengeShrine);

        RefreshCacheForType<InteractableChest>(WorldObjectTypeEnum.Chest);
        RefreshCacheForType<InteractableShadyGuy>(WorldObjectTypeEnum.ShadyGuy);
        RefreshCacheForType<InteractableMicrowave>(WorldObjectTypeEnum.Microwave);
        RefreshCacheForType<InteractableBossSpawner>(WorldObjectTypeEnum.BossSpawner);
        RefreshCacheForType<InteractableBossSpawnerFinal>(WorldObjectTypeEnum.BossSpawnerFinal);

        RefreshCacheForType<OpenChest>(WorldObjectTypeEnum.OpenChest);
        RefreshCacheForType<Enemy>(WorldObjectTypeEnum.Enemy);

        IsValid = true;
        ModLogger.LogDebug($"[WorldObjectCache] Built cache with {_cachedObjects.Count} categories");
    }

    internal void UpdateDynamicObjects()
    {
        RefreshCacheForType<OpenChest>(WorldObjectTypeEnum.OpenChest);
        RefreshCacheForType<Enemy>(WorldObjectTypeEnum.Enemy);

        if (_cachedObjects.TryGetValue(WorldObjectTypeEnum.Enemy, out var enemies))
        {
            var bosses = enemies
                .Where(e => e.ComponentRef && ((Enemy)e.ComponentRef).IsBoss())
                .ToList();
            _cachedObjects[WorldObjectTypeEnum.Boss] = bosses;
        }

        foreach (var list in _cachedObjects.Values)
        {
            foreach (var item in list)
            {
                if (item.ComponentRef)
                {
                    item.Position = item.ComponentRef.transform.position;
                }
            }
        }
    }

    internal void CleanupCompleted()
    {
        foreach (var key in _cachedObjects.Keys.ToList())
        {
            _cachedObjects[key].RemoveAll(x => x.ComponentRef == null);
        }

        CleanupSpecificShrines<ChargeShrine>(
            WorldObjectTypeEnum.ChargeShrine,
            s => s.completed,
            "completed charge shrines");

        CleanupSpecificShrines<InteractableShrineGreed>(
            WorldObjectTypeEnum.GreedShrine,
            s => s.done,
            "done greed shrines");

        var idsToRemove = _instanceIdLookup.Where(kvp => !kvp.Value).Select(kvp => kvp.Key).ToList();
        foreach (var id in idsToRemove) _instanceIdLookup.Remove(id);
    }

    internal void Invalidate()
    {
        IsValid = false;
        _cachedObjects.Clear();
        _instanceIdLookup.Clear();
    }

    private void RefreshCacheForType<T>(WorldObjectTypeEnum type) where T : Component
    {
        var unityObjects = Object.FindObjectsOfType<T>();
        var safeList = new List<SafeObjectData>();

        foreach (var obj in unityObjects)
        {
            if (!obj || !obj.gameObject) continue;
            var instanceId = obj.gameObject.GetInstanceID();

            safeList.Add(new SafeObjectData
            {
                ComponentRef = obj,
                Position = obj.transform.position,
                InstanceId = instanceId
            });

            _instanceIdLookup[instanceId] = obj;
        }

        _cachedObjects[type] = safeList;
    }

    private void CleanupSpecificShrines<T>(WorldObjectTypeEnum type, Func<T, bool> isCompletedCondition, string logName)
        where T : Component
    {
        if (!_cachedObjects.TryGetValue(type, out var list)) return;

        var toRemove = list.Where(safeObj =>
        {
            if (!safeObj.ComponentRef) return true;

            var typedObj = safeObj.ComponentRef as T;
            return typedObj && isCompletedCondition(typedObj);
        }).ToList();

        foreach (var item in toRemove)
        {
            list.Remove(item);
            _instanceIdLookup.Remove(item.InstanceId);
        }

        if (toRemove.Count > 0)
            ModLogger.LogDebug($"[WorldObjectCache] Removed {toRemove.Count} {logName}");
    }

    public T GetComponentByInstanceId<T>(int instanceId) where T : Component
    {
        if (!_instanceIdLookup.TryGetValue(instanceId, out var component)) return null;
        if (component) return component as T;
        _instanceIdLookup.Remove(instanceId);
        return null;
    }

    public IEnumerable<T> GetCachedObjects<T>(WorldObjectTypeEnum objectType) where T : Component
    {
        if (_cachedObjects.TryGetValue(objectType, out var safeObjects))
        {
            return safeObjects
                .Where(x => x.ComponentRef)
                .Select(x => x.ComponentRef as T);
        }

        return Enumerable.Empty<T>();
    }

    public Vector3? GetNearestObject(WorldObjectTypeEnum objectType, Vector3 referencePosition)
    {
        if (!_cachedObjects.TryGetValue(objectType, out var objects) || objects.Count == 0)
            return null;

        var nearest = objects
            .Where(obj => obj.ComponentRef)
            .OrderBy(obj => Vector3.Distance(referencePosition, obj.Position))
            .FirstOrDefault();

        return nearest?.Position;
    }
}