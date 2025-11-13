using System.Text.Json;
using BonkersLib.Core;
using MegaBonkPlusMod.Actions.Base;
using MegaBonkPlusMod.Utils;
using UnityEngine;

namespace MegaBonkPlusMod.Actions.Teleport;

public class TeleportToNearestAction : IAction
{
    public string Execute(JsonElement payload, ActionHandler actionHandler)
    {
        if (!payload.TryGetProperty("object", out var objectElement))
        {
            ModLogger.LogDebug("[TeleportToNearest] failed: 'object' missing in Payload");
            return "Error: 'object' missing";
        }

        if (!BonkersAPI.Game.IsInGame)
            return "Cannot teleport: Not in game";

        string teleportToObject = objectElement.GetString();
        Vector3 nearestObject = Vector3.zero;
        switch (teleportToObject)
        {
            case "chest":
                nearestObject = BonkersAPI.World.GetNearestChest() ?? nearestObject;
                if (nearestObject != Vector3.zero)
                    BonkersAPI.Player.TeleportTo(nearestObject);
                break;
            case "charge_shrine":
                nearestObject = BonkersAPI.World.GetNearestChargeShrine() ?? nearestObject;
                if (nearestObject != Vector3.zero)
                    BonkersAPI.Player.TeleportTo(nearestObject);
                break;
            case "shady_guy":
                nearestObject = BonkersAPI.World.GetNearestShadyGuy() ?? nearestObject;
                if (nearestObject != Vector3.zero)
                    BonkersAPI.Player.TeleportTo(nearestObject);
                break;
            case "boss_spawner":
                nearestObject = BonkersAPI.World.GetNearestBossSpawner() ??
                                BonkersAPI.World.GetNearestBossSpawnerFinal() ?? nearestObject;
                if (nearestObject != Vector3.zero)
                    BonkersAPI.Player.TeleportTo(nearestObject);
                break;
            case "moai_shrine":
                nearestObject = BonkersAPI.World.GetNearestMoaiShrine() ?? nearestObject;
                if (nearestObject != Vector3.zero)
                    BonkersAPI.Player.TeleportTo(nearestObject);
                break;
            case "challenge_shrine":
                nearestObject = BonkersAPI.World.GetNearestChallengeShrine() ?? nearestObject;
                if (nearestObject != Vector3.zero)
                    BonkersAPI.Player.TeleportTo(nearestObject);
                break;
            case "cursed_shrine":
                nearestObject = BonkersAPI.World.GetNearestCursedShrine() ?? nearestObject;
                if (nearestObject != Vector3.zero)
                    BonkersAPI.Player.TeleportTo(nearestObject);
                break;
            case "greed_shrine":
                nearestObject = BonkersAPI.World.GetNearestGreedShrine() ?? nearestObject;
                if (nearestObject != Vector3.zero)
                    BonkersAPI.Player.TeleportTo(nearestObject);
                break;
            case "magnet_shrine":
                nearestObject = BonkersAPI.World.GetNearestMagnetShrine() ?? nearestObject;
                if (nearestObject != Vector3.zero)
                    BonkersAPI.Player.TeleportTo(nearestObject);
                break;
            case "microwave":
                nearestObject = BonkersAPI.World.GetNearestMicrowave() ?? nearestObject;
                if (nearestObject != Vector3.zero)
                    BonkersAPI.Player.TeleportTo(nearestObject);
                break;
            case "open_chest":
                nearestObject = BonkersAPI.World.GetNearestOpenChest() ?? nearestObject;
                if (nearestObject != Vector3.zero)
                    BonkersAPI.Player.TeleportTo(nearestObject);
                break;
            default:
                return $"Error: Unknown teleport target '{teleportToObject}'";
        }
        
        return "Teleport successful";
    }
}