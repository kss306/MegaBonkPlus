using System.Linq;
using System.Text.Json;
using Assets.Scripts.Actors.Player;
using Assets.Scripts.Inventory__Items__Pickups.Chests;
using BepInEx.Logging;
using UnityEngine;

namespace MegaBonkPlusMod.Actions;

public class InteractAction : IAction
{
    public void Execute(JsonElement payload, ActionHandler handler)
    {
        
        if (!payload.TryGetProperty("instanceId", out var idElement) || idElement.ValueKind != JsonValueKind.Number) return;

        int instanceId = idElement.GetInt32();
        var allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        GameObject target = allGameObjects.FirstOrDefault(g => g.GetInstanceID() == instanceId);

        if (!target) return;
        
        var chest = target.GetComponent<InteractableChest>();
        if (chest) { chest.Interact(); return; }

        var maoiShrine = target.GetComponent<InteractableShrineMoai>();
        if (maoiShrine) { maoiShrine.Interact(); return; }

        var bossSpawner = target.GetComponent<InteractableBossSpawner>();
        if (bossSpawner) { bossSpawner.Interact(); return; }
                    
        var challengeShrine = target.GetComponent<InteractableShrineChallenge>();
        if (challengeShrine) { challengeShrine.Interact(); return; }
                    
        var cursedShrine = target.GetComponent<InteractableShrineCursed>();
        if (cursedShrine) { cursedShrine.Interact(); return; }
                    
        var greedShrine = target.GetComponent<InteractableShrineGreed>();
        if (greedShrine) { greedShrine.Interact(); return; }
                    
        var magnetShrine = target.GetComponent<InteractableShrineMagnet>();
        if (magnetShrine) { magnetShrine.Interact(); return; }
                    
        var microwave = target.GetComponent<InteractableMicrowave>();
        if (microwave) { microwave.Interact(); return; }
                    
        var shadyGuy = target.GetComponent<InteractableShadyGuy>();
        if (shadyGuy) { shadyGuy.Interact(); }
    }
}