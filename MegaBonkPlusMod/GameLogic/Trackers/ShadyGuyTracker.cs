using System.Collections.Generic;
using BepInEx.Logging;
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
        var trackedShadyGuys = new List<TrackedObjectData>();
        var allShadyGuys = Object.FindObjectsOfType<InteractableShadyGuy>();

        foreach (var shadyGuy in allShadyGuys)
        {
            var guyData = new TrackedObjectData
            {
                Position = PositionData.FromVector3(shadyGuy.transform.position)
            };

            var itemNames = new List<string>();

            foreach (var item in shadyGuy.items) itemNames.Add(item.name);
            var itemPrices = new List<int>();

            foreach (var price in shadyGuy.prices) itemPrices.Add(price);

            guyData.CustomProperties["itemNames"] = itemNames;
            guyData.CustomProperties["itemPrices"] = itemPrices;
            guyData.CustomProperties["rarity"] = shadyGuy.rarity.ToString();
            trackedShadyGuys.Add(guyData);
        }

        return new ApiListResponse<TrackedObjectData>(trackedShadyGuys);
    }
}