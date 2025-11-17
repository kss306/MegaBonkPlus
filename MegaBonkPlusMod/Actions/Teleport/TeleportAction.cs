using System.Linq;
using System.Text.Json;
using BonkersLib.Core;
using MegaBonkPlusMod.Actions.Base;
using MegaBonkPlusMod.Utils;
using UnityEngine;

namespace MegaBonkPlusMod.Actions.Teleport;

public class TeleportAction : IAction
{
    public string Execute(JsonElement payload, ActionHandler handler)
    {
        if (!payload.TryGetProperty("instanceId", out var idElement) || idElement.ValueKind != JsonValueKind.Number)
        {
            ModLogger.LogDebug("[TeleportAction] failed: 'instanceId' missing in Payload");
            return "Error: 'instanceId' missing";
        }

        var instanceId = idElement.GetInt32();

        var allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        var target = allGameObjects.FirstOrDefault(g => g.GetInstanceID() == instanceId);

        if (!target)
        {
            ModLogger.LogDebug($"[TeleportAction] failed: ObjectID {instanceId} not found");
            return "Error: Object not found";
        }

        ModLogger.LogDebug($"[TeleportAction] Starting Teleport-Job to {target.name}...");

        var targetPosition = target.transform.position;

        BonkersAPI.Player.TeleportTo(targetPosition);

        return "Teleport successful";
    }
}