using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Inventory__Items__Pickups.Chests;
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

        void CacheObjectType<T>(WorldObjectTypeEnum type) where T : Component
        {
            var objects = Object.FindObjectsOfType<T>();
            var safeList = new List<SafeObjectData>();

            foreach (var obj in objects)
            {
                if (obj)
                {
                    safeList.Add(new SafeObjectData
                    {
                        ComponentRef = obj,
                        Position = obj.transform.position,
                        InstanceId = obj.gameObject.GetInstanceID()
                    });

                    _instanceIdLookup[obj.gameObject.GetInstanceID()] = obj;
                }
            }

            _cachedObjects[type] = safeList;
        }

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
        ModLogger.LogDebug($"[WorldObjectCache] Built cache with {_cachedObjects.Count} categories");
    }

    internal void UpdateDynamicObjects()
    {
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
            _cachedObjects[key].RemoveAll(x => !x.ComponentRef);
        }

        var idsToRemove = _instanceIdLookup.Where(kvp => !kvp.Value).Select(kvp => kvp.Key).ToList();
        foreach (var id in idsToRemove) _instanceIdLookup.Remove(id);
    }

    internal void Invalidate()
    {
        IsValid = false;
        _cachedObjects.Clear();
        _instanceIdLookup.Clear();
    }

    public T GetComponentByInstanceId<T>(int instanceId) where T : Component
    {
        if (_instanceIdLookup.TryGetValue(instanceId, out var component))
        {
            if (component) return component as T;
            _instanceIdLookup.Remove(instanceId);
        }

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