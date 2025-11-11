using System.Linq;
using System.Text.Json;
using Assets.Scripts.Actors.Player;
using BepInEx.Logging;
using BonkersLib.Core;
using UnityEngine;

namespace MegaBonkPlusMod.Actions;
public class TeleportAction : IAction
{
    public void Execute(JsonElement payload, MyPlayer player, ManualLogSource logger, ActionHandler handler)
    {
        if (!payload.TryGetProperty("instanceId", out var idElement) || idElement.ValueKind != JsonValueKind.Number)
        {
            logger.LogWarning("[TeleportAction] failed: 'instanceId' missing in Payload");
            return;
        }

        int instanceId = idElement.GetInt32();

        var allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        GameObject target = allGameObjects.FirstOrDefault(g => g.GetInstanceID() == instanceId);

        if (target == null)
        {
            logger.LogWarning($"[TeleportAction] failed: ObjectID {instanceId} not found");
            return;
        }

        logger.LogInfo($"[TeleportAction] Starting Teleport-Job to {target.name}...");
        
        Vector3 targetPosition = target.transform.position;
        
        BonkersAPI.Player.TeleportTo(targetPosition);
    }
}