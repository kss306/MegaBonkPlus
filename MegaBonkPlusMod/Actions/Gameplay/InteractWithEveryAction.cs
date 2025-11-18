using System;
using System.Text.Json;
using BonkersLib.Core;
using BonkersLib.Enums;
using MegaBonkPlusMod.Actions.Base;
using MegaBonkPlusMod.Utils;

namespace MegaBonkPlusMod.Actions.Gameplay;

public class InteractWithEveryAction : IAction
{
    public string Execute(JsonElement payload, ActionHandler actionHandler)
    {
        if (!payload.TryGetProperty("interactable", out var interactElement) ||
            interactElement.ValueKind != JsonValueKind.String)
        {
            ModLogger.LogDebug(payload.GetRawText());
            ModLogger.LogDebug("[InteractWithEveryAction] No valid interactable provided");
            return "Error: No valid interactable provided";
        }
        
        var interactableString = interactElement.GetString();
        
        if (!Enum.TryParse(interactableString, true, out WorldObjectTypeEnum parsedEnum))
        {
            ModLogger.LogDebug("[InteractWithEveryAction] Could not parse interactable string");
            return "Error: Interactable not found";
        }
        
        BonkersAPI.World.InteractWithEvery(parsedEnum);
        return $"Interacted with every {interactableString}";
        
    }
}