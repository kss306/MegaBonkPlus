using System.Collections.Generic;
using BepInEx.Logging;
using MegaBonkPlusMod.Models;
using Object = UnityEngine.Object;

namespace MegaBonkPlusMod.GameLogic.Trackers;

public class MicrowaveTracker : BaseTracker
{
    public MicrowaveTracker(ManualLogSource logger, float scanIntervalInSeconds) : base(logger,
        scanIntervalInSeconds)
    {
    }

    public override string ApiRoute => "/api/tracker/microwave";

    protected override object BuildDataPayload()
    {
        var trackedObjects = new List<TrackedObjectData>();
        var allObjects = Object.FindObjectsOfType<InteractableMicrowave>();
        
        foreach (var trackedObject in allObjects)   
        {
            CacheIconsForObject(trackedObject.transform.parent);
            var objectData = new TrackedObjectData
            {
                Position = PositionData.FromVector3(trackedObject.transform.position),
                InstanceId = trackedObject.gameObject.GetInstanceID()
            };
            objectData.CustomProperties["rarity"] = trackedObject.rarity.ToString();
            trackedObjects.Add(objectData);
        }

        return new ApiListResponse<TrackedObjectData>(trackedObjects);
    }
}