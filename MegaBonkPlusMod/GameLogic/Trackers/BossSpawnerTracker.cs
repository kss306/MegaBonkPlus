using System.Collections.Generic;
using BepInEx.Logging;
using MegaBonkPlusMod.Models;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MegaBonkPlusMod.GameLogic.Trackers;

public class BossSpawnerTracker : BaseTracker
{
    public BossSpawnerTracker(ManualLogSource logger, float scanIntervalInSeconds) : base(logger, scanIntervalInSeconds)
    {
    }

    public override string ApiRoute => "/api/tracker/bossspawner";

    protected override object BuildDataPayload()
    {
        var trackedSpawner = new List<TrackedObjectData>();

        Component bossSpawner = null;

        bossSpawner = Object.FindObjectOfType<InteractableBossSpawner>();

        if (!bossSpawner) bossSpawner = Object.FindObjectOfType<InteractableBossSpawnerFinal>();

        if (bossSpawner)
        {
            CacheIconsForObject(bossSpawner.transform);
            var bossSpawnerData = new TrackedObjectData
            {
                Position = PositionData.FromVector3(bossSpawner.transform.position),
                InstanceId = bossSpawner.gameObject.GetInstanceID(), 
            };
            trackedSpawner.Add(bossSpawnerData);
        }

        return new ApiListResponse<TrackedObjectData>(trackedSpawner);
    }
}