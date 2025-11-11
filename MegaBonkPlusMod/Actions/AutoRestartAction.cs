using System;
using System.Collections.Generic;
using System.Text.Json;
using Assets.Scripts.Actors.Player;
using BepInEx.Logging;
using BonkersLib.Core;
using System.Linq;
using MegaBonkPlusMod.Utils;
using UnityEngine;

namespace MegaBonkPlusMod.Actions;

public class AutoRestartAction : IAction, IUpdatableAction
{
    private bool _enabled;
    private bool _isRegistered;
    private readonly List<string> _itemIds = new();
    
    private float _restartCooldown;
    private const float RESTART_DELAY_SECONDS = 3.0f;

    public bool IsEnabled => _enabled;
    public List<string> ItemIds => _itemIds;

    public void Execute(JsonElement payload, ActionHandler handler)
    {
        _restartCooldown = 0;
        
        try
        {
            bool newEnabledState = _enabled;
            if (payload.TryGetProperty("enabled", out var enabledElement))
            {
                newEnabledState = enabledElement.GetBoolean();
            }
            
            _itemIds.Clear();
            if (payload.TryGetProperty("itemIds", out var itemIdsElement) && itemIdsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in itemIdsElement.EnumerateArray())
                {
                    _itemIds.Add(item.GetString().ToLowerInvariant());
                }
                ModLogger.LogDebug($"[AutoRestartAction] {itemIdsElement.GetArrayLength()} Item-IDs registered");
            }

            if (newEnabledState)
            {
                _enabled = true;
                if (!_isRegistered)
                {
                    handler.RegisterUpdatable(this);
                    _isRegistered = true;
                }
                ModLogger.LogDebug("[AutoRestartAction] activated");
            }
            else
            {
                _enabled = false; 
                ModLogger.LogDebug("[AutoRestartAction] deactivated");
            }
        }
        catch (Exception ex)
        {
            ModLogger.LogDebug($"[AutoRestartAction] Error processing: {ex.Message}");
        }
    }
    
    private void AutoRestart()
    {
        var shadyItems = BonkersAPI.World.GetEveryShadyItem();
        
        var shadyItemIds = shadyItems
            .Select(item => item.eItem.ToString().ToLowerInvariant())
            .ToHashSet();

        bool allItemsFound = _itemIds.All(requiredId => shadyItemIds.Contains(requiredId));

        if (allItemsFound)
        {
            ModLogger.LogDebug("[AutoRestartAction] Wishlist items all found. Stopping autorestart");
            _enabled = false;
        }
        else
        {
            ModLogger.LogDebug("[AutoRestartAction] Not all Wishlist items found. Restarting run");
            BonkersAPI.Game.RestartRun();
            
            _restartCooldown = RESTART_DELAY_SECONDS;
        }
    }

    public bool Update()
    {
        if (!_enabled)
        {
            _isRegistered = false;
            return false;
        }
        
        if (_restartCooldown > 0)
        {
            _restartCooldown -= Time.deltaTime;
            return true;
        }
        
        if (BonkersAPI.Game.IsInGame && BonkersAPI.World != null)
        {
            AutoRestart();
        }

        return true;
    }
}