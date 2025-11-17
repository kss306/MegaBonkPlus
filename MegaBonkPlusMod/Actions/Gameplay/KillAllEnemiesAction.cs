using System;
using System.Text.Json;
using BonkersLib.Core;
using MegaBonkPlusMod.Actions.Base;

namespace MegaBonkPlusMod.Actions.Gameplay;

public class KillAllEnemiesAction : IAction, IUpdatableAction
{
    private bool _isRegistered;

    public bool IsLooping { get; private set; }

    public string Execute(JsonElement payload, ActionHandler handler)
    {
        var newLoopingState = IsLooping;
        var hasExplicitLooping = false;

        if (payload.TryGetProperty("looping", out var loopingElement))
            switch (loopingElement.ValueKind)
            {
                case JsonValueKind.True:
                case JsonValueKind.False:
                    newLoopingState = loopingElement.GetBoolean();
                    hasExplicitLooping = true;
                    break;
                case JsonValueKind.String:
                    if (bool.TryParse(loopingElement.GetString(), out var parsed))
                    {
                        newLoopingState = parsed;
                        hasExplicitLooping = true;
                    }

                    break;
            }

        if (!hasExplicitLooping && payload.TryGetProperty("mode", out var modeElement) &&
            modeElement.ValueKind == JsonValueKind.String)
        {
            var mode = modeElement.GetString();
            if (string.Equals(mode, "toggle", StringComparison.OrdinalIgnoreCase))
                newLoopingState = !IsLooping;
            else if (string.Equals(mode, "single", StringComparison.OrdinalIgnoreCase)) newLoopingState = false;
        }

        if (newLoopingState)
        {
            IsLooping = true;
            if (!_isRegistered)
            {
                handler.RegisterUpdatable(this);
                _isRegistered = true;
            }

            return "Started Kill loop";
        }

        if (IsLooping)
        {
            IsLooping = false;
            return "Stopped Kill loop";
        }

        BonkersAPI.World.KillAllEnemies();
        return "Killed all enemies";
    }

    public bool Update()
    {
        if (!IsLooping)
        {
            _isRegistered = false;
            return false;
        }

        BonkersAPI.World.KillAllEnemies();
        return true;
    }
}