using System.Text.Json;
using Assets.Scripts.Inventory__Items__Pickups.Chests;
using BonkersLib.Core;
using MegaBonkPlusMod.Actions.Base;
using MegaBonkPlusMod.Utils;
using UnityEngine;

namespace MegaBonkPlusMod.Actions.Gameplay;

public class InteractAction : IAction
{
    public string Execute(JsonElement payload, ActionHandler handler)
    {
        if (!payload.TryGetProperty("instanceId", out var idElement) ||
            idElement.ValueKind != JsonValueKind.Number)
        {
            ModLogger.LogDebug("[InteractAction] No valid instanceId provided");
            return "Error: No valid instanceId provided";
        }

        int instanceId = idElement.GetInt32();

        var component = BonkersAPI.World.GetComponentByInstanceId<Component>(instanceId);

        if (!component || !component)
        {
            ModLogger.LogDebug($"[InteractAction] Component with instanceId {instanceId} not found in cache");
            return "Error: Component not found in cache";
        }

        switch (component)
        {
            case InteractableChest chest:
                chest.Interact();
                ModLogger.LogDebug($"[InteractAction] Interacted with Chest");
                break;

            case InteractableShrineMoai moaiShrine:
                moaiShrine.Interact();
                ModLogger.LogDebug($"[InteractAction] Interacted with Moai Shrine");
                break;

            case InteractableBossSpawner bossSpawner:
                bossSpawner.Interact();
                ModLogger.LogDebug($"[InteractAction] Interacted with Boss Spawner");
                break;

            case InteractableBossSpawnerFinal bossSpawnerFinal:
                bossSpawnerFinal.Interact();
                ModLogger.LogDebug($"[InteractAction] Interacted with Final Boss Spawner");
                break;

            case InteractableShrineChallenge challengeShrine:
                challengeShrine.Interact();
                ModLogger.LogDebug($"[InteractAction] Interacted with Challenge Shrine");
                break;

            case InteractableShrineCursed cursedShrine:
                cursedShrine.Interact();
                ModLogger.LogDebug($"[InteractAction] Interacted with Cursed Shrine");
                break;

            case InteractableShrineGreed greedShrine:
                greedShrine.Interact();
                ModLogger.LogDebug($"[InteractAction] Interacted with Greed Shrine");
                break;

            case InteractableShrineMagnet magnetShrine:
                magnetShrine.Interact();
                ModLogger.LogDebug($"[InteractAction] Interacted with Magnet Shrine");
                break;

            case InteractableMicrowave microwave:
                microwave.Interact();
                ModLogger.LogDebug($"[InteractAction] Interacted with Microwave");
                break;

            case InteractableShadyGuy shadyGuy:
                shadyGuy.Interact();
                ModLogger.LogDebug($"[InteractAction] Interacted with Shady Guy");
                break;

            case ChargeShrine chargeShrine:
                chargeShrine.Complete();
                ModLogger.LogDebug($"[InteractAction] Completed Charge Shrine");
                break;

            default:
                ModLogger.LogDebug(
                    $"[InteractAction] Component type {component.GetType().Name} has no interaction handler");
                return "Error: No interaction handler found";
        }
        
        return "Interacted with Object";
    }
}