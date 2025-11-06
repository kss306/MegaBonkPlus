using System.Collections.Generic;
using BepInEx.Logging;
using MegaBonkPlusMod.Models;
using Object = UnityEngine.Object;

namespace MegaBonkPlusMod.GameLogic.Trackers;

public class MaoiShrineTracker : BaseTracker
{
    public MaoiShrineTracker(ManualLogSource logger, float scanIntervalInSeconds) : base(logger, scanIntervalInSeconds)
    {
    }

    public override string ApiRoute => "/api/tracker/shrines/maoi";

    protected override object BuildDataPayload()
    {
        var trackedMaoiShrines = new List<TrackedObjectData>();
        var allMaoiShrines = Object.FindObjectsOfType<InteractableShrineMoai>();

        foreach (var maoiShrine in allMaoiShrines)
        {
            var shrineData = new TrackedObjectData
            {
                Position = PositionData.FromVector3(maoiShrine.transform.position)
            };
            trackedMaoiShrines.Add(shrineData);
        }

        return new ApiListResponse<TrackedObjectData>(trackedMaoiShrines);
    }
}