using System.Text.Json;
using Assets.Scripts.Actors.Player;
using BepInEx.Logging;

namespace MegaBonkPlusMod.Actions;

public interface IAction
{
    void Execute(JsonElement payload, ActionHandler actionHandler);
    
}