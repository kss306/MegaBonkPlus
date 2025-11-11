using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using BepInEx.Logging;
using Assets.Scripts.Actors.Player;

namespace MegaBonkPlusMod.Actions;

public class ActionHandler
{
    private readonly Dictionary<string, IAction> _actions;
    private readonly ManualLogSource _logger;
    
    private readonly List<IUpdatableAction> _updatableActions = new();

    public ActionHandler(ManualLogSource logger)
    {
        _logger = logger;
        _actions = new Dictionary<string, IAction>(StringComparer.OrdinalIgnoreCase);
        RegisterActions();
    }
    
    private void RegisterActions()
    {
        _logger.LogInfo("Register actions...");
        
        _actions["teleport"] = new TeleportAction();
        _actions["interact"] = new InteractAction();
        _actions["kill_all_enemies"] = new KillAllEnemiesAction();
        _actions["set_auto_restart_config"] = new AutoRestartAction();
        
        _logger.LogInfo($"{_actions.Count} actions registered.");
    }
    
    public void HandleAction(string actionName, JsonElement payload, MyPlayer player)
    {
        if (_actions.TryGetValue(actionName, out IAction action))
        {
            try
            {
                action.Execute(payload, player, _logger, this);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error executing action '{actionName}': {ex.Message}\n{ex.StackTrace}");
            }
        }
        else
        {
            _logger.LogWarning($"Unknown action received: '{actionName}'");
        }
    }
    
    public void RegisterUpdatable(IUpdatableAction action)
    {
        if (action != null && !_updatableActions.Contains(action))
        {
            _updatableActions.Add(action);
        }
    }
    
    public Dictionary<string, object> GetActionStates()
    {
        var states = new Dictionary<string, object>();

        if (_actions.TryGetValue("kill_all_enemies", out IAction action1) && action1 is KillAllEnemiesAction killAction)
        {
            states["kill_all_enemies"] = new { looping = killAction.IsLooping };
        }
        
        if (_actions.TryGetValue("set_auto_restart_config", out IAction action2) && action2 is AutoRestartAction restartAction)
        {
            states["set_auto_restart_config"] = new { enabled = restartAction.IsEnabled };
        }

        return states;
    }

    public void UpdateActions()
    {
        if (_updatableActions.Count == 0) return;
        foreach (var action in _updatableActions.ToList())
        {
            try
            {
                bool keepRunning = action.Update();
                    
                if (!keepRunning)
                {
                    _updatableActions.Remove(action);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in 'UpdateActions': {ex.Message}");
                _updatableActions.Remove(action);
            }
        }
    }
}