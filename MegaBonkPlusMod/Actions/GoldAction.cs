using System.Text.Json;
using BonkersLib.Core;
using MegaBonkPlusMod.Utils;

namespace MegaBonkPlusMod.Actions;

public class GoldAction :IAction
{
    public void Execute(JsonElement payload, ActionHandler actionHandler)
    {
        if (!payload.TryGetProperty("changeMode", out var changeModeElement) || !payload.TryGetProperty("amount", out var amountElement))
        {
            ModLogger.LogDebug("[GoldEdit] failed: 'changeMode' or 'amount' missing in Payload");
            return;
        }

        if (!BonkersAPI.Game.IsInGame) return;
            
        string changeMode = changeModeElement.GetString();
        int amount = amountElement.GetInt32();
        
        if (amount < 0) return;

        switch (changeMode)
        {
            case "set":
                BonkersAPI.Player.SetGold(amount);
                break;
            case "add":
                BonkersAPI.Player.GiveGold(amount);
                break;
        }
    }
}