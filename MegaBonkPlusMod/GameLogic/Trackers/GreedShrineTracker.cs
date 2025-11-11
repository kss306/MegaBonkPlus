using System.Collections.Generic;
using BepInEx.Logging;
using BonkersLib.Core;
using BonkersLib.Services;
using BonkersLib.Utils;
using MegaBonkPlusMod.Models;

namespace MegaBonkPlusMod.GameLogic.Trackers;

public class GreedShrineTracker : BaseTracker
{
    public GreedShrineTracker(ManualLogSource logger, float scanIntervalInSeconds) : base(logger, scanIntervalInSeconds)
    {
    }

    public override string ApiRoute => "/api/tracker/shrines/greed";

    protected override object BuildDataPayload()
    {
        var trackedObjects = new List<TrackedObjectData>();
        
        if (!BonkersAPI.Game.IsInGame)
            return new ApiListResponse<TrackedObjectData>(trackedObjects);
        
        WorldService world = BonkersAPI.World;
        
        var allObjects = world.GetGreedShrines();

        foreach (var trackedObject in allObjects)
        {
            CacheIconsForObject(GameObjectUtils.FindMinimapIcon(trackedObject.transform));
            var objectData = new TrackedObjectData
            {
                Position = PositionData.FromVector3(trackedObject.transform.position),
                InstanceId = trackedObject.gameObject.GetInstanceID()
            };
            objectData.CustomProperties["done"] = trackedObject.done;
            trackedObjects.Add(objectData);
        }

        return new ApiListResponse<TrackedObjectData>(trackedObjects);
    }
}