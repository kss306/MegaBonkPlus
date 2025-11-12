using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using MegaBonkPlusMod.Core;
using MegaBonkPlusMod.Utils;

namespace MegaBonkPlusMod.Actions;

public class ActionHandler
{
    private readonly Dictionary<string, IAction> _actions;
    private readonly List<IUpdatableAction> _updatableActions = new();

    public ActionHandler()
    {
        _actions = new Dictionary<string, IAction>(StringComparer.OrdinalIgnoreCase);
        RegisterActions();
    }
    
    private void RegisterActions()
    {
        ModLogger.LogDebug("Register actions...");
        
        _actions["teleport"] = new TeleportAction();
        _actions["interact"] = new InteractAction();
        _actions["kill_all_enemies"] = new KillAllEnemiesAction();
        _actions["set_auto_restart_config"] = new AutoRestartAction();
        _actions["spawn_items"] = new SpawnItemsAction();
        _actions["edit_gold"] = new GoldAction();
        _actions["teleport_to_nearest"] = new TeleportToNearestAction();
        _actions["add_levels"] = new LevelAction();
        
        _actions["set_hotkey_config"] = new SetHotkeyConfigAction();
        ModLogger.LogDebug($"{_actions.Count} actions registered.");
    }
    
    public void HandleAction(string actionName, JsonElement payload)
    {
        if (_actions.TryGetValue(actionName, out IAction action))
        {
            try
            {
                action.Execute(payload, this);
            }
            catch (Exception ex)
            {
                ModLogger.LogDebug($"Error executing action '{actionName}': {ex.Message}\n{ex.StackTrace}");
            }
        }
        else
        {
            ModLogger.LogDebug($"Unknown action received: '{actionName}'");
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
            states["set_auto_restart_config"] = new
            {
                enabled = restartAction.IsEnabled,
                itemIds = restartAction.ItemIds
            };
        }

        return states;
    }

    public void UpdateActions()
    {
        HotkeyManager.CheckKeys(this);
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
                ModLogger.LogDebug($"Error in 'UpdateActions': {ex.Message}");
                _updatableActions.Remove(action);
            }
        }
    }
}