using System.Text.Json;
using BonkersLib.Core;
using MegaBonkPlusMod.Utils;

namespace MegaBonkPlusMod.Actions;

public class LevelAction : IAction
{
    public void Execute(JsonElement payload, ActionHandler actionHandler)
    {
        if (!payload.TryGetProperty("amount", out var amountElement))
        {
            ModLogger.LogDebug("[LevelAction] failed: 'amount' missing in Payload");
            return;
        }

        if (!BonkersAPI.Game.IsInGame) return;
        
        int amount = amountElement.GetInt32();
        
        if (amount < 0 || amount > 9999) return;

        BonkersAPI.Player.AddLevel(amount);
    }
}