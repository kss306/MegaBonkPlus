using System.Collections.Generic;
using BonkersLib.Core;
using BonkersLib.Services;
using BonkersLib.Utils;
using MegaBonkPlusMod.GameLogic.Trackers.Base;
using MegaBonkPlusMod.Models;

namespace MegaBonkPlusMod.GameLogic.Trackers;

public class ShadyGuyTracker : BaseTracker
{
    public ShadyGuyTracker(float scanIntervalInSeconds) : base(scanIntervalInSeconds)
    {
    }
    
    protected override object BuildDataPayload()
    {
        var trackedObjects = new List<TrackedObjectDataModel>();

        if (!BonkersAPI.Game.IsInGame)
            return new ApiListResponseModel<TrackedObjectDataModel>(trackedObjects);

        WorldService world = BonkersAPI.World;

        var allObjects = world.GetShadyGuys();

        foreach (var trackedObject in allObjects)
        {
            CacheIconsForObject(GameObjectUtils.FindMinimapIcon(trackedObject.transform));
            var guyData = new TrackedObjectDataModel
            {
                Position = PositionDataModel.FromVector3(trackedObject.transform.position),
                InstanceId = trackedObject.gameObject.GetInstanceID()
            };

            var itemNames = new List<string>();
            var itemPrices = new List<int>();
            var itemIds = new List<string>();

            foreach (var price in trackedObject.prices) itemPrices.Add(price);
            foreach (var item in trackedObject.items)
            {
                itemNames.Add(item.name);
                itemIds.Add(item.eItem.ToString().ToLowerInvariant());
            }

            guyData.CustomProperties["itemNames"] = itemNames;
            guyData.CustomProperties["itemIds"] = itemIds;
            guyData.CustomProperties["itemPrices"] = itemPrices;
            guyData.CustomProperties["rarity"] = trackedObject.rarity.ToString();
            trackedObjects.Add(guyData);
        }

        return new ApiListResponseModel<TrackedObjectDataModel>(trackedObjects);
    }
}