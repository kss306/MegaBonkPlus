using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Inventory__Items__Pickups.Chests;
using BonkersLib.Enums;
using MegaBonkPlusMod.GameLogic.Trackers;
using MegaBonkPlusMod.GameLogic.Trackers.Base;

namespace MegaBonkPlusMod.Infrastructure.Services;

public class TrackerRegistryService
{
    private readonly Dictionary<string, BaseTracker> _trackers = new();
    
    public IReadOnlyDictionary<string, BaseTracker> TrackersDictionary => _trackers;
    public IReadOnlyList<BaseTracker> TrackersList => _trackers.Values.ToList();

    public void RegisterDefaultTrackers()
    {
        Register(TrackerKeys.Player, new PlayerTracker(0.1f));
        Register(TrackerKeys.BossSpawner, new BossSpawnerTracker(5.0f));
        Register(TrackerKeys.ShadyGuys, new ShadyGuyTracker(2.0f));

        Register(TrackerKeys.ChargeShrines, new GenericTracker(
            WorldObjectTypeEnum.ChargeShrine,
            2.0f,
            obj =>
            {
                var shrine = obj as ChargeShrine;
                return new Dictionary<string, object>
                {
                    ["isGolden"] = shrine?.isGolden ?? false
                };
            }
        ));

        Register(TrackerKeys.Microwaves, new GenericTracker(
            WorldObjectTypeEnum.Microwave,
            2.0f,
            obj =>
            {
                var microwave = obj as InteractableMicrowave;
                return new Dictionary<string, object>
                {
                    ["rarity"] = microwave?.rarity.ToString() ?? "Unknown"
                };
            }
        ));

        Register(TrackerKeys.Chests, new GenericTracker(
            WorldObjectTypeEnum.Chest,
            2.0f,
            obj =>
            {
                var chest = obj as InteractableChest;
                return new Dictionary<string, object>
                {
                    ["type"] = chest?.chestType.ToString() ?? "Unknown"
                };
            }
        ));

        Register(TrackerKeys.GreedShrines, new GenericTracker(WorldObjectTypeEnum.GreedShrine, 2.0f));
        Register(TrackerKeys.MoaiShrines, new GenericTracker(WorldObjectTypeEnum.MoaiShrine, 2.0f));
        Register(TrackerKeys.CursedShrines, new GenericTracker(WorldObjectTypeEnum.CursedShrine, 2.0f));
        Register(TrackerKeys.MagnetShrines, new GenericTracker(WorldObjectTypeEnum.MagnetShrine, 2.0f));
        Register(TrackerKeys.ChallengeShrines, new GenericTracker(WorldObjectTypeEnum.ChallengeShrine, 2.0f));

        Register(TrackerKeys.Bosses, new GenericTracker(WorldObjectTypeEnum.Boss, 2.0f));
    }

    private void Register(string key, BaseTracker tracker)
    {
        _trackers[key] = tracker;
    }

    public void UpdateAll()
    {
        foreach (var tracker in _trackers.Values)
        {
            tracker.Update();
        }
    }
}