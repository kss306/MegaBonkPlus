using System.Collections.Generic;
using BepInEx.Logging;
using BonkersLib.Core;
using BonkersLib.Services;
using BonkersLib.Utils;
using MegaBonkPlusMod.Models;
using Object = UnityEngine.Object;

namespace MegaBonkPlusMod.GameLogic.Trackers;

public class ShadyGuyTracker : BaseTracker
{
    public ShadyGuyTracker(ManualLogSource logger, float scanIntervalInSeconds) : base(logger, scanIntervalInSeconds)
    {
    }

    public override string ApiRoute => "/api/tracker/shadyguys";

    protected override object BuildDataPayload()
    {
        var trackedObjects = new List<TrackedObjectData>();
        
        if (!BonkersAPI.Game.IsInGame)
            return new ApiListResponse<TrackedObjectData>(trackedObjects);
        
        WorldService world = BonkersAPI.World;
        
        var allObjects = Object.FindObjectsOfType<InteractableShadyGuy>();
        
        foreach (var trackedObject in allObjects)
        {
            CacheIconsForObject(GameObjectUtils.FindMinimapIcon(trackedObject.transform));
            var guyData = new TrackedObjectData
            {
                Position = PositionData.FromVector3(trackedObject.transform.position),
                InstanceId = trackedObject.gameObject.GetInstanceID()
            };

            var itemNames = new List<string>();

            foreach (var item in trackedObject.items) itemNames.Add(item.name);
            var itemPrices = new List<int>();

            foreach (var price in trackedObject.prices) itemPrices.Add(price);

            guyData.CustomProperties["itemNames"] = itemNames;
            guyData.CustomProperties["itemPrices"] = itemPrices;
            guyData.CustomProperties["rarity"] = trackedObject.rarity.ToString();
            trackedObjects.Add(guyData);
        }

        return new ApiListResponse<TrackedObjectData>(trackedObjects);
    }
}