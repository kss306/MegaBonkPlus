using System.Collections.Generic;
using BepInEx.Logging;
using MegaBonkPlusMod.Models;
using Object = UnityEngine.Object;

namespace MegaBonkPlusMod.GameLogic.Trackers;

public class CursedShrineTracker : BaseTracker
{
    public CursedShrineTracker(ManualLogSource logger, float scanIntervalInSeconds) : base(logger,
        scanIntervalInSeconds)
    {
    }

    public override string ApiRoute => "/api/tracker/shrines/cursed";

    protected override object BuildDataPayload()
    {
        var trackedObjects = new List<TrackedObjectData>();
        var allObjects = Object.FindObjectsOfType<InteractableShrineCursed>();
        
        foreach (var trackedObject in allObjects)
        {
            CacheIconsForObject(trackedObject.transform);
            var objectData = new TrackedObjectData
            {
                Position = PositionData.FromVector3(trackedObject.transform.position)
            };
            trackedObjects.Add(objectData);
        }

        return new ApiListResponse<TrackedObjectData>(trackedObjects);
    }
}