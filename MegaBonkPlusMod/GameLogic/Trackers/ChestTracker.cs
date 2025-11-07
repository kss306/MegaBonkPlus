using System.Collections.Generic;
using Assets.Scripts.Inventory__Items__Pickups.Chests;
using Assets.Scripts.Inventory__Items__Pickups.Interactables;
using BepInEx.Logging;
using MegaBonkPlusMod.Models;
using UnityEngine;

namespace MegaBonkPlusMod.GameLogic.Trackers;

public class ChestTracker : BaseTracker
{
    public ChestTracker(ManualLogSource logger, float scanIntervalInSeconds) : base(logger, scanIntervalInSeconds)
    {
        Logger.LogInfo("ChestTracker (Intervall: 2s, Super-Slim) initialisiert.");
    }

    public override string ApiRoute => "/api/tracker/chests";

    protected override object BuildDataPayload()
    {
        var trackedChests = new List<TrackedObjectData>();
        var allChests = Object.FindObjectsOfType<InteractableChest>();
        var allOpenChests = Object.FindObjectsOfType<OpenChest>();
        
        foreach (var chest in allChests)
        {
            CacheIconsForObject(chest.transform);
            var chestData = new TrackedObjectData
            {
                Position = PositionData.FromVector3(chest.transform.position),
                InstanceId = chest.gameObject.GetInstanceID()
            };
            chestData.CustomProperties["type"] = chest.chestType.ToString();
            trackedChests.Add(chestData);
        }
        
        foreach (var chest in allOpenChests)
        {
            CacheIconsForObject(chest.transform);
            var chestData = new TrackedObjectData
            {
                Position = PositionData.FromVector3(chest.transform.position),
                InstanceId = chest.gameObject.GetInstanceID()
            };
            chestData.CustomProperties["type"] = "Open";
            trackedChests.Add(chestData);
        }

        return new ApiListResponse<TrackedObjectData>(trackedChests);
    }
}