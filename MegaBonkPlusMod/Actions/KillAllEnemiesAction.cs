using System;
using System.Text.Json;
using Assets.Scripts.Actors.Enemies;
using Assets.Scripts.Actors.Player;
using BepInEx.Logging;
using BonkersLib.Core;
using Object = UnityEngine.Object;

namespace MegaBonkPlusMod.Actions;

public class KillAllEnemiesAction : IAction, IUpdatableAction
{
    private bool _looping;
    private bool _isRegistered;
    
    public bool IsLooping => _looping;

    public void Execute(JsonElement payload, ActionHandler handler)
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
        }
        else
        {
            if (_looping)
            {
                _looping = false;
            }
            else
            {
                BonkersAPI.World.KillAllEnemies();
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