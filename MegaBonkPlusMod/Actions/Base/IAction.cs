using System.Text.Json;

namespace MegaBonkPlusMod.Actions.Base;

public interface IAction
{
    string Execute(JsonElement payload, ActionHandler actionHandler);
}