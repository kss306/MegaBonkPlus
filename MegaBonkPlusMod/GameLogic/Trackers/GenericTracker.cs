using System;
using System.Collections.Generic;
using BonkersLib.Core;
using BonkersLib.Enums;
using BonkersLib.Utils;
using MegaBonkPlusMod.GameLogic.Trackers.Base;
using MegaBonkPlusMod.Models;
using MegaBonkPlusMod.Utils;
using UnityEngine;

namespace MegaBonkPlusMod.GameLogic.Trackers;

public class GenericTracker : BaseTracker
{
    private readonly Func<Component, Dictionary<string, object>> _customPropertiesExtractor;
    private readonly WorldObjectTypeEnum _objectType;

    public GenericTracker(
        WorldObjectTypeEnum objectType,
        float scanIntervalInSeconds,
        Func<Component, Dictionary<string, object>> customPropertiesExtractor = null)
        : base(scanIntervalInSeconds)
    {
        _objectType = objectType;
        _customPropertiesExtractor = customPropertiesExtractor;
    }


    protected override object BuildDataPayload()
    {
        var trackedObjects = new List<TrackedObjectDataModel>();

        if (!BonkersAPI.Game.IsInGame)
            return new ApiListResponseModel<TrackedObjectDataModel>(trackedObjects);

        try
        {
            var world = BonkersAPI.World;
            var allObjects = world.GetCachedObjects<Component>(_objectType);

            foreach (var trackedObject in allObjects)
            {
                if (!trackedObject || !trackedObject.gameObject)
                    continue;

                try
                {
                    CacheIconsForObject(GameObjectUtils.FindMinimapIcon(trackedObject.transform));

                    var objectData = new TrackedObjectDataModel
                    {
                        Position = PositionDataModel.FromVector3(trackedObject.transform.position),
                        InstanceId = trackedObject.gameObject.GetInstanceID()
                    };

                    if (_customPropertiesExtractor != null)
                        try
                        {
                            var customProps = _customPropertiesExtractor(trackedObject);
                            if (customProps != null)
                                foreach (var kvp in customProps)
                                    objectData.CustomProperties[kvp.Key] = kvp.Value;
                        }
                        catch (Exception ex)
                        {
                            ModLogger.LogDebug(
                                $"[GenericTracker] Error extracting custom properties: {ex.Message}");
                        }

                    trackedObjects.Add(objectData);
                }
                catch (Exception ex)
                {
                    ModLogger.LogDebug($"[GenericTracker] Error processing object: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            ModLogger.LogDebug($"[GenericTracker] Error in BuildDataPayload: {ex.Message}");
        }

        return new ApiListResponseModel<TrackedObjectDataModel>(trackedObjects);
    }
}