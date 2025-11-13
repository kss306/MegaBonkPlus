using System.Collections.Generic;
using System.Linq;
using BonkersLib.Core;
using BonkersLib.Services;
using BonkersLib.Utils;
using MegaBonkPlusMod.GameLogic.Trackers.Base;
using MegaBonkPlusMod.Models;
using UnityEngine;

namespace MegaBonkPlusMod.GameLogic.Trackers;

public class BossSpawnerTracker : BaseTracker
{
    public BossSpawnerTracker(float scanIntervalInSeconds) : base(scanIntervalInSeconds)
    {
    }
    
    protected override object BuildDataPayload()
    {
        var trackedObjects = new List<TrackedObjectDataModel>();

        if (!BonkersAPI.Game.IsInGame)
            return new ApiListResponseModel<TrackedObjectDataModel>(trackedObjects);

        WorldService world = BonkersAPI.World;

        Component bossSpawner = world.GetBossSpawner().FirstOrDefault();

        if (!bossSpawner)
        {
            bossSpawner = world.GetBossSpawnerFinal().FirstOrDefault();
        }

        if (bossSpawner)
        {
            CacheIconsForObject(GameObjectUtils.FindMinimapIcon(bossSpawner.transform));
            var bossSpawnerData = new TrackedObjectDataModel
            {
                Position = PositionDataModel.FromVector3(bossSpawner.transform.position),
                InstanceId = bossSpawner.gameObject.GetInstanceID(),
            };
            trackedObjects.Add(bossSpawnerData);
        }

        return new ApiListResponseModel<TrackedObjectDataModel>(trackedObjects);
    }
}