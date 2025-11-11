using System.Collections.Generic;
using BepInEx.Logging;
using BonkersLib.Core;
using BonkersLib.Services;
using BonkersLib.Utils;
using MegaBonkPlusMod.Models;

namespace MegaBonkPlusMod.GameLogic.Trackers;

public class ChallengeShrineTracker : BaseTracker
{
    public ChallengeShrineTracker(float scanIntervalInSeconds) : base(scanIntervalInSeconds)
    {
    }

    public override string ApiRoute => "/api/tracker/shrines/challenge";

    protected override object BuildDataPayload()
    {
        var trackedObjects = new List<TrackedObjectDataModel>();
        
        if (!BonkersAPI.Game.IsInGame)
            return new ApiListResponseModel<TrackedObjectDataModel>(trackedObjects);
        
        WorldService world = BonkersAPI.World;
        
        var allObjects = world.GetChallengeShrines();

        foreach (var trackedObject in allObjects)
        {
            CacheIconsForObject(GameObjectUtils.FindMinimapIcon(trackedObject.transform));
            var objectData = new TrackedObjectDataModel
            {
                Position = PositionDataModel.FromVector3(trackedObject.transform.position),
                InstanceId = trackedObject.gameObject.GetInstanceID(), 
            };
            trackedObjects.Add(objectData);
        }

        return new ApiListResponseModel<TrackedObjectDataModel>(trackedObjects);
    }
}