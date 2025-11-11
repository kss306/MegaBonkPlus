using System;
using System.Text.Json;
using BonkersLib.Core;
using MegaBonkPlusMod.GameLogic.Common;
using MegaBonkPlusMod.Utils;

namespace MegaBonkPlusMod.Actions;

public class SpawnItemsAction : IAction
{
    public void Execute(JsonElement payload, ActionHandler handler)
    {
        if (!BonkersAPI.Game.IsInGame) return;

        if (!payload.TryGetProperty("items", out var itemsElement) || itemsElement.ValueKind != JsonValueKind.Array)
        {
            ModLogger.LogDebug("[SpawnItemsAction] 'items' array not found in payload.");
            return;
        }

        MainThreadActionQueue.QueueAction(() =>
        {
            try
            {
                foreach (var itemJson in itemsElement.EnumerateArray())
                {
                    string id = itemJson.GetProperty("id").GetString();
                    int quantity = itemJson.GetProperty("quantity").GetInt32();

                    if (!string.IsNullOrEmpty(id) && quantity > 0)
                    {
                        BonkersAPI.Player.GiveItem(id, quantity);
                    }
                }
            }
            catch (Exception ex)
            {
                ModLogger.LogDebug($"[SpawnItemsAction] Fehler beim Spawnen: {ex.Message}");
            }
        });
    }
}