using System.Text.Json;
using BonkersLib.Core;
using MegaBonkPlusMod.Actions.Base;
using MegaBonkPlusMod.Utils;

namespace MegaBonkPlusMod.Actions.Gameplay;

public class GoldAction : IAction
{
    public string Execute(JsonElement payload, ActionHandler actionHandler)
    {
        if (!payload.TryGetProperty("changeMode", out var changeModeElement) || 
            !payload.TryGetProperty("amount", out var amountElement))
        {
            ModLogger.LogDebug("[GoldAction] Missing required properties in payload");
            return "Error: 'changeMode' or 'amount' missing";
        }

        if (!BonkersAPI.Game.IsInGame) 
            return "Cannot edit gold: Not in game";
        
        string changeMode = changeModeElement.GetString();
        int amount = amountElement.GetInt32();
        
        if (amount < 0) 
            return "Gold cannot be negative";

        switch (changeMode)
        {
            case "set":
                BonkersAPI.Player.SetGold(amount);
                return $"Gold set to {amount:N0}";
            case "add":
                BonkersAPI.Player.GiveGold(amount);
                return $"Added {amount:N0} gold";
            default:
                return $"Error: Unknown changeMode '{changeMode}'";
        }
    }
}