using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using MegaBonkPlusMod.Actions.Gameplay;
using MegaBonkPlusMod.Actions.Inventory;
using MegaBonkPlusMod.Actions.Teleport;
using MegaBonkPlusMod.Core;
using MegaBonkPlusMod.Utils;

namespace MegaBonkPlusMod.Actions.Base;

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
        _actions["pick_up_all_xp"] = new PickUpAllXpAction();
        _actions["weapon"] = new WeaponAction();
        _actions["tome"] = new TomeAction();
        _actions["unlock_all"] = new AchievementAction();
        _actions["interact_with_every"] = new InteractWithEveryAction();

        ModLogger.LogDebug($"{_actions.Count} actions registered.");
    }

    public string HandleAction(string actionName, JsonElement payload)
    {
        if (_actions.TryGetValue(actionName, out var action))
            try
            {
                return action.Execute(payload, this);
            }
            catch (Exception ex)
            {
                ModLogger.LogDebug($"Error executing action '{actionName}': {ex.Message}\n{ex.StackTrace}");
                return $"Error: {ex.Message}";
            }

        ModLogger.LogDebug($"Unknown action received: '{actionName}'");
        return $"Unknown action: {actionName}";
    }

    public void RegisterUpdatable(IUpdatableAction action)
    {
        if (action != null && !_updatableActions.Contains(action)) _updatableActions.Add(action);
    }

    public Dictionary<string, object> GetActionStates()
    {
        var states = new Dictionary<string, object>();

        var killAction = GetAction<KillAllEnemiesAction>("kill_all_enemies");
        if (killAction != null) states["kill_all_enemies"] = new { looping = killAction.IsLooping };

        var pickUpAction = GetAction<PickUpAllXpAction>("pick_up_all_xp");
        if (pickUpAction != null) states["pick_up_all_xp"] = new { looping = pickUpAction.IsLooping };

        var restartAction = GetAction<AutoRestartAction>("set_auto_restart_config");
        if (restartAction != null)
            states["set_auto_restart_config"] = new
            {
                enabled = restartAction.IsEnabled,
                itemIds = restartAction.ItemIds
            };

        return states;
    }

    public void UpdateActions()
    {
        HotkeyManager.CheckKeys(this);
        if (_updatableActions.Count == 0) return;
        foreach (var action in _updatableActions.ToList())
            try
            {
                var keepRunning = action.Update();

                if (!keepRunning) _updatableActions.Remove(action);
            }
            catch (Exception ex)
            {
                ModLogger.LogDebug($"Error in 'UpdateActions': {ex.Message}");
                _updatableActions.Remove(action);
            }
    }

    private TAction GetAction<TAction>(string name) where TAction : class, IAction
    {
        return _actions.TryGetValue(name, out var action) ? action as TAction : null;
    }


    public void StopLoopingActions(IEnumerable<string> actionNames)
    {
        if (actionNames == null) return;

        foreach (var name in actionNames) StopLoopingForAction(name);
    }

    public void StopAllLoopingActions()
    {
        StopLoopingForAction("kill_all_enemies");
        StopLoopingForAction("pick_up_all_xp");
    }

    private void StopLoopingForAction(string actionName)
    {
        if (!_actions.TryGetValue(actionName, out var action))
            return;

        var isLooping =
            (action as KillAllEnemiesAction)?.IsLooping == true ||
            (action as PickUpAllXpAction)?.IsLooping == true;

        if (!isLooping)
            return;

        using var doc = JsonDocument.Parse("{\"looping\": false}");
        try
        {
            action.Execute(doc.RootElement, this);
        }
        catch (Exception ex)
        {
            ModLogger.LogDebug($"Error stopping looping action '{actionName}': {ex.Message}");
        }
    }
}