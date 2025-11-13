using System;
using System.Text.Json;
using BonkersLib.Core;
using MegaBonkPlusMod.Actions.Base;
using MegaBonkPlusMod.Utils;

namespace MegaBonkPlusMod.Actions.Gameplay;

public class SpawnItemsAction : IAction
{
    public string Execute(JsonElement payload, ActionHandler handler)
    {
        if (!BonkersAPI.Game.IsInGame)
            return "Error: Not in game";

        if (!payload.TryGetProperty("items", out var itemsElement) || itemsElement.ValueKind != JsonValueKind.Array)
        {
            ModLogger.LogDebug("[SpawnItemsAction] 'items' array not found in payload.");
            return "Error: 'items' array not found in payload.";
        }

        MainThreadActionQueue.QueueAction(() =>
        {
            try
            {
                foreach (var itemJson in itemsElement.EnumerateArray())
                {
                    string id = itemJson.GetProperty("id").GetString();
                    int quantity = itemJson.GetProperty("quantity").GetInt32();

                    if (!string.IsNullOrEmpty(id) && quantity is > 0 and <= 99)
                    {
                        BonkersAPI.Player.GiveItem(id, quantity);
                    }
                    else
                    {
                        ModLogger.LogDebug($"[SpawnItemsAction] Invalid quantity ({quantity}) for item '{id}'. Skipping.");
                    }
                }
            }
            catch (Exception ex)
            {
                ModLogger.LogDebug($"[SpawnItemsAction] Fehler beim Spawnen: {ex.Message}");
            }
        });
        
        return "Items spawned.";
    }
}