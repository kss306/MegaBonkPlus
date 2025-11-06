using System.Collections.Generic;
using BepInEx.Logging;
using MegaBonkPlusMod.Models;
using Object = UnityEngine.Object;

namespace MegaBonkPlusMod.GameLogic.Trackers;

public class MagnetShrineTracker : BaseTracker
{
    public MagnetShrineTracker(ManualLogSource logger, float scanIntervalInSeconds) : base(logger,
        scanIntervalInSeconds)
    {
    }

    public override string ApiRoute => "/api/tracker/shrines/magnet";

    protected override object BuildDataPayload()
    {
        var trackedObjects = new List<TrackedObjectData>();
        var allObjects = Object.FindObjectsOfType<InteractableShrineMagnet>();

        foreach (var trackedObject in allObjects)
        {
            var objectData = new TrackedObjectData
            {
                Position = PositionData.FromVector3(trackedObject.transform.position)
            };
            trackedObjects.Add(objectData);
        }

        return new ApiListResponse<TrackedObjectData>(trackedObjects);
    }
}