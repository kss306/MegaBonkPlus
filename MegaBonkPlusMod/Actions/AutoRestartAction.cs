using System;
using System.Collections.Generic;
using System.Text.Json;
using Assets.Scripts.Actors.Player;
using BepInEx.Logging;
using BonkersLib.Core;
using System.Linq;
using UnityEngine;

namespace MegaBonkPlusMod.Actions;

public class AutoRestartAction : IAction, IUpdatableAction
{
    private bool _enabled;
    private bool _isRegistered;
    private List<string> _itemIds = new();
    private ManualLogSource _logger;
    
    private float _restartCooldown;
    private const float RESTART_DELAY_SECONDS = 3.0f;

    public bool IsEnabled => _enabled;

    public void Execute(JsonElement payload, MyPlayer player, ManualLogSource logger, ActionHandler handler)
    {
        _logger = logger;
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
                logger.LogInfo($"[AutoRestartAction] {itemIdsElement.GetArrayLength()} Item-IDs registriert.");
            }

            if (newEnabledState)
            {
                _enabled = true;
                if (!_isRegistered)
                {
                    handler.RegisterUpdatable(this);
                    _isRegistered = true;
                }
                logger.LogInfo("[AutoRestartAction] Aktiviert.");
            }
            else
            {
                _enabled = false; 
                logger.LogInfo("[AutoRestartAction] Deaktiviert.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"[AutoRestartAction] Fehler beim Verarbeiten: {ex.Message}");
        }
    }
    
    private void AutoRestart()
    {
        var shadyGuys = BonkersAPI.World.GetShadyGuys();
        if (!shadyGuys.Any())
        {
            _logger?.LogInfo("[AutoRestartAction] Warten auf ShadyGuy-Spawn...");
            return;
        }
        
        var shadyItems = BonkersAPI.World.GetEveryShadyItem();
        
        var shadyItemIds = shadyItems
            .Select(item => item.eItem.ToString().ToLowerInvariant())
            .ToHashSet();

        bool allItemsFound = _itemIds.All(requiredId => shadyItemIds.Contains(requiredId));

        if (allItemsFound)
        {
            _logger?.LogInfo("[AutoRestartAction] Alle gewünschten Items gefunden. Auto-Restart wird deaktiviert.");
            _enabled = false;
        }
        else
        {
            _logger?.LogInfo("[AutoRestartAction] Nicht alle Items gefunden. Starte Run neu...");
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