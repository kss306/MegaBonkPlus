using System;
using System.Text.Json;
using Assets.Scripts.Actors.Enemies;
using Assets.Scripts.Actors.Player;
using BepInEx.Logging;
using BonkersLib.Core;
using MegaBonkPlusMod.Actions.Base;
using Object = UnityEngine.Object;

namespace MegaBonkPlusMod.Actions.Gameplay;

public class KillAllEnemiesAction : IAction, IUpdatableAction
{
    private bool _looping;
    private bool _isRegistered;
    
    public bool IsLooping => _looping;

    public string Execute(JsonElement payload, ActionHandler handler)
    {
        bool newLoopingState = _looping;
        if (payload.TryGetProperty("looping", out var loopingElement))
        {
            newLoopingState = loopingElement.GetBoolean();
        }
        
        if (newLoopingState)
        {
            _looping = true;
            if (!_isRegistered)
            {
                handler.RegisterUpdatable(this);
                _isRegistered = true;
            }
            return "Started Kill loop";
        }
        else
        {
            if (_looping)
            {
                _looping = false;
                return "Stopped Kill loop";
            }
            else
            {
                BonkersAPI.World.KillAllEnemies();
                return "Killed all enemies";
            }
        }
    }

    public bool Update()
    {
        if (!_looping)
        {
            _isRegistered = false;
            return false;
        }
        
        BonkersAPI.World.KillAllEnemies();
        return true;
    }
}