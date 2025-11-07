using System.Collections.Generic;
using BepInEx.Logging;
using MegaBonkPlusMod.Models;
using Object = UnityEngine.Object;

namespace MegaBonkPlusMod.GameLogic.Trackers;

public class ChargeShrineTracker : BaseTracker
{
    public ChargeShrineTracker(ManualLogSource logger, float scanIntervalInSeconds) : base(logger,
        scanIntervalInSeconds)
    {
    }

    public override string ApiRoute => "/api/tracker/shrines/charge";

    protected override object BuildDataPayload()
    {
        var trackedChargeShrines = new List<TrackedObjectData>();
        var allChargeShrines = Object.FindObjectsOfType<ChargeShrine>();
        
        foreach (var chargeShrine in allChargeShrines)
        {
            CacheIconsForObject(chargeShrine.transform);
            var shrineData = new TrackedObjectData
            {
                Position = PositionData.FromVector3(chargeShrine.transform.position),
                InstanceId = chargeShrine.gameObject.GetInstanceID(), 
            };
            shrineData.CustomProperties["completed"] = chargeShrine.completed;
            shrineData.CustomProperties["isGolden"] = chargeShrine.isGolden;
            trackedChargeShrines.Add(shrineData);
        }

        return new ApiListResponse<TrackedObjectData>(trackedChargeShrines);
    }
}