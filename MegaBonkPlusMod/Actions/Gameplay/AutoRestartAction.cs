using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using BonkersLib.Core;
using MegaBonkPlusMod.Actions.Base;
using MegaBonkPlusMod.Utils;
using UnityEngine;

namespace MegaBonkPlusMod.Actions.Gameplay;

public class AutoRestartAction : IAction, IUpdatableAction
{
    private const float RESTART_DELAY_SECONDS = 3.0f;
    private bool _isRegistered;

    private float _restartCooldown;

    public bool IsEnabled { get; private set; }

    public List<string> ItemIds { get; } = new();

    public string Execute(JsonElement payload, ActionHandler handler)
    {
        _restartCooldown = 0;

        try
        {
            var newEnabledState = IsEnabled;
            if (payload.TryGetProperty("enabled", out var enabledElement))
                newEnabledState = enabledElement.GetBoolean();

            ItemIds.Clear();
            if (payload.TryGetProperty("itemIds", out var itemIdsElement) &&
                itemIdsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in itemIdsElement.EnumerateArray()) ItemIds.Add(item.GetString().ToLowerInvariant());
                ModLogger.LogDebug($"[AutoRestartAction] {itemIdsElement.GetArrayLength()} Item-IDs registered");
            }

            if (newEnabledState)
            {
                IsEnabled = true;
                if (!_isRegistered)
                {
                    handler.RegisterUpdatable(this);
                    _isRegistered = true;
                }

                ModLogger.LogDebug("[AutoRestartAction] activated");
                return "Started AutoRestarter";
            }

            IsEnabled = false;
            ModLogger.LogDebug("[AutoRestartAction] deactivated");
            return "Stopped AutoRestarter";
        }
        catch (Exception ex)
        {
            ModLogger.LogDebug($"[AutoRestartAction] Error processing: {ex.Message}");
            return "Failed to start AutoRestarter";
        }
    }

    public bool Update()
    {
        if (!IsEnabled)
        {
            _isRegistered = false;
            return false;
        }

        if (_restartCooldown > 0)
        {
            _restartCooldown -= Time.deltaTime;
            return true;
        }

        if (BonkersAPI.Game.IsInGame && BonkersAPI.World != null) AutoRestart();

        return true;
    }

    private void AutoRestart()
    {
        var shadyItems = BonkersAPI.World.GetEveryShadyItem();

        var shadyItemIds = shadyItems
            .Select(item => item.eItem.ToString().ToLowerInvariant())
            .ToHashSet();

        var allItemsFound = ItemIds.All(requiredId => shadyItemIds.Contains(requiredId));

        if (allItemsFound)
        {
            ModLogger.LogDebug("[AutoRestartAction] Wishlist items all found. Stopping autorestart");
            IsEnabled = false;
        }
        else
        {
            ModLogger.LogDebug("[AutoRestartAction] Not all Wishlist items found. Restarting run");
            BonkersAPI.Game.RestartRun();

            _restartCooldown = RESTART_DELAY_SECONDS;
        }
    }
}