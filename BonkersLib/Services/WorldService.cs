using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Actors.Enemies;
using Assets.Scripts.Inventory__Items__Pickups.Chests;
using Assets.Scripts.Inventory__Items__Pickups.Interactables;
using Assets.Scripts.Inventory__Items__Pickups.Items;
using Assets.Scripts.Managers;
using BepInEx.Logging;
using BonkersLib.Core;
using UnityEngine;

namespace BonkersLib.Services;

public class WorldService
{
    private readonly ManualLogSource _log;

    internal WorldService(ManualLogSource log)
    {
        _log = log;
    }

    internal void Update()
    {
    }

    private EnemyManager _enemyManager => BonkersAPI.Game.EnemyManagerInstance;
    
    public bool AreEnemiesSpawning => _enemyManager?.enabledWaves ?? false;

    public IEnumerable<T> FindInteractables<T>() where T : Component
    {
        if (!BonkersAPI.Game.IsInGame) return Enumerable.Empty<T>();
        return Object.FindObjectsOfType<T>();
    }

    public IEnumerable<InteractableChest> GetChests()
    {
        return FindInteractables<InteractableChest>();
    }

    public IEnumerable<InteractableBossSpawner> GetBossSpawner()
    {
        return FindInteractables<InteractableBossSpawner>();
    }

    public IEnumerable<InteractableBossSpawnerFinal> GetBossSpawnerFinal()
    {
        return FindInteractables<InteractableBossSpawnerFinal>();
    }

    public IEnumerable<InteractableShrineChallenge> GetChallengeShrines()
    {
        return FindInteractables<InteractableShrineChallenge>();
    }

    public IEnumerable<InteractableShrineCursed> GetCursedShrines()
    {
        return FindInteractables<InteractableShrineCursed>();
    }

    public IEnumerable<InteractableShrineGreed> GetGreedShrines()
    {
        return FindInteractables<InteractableShrineGreed>();
    }

    public IEnumerable<InteractableShrineMagnet> GetMagnetShrines()
    {
        return FindInteractables<InteractableShrineMagnet>();
    }

    public IEnumerable<InteractableMicrowave> GetMicrowaves()
    {
        return FindInteractables<InteractableMicrowave>();
    }

    public IEnumerable<InteractableShadyGuy> GetShadyGuys()
    {
        return FindInteractables<InteractableShadyGuy>();
    }

    public IEnumerable<InteractableShrineMoai> GetMoaiShrines()
    {
        return FindInteractables<InteractableShrineMoai>();
    }

    public IEnumerable<ChargeShrine> GetChargeShrines()
    {
        return FindInteractables<ChargeShrine>();
    }

    public IEnumerable<OpenChest> GetOpenChests()
    {
        return FindInteractables<OpenChest>();
    }

    public void KillAllEnemies()
    {
        if (!BonkersAPI.Game.IsInGame) return;

        var allEnemies = Object.FindObjectsOfType<Enemy>();

        foreach (var enemy in allEnemies)
        {
            if (enemy)
            {
                enemy.DiedNextFrame();
            }
        }
    }

    public void ToggleEnemySpawns()
    {
        _enemyManager.enabledWaves = !AreEnemiesSpawning;
    }

    public List<ItemData> GetEveryShadyItem()
    {
        var shadyGuys = GetShadyGuys();
        var items = new List<ItemData>();
        foreach (var shadyGuy in shadyGuys)
        {
            foreach (var item in shadyGuy.items)
            {
                items.Add(item);
            }
        }
        return items;
    }
}