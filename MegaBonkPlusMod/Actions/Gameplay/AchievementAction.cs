using System.Text.Json;
using BonkersLib.Core;
using MegaBonkPlusMod.Actions.Base;
using MegaBonkPlusMod.Utils;

namespace MegaBonkPlusMod.Actions.Gameplay;

public class AchievementAction : IAction
{
    public string Execute(JsonElement payload, ActionHandler actionHandler)
    {
        if(BonkersAPI.Achievements.AllAchievements == null)
            return "Error: Achievements not loaded yet";
        
        BonkersAPI.Achievements.UnlockAll();
        
        return "Achievements unlocked";
    }
}