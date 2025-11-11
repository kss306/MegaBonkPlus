using System.Text.Json;
using MegaBonkPlusMod.Core;

namespace MegaBonkPlusMod.Actions;

public class SetHotkeyConfigAction : IAction
{
    public void Execute(JsonElement payload, ActionHandler actionHandler)
    {
        HotkeyManager.UpdateConfig(payload);
    }
}