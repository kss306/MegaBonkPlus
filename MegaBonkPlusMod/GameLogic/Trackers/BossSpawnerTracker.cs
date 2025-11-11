

using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using BonkersLib.Core;
using BonkersLib.Services;
using MegaBonkPlusMod.Models;
using UnityEngine;
using BonkersLib.Utils;

namespace MegaBonkPlusMod.GameLogic.Trackers;

public class BossSpawnerTracker : BaseTracker
{
    public BossSpawnerTracker(ManualLogSource logger, float scanIntervalInSeconds) : base(logger, scanIntervalInSeconds)
    {
    }

    public override string ApiRoute => "/api/tracker/bossspawner";

    protected override object BuildDataPayload()
    {
        var trackedObjects = new List<TrackedObjectData>();
        
        if (!BonkersAPI.Game.IsInGame)
            return new ApiListResponse<TrackedObjectData>(trackedObjects);
        
        WorldService world = BonkersAPI.World;
        
        Component bossSpawner = world.GetBossSpawner().FirstOrDefault();

        if (!bossSpawner)
        {
            bossSpawner = world.GetBossSpawnerFinal().FirstOrDefault();
        }

        if (bossSpawner)
        {
            CacheIconsForObject(GameObjectUtils.FindMinimapIcon(bossSpawner.transform));
            var bossSpawnerData = new TrackedObjectData
            {
                Position = PositionData.FromVector3(bossSpawner.transform.position),
                InstanceId = bossSpawner.gameObject.GetInstanceID(), 
            };
            trackedObjects.Add(bossSpawnerData);
        }

        return new ApiListResponse<TrackedObjectData>(trackedObjects);
    }
}