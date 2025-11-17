using System.Text.Json;
using BonkersLib.Core;
using MegaBonkPlusMod.Actions.Base;
using MegaBonkPlusMod.Utils;

namespace MegaBonkPlusMod.Actions.Gameplay;

public class LevelAction : IAction
{
    public string Execute(JsonElement payload, ActionHandler actionHandler)
    {
        if (!payload.TryGetProperty("amount", out var amountElement))
        {
            ModLogger.LogDebug("[LevelAction] failed: 'amount' missing in Payload");
            return "Error: 'amount' missing";
        }

        if (!BonkersAPI.Game.IsInGame)
            return "Cannot add levels: Not in game";
        ;

        var amount = amountElement.GetInt32();

        if (amount < 0 || amount > 9999)
            return "Error: Amount must be between 0 and 9999";

        BonkersAPI.Player.AddLevel(amount);
        return $"Added {amount} level{(amount == 1 ? "" : "s")}";
    }
}