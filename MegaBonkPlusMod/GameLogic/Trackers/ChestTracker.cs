using System.Collections.Generic;
using Assets.Scripts.Inventory__Items__Pickups.Chests;
using Assets.Scripts.Inventory__Items__Pickups.Interactables;
using BepInEx.Logging;
using BonkersLib.Core;
using BonkersLib.Services;
using BonkersLib.Utils;
using MegaBonkPlusMod.Models;
using UnityEngine;

namespace MegaBonkPlusMod.GameLogic.Trackers;

public class ChestTracker : BaseTracker
{
    public ChestTracker(float scanIntervalInSeconds) : base(scanIntervalInSeconds)
    {
    }

    public override string ApiRoute => "/api/tracker/chests";

    protected override object BuildDataPayload()
    {
        var trackedObjects = new List<TrackedObjectDataModel>();
        
        if (!BonkersAPI.Game.IsInGame)
            return new ApiListResponseModel<TrackedObjectDataModel>(trackedObjects);
        
        WorldService world = BonkersAPI.World;
        
        var allChests = world.GetChests();
        var allOpenChests = world.GetOpenChests();
        
        foreach (var chest in allChests)
        {
            CacheIconsForObject(GameObjectUtils.FindMinimapIcon(chest.transform));
            var chestData = new TrackedObjectDataModel
            {
                Position = PositionDataModel.FromVector3(chest.transform.position),
                InstanceId = chest.gameObject.GetInstanceID()
            };
            chestData.CustomProperties["type"] = chest.chestType.ToString();
            trackedObjects.Add(chestData);
        }
        
        foreach (var chest in allOpenChests)
        {
            CacheIconsForObject(GameObjectUtils.FindMinimapIcon(chest.transform));
            var chestData = new TrackedObjectDataModel
            {
                Position = PositionDataModel.FromVector3(chest.transform.position),
                InstanceId = chest.gameObject.GetInstanceID()
            };
            chestData.CustomProperties["type"] = "Open";
            trackedObjects.Add(chestData);
        }

        return new ApiListResponseModel<TrackedObjectDataModel>(trackedObjects);
    }
}