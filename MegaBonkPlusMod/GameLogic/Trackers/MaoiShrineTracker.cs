using System.Collections.Generic;
using BepInEx.Logging;
using BonkersLib.Core;
using BonkersLib.Services;
using BonkersLib.Utils;
using MegaBonkPlusMod.Models;
using Object = UnityEngine.Object;

namespace MegaBonkPlusMod.GameLogic.Trackers;

public class MaoiShrineTracker : BaseTracker
{
    public MaoiShrineTracker(float scanIntervalInSeconds) : base(scanIntervalInSeconds)
    {
    }

    public override string ApiRoute => "/api/tracker/shrines/maoi";

    protected override object BuildDataPayload()
    {
        var trackedObjects = new List<TrackedObjectDataModel>();
        
        if (!BonkersAPI.Game.IsInGame)
            return new ApiListResponseModel<TrackedObjectDataModel>(trackedObjects);
        
        WorldService world = BonkersAPI.World;
        
        var allObjects = world.GetMoaiShrines();

        foreach (var trackedObject in allObjects)
        {
            CacheIconsForObject(GameObjectUtils.FindMinimapIcon(trackedObject.transform));
            var shrineData = new TrackedObjectDataModel
            {
                Position = PositionDataModel.FromVector3(trackedObject.transform.position),
                InstanceId = trackedObject.gameObject.GetInstanceID()
            };
            trackedObjects.Add(shrineData);
        }

        return new ApiListResponseModel<TrackedObjectDataModel>(trackedObjects);
    }
}