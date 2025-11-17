using System;
using System.Text.Json;
using BonkersLib.Core;
using MegaBonkPlusMod.Actions.Base;

namespace MegaBonkPlusMod.Actions.Gameplay;

public class PickUpAllXpAction : IAction, IUpdatableAction
{
    private bool _isRegistered;
    private DateTime _nextAllowedPickup = DateTime.MinValue;

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

            _nextAllowedPickup = DateTime.UtcNow;
            return "Started Pick Up loop";
        }

        if (IsLooping)
        {
            IsLooping = false;
            return "Stopped Pick Up loop";
        }

        var now = DateTime.UtcNow;
        if (now < _nextAllowedPickup) return "Pick up XP is on cooldown";

        BonkersAPI.Player.PickUpAllXp();
        _nextAllowedPickup = now.AddSeconds(2);
        return "Picked up all XP";
    }

    public bool Update()
    {
        if (!IsLooping)
        {
            _isRegistered = false;
            return false;
        }

        var now = DateTime.UtcNow;
        if (now < _nextAllowedPickup) return true;

        BonkersAPI.Player.PickUpAllXp();
        _nextAllowedPickup = now.AddSeconds(5);
        return true;
    }
}