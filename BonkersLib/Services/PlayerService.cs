using System;
using System.Linq;
using Assets.Scripts.Actors.Player;
using Assets.Scripts.Inventory__Items__Pickups;
using Assets.Scripts.Inventory__Items__Pickups.Stats;
using Assets.Scripts.Menu.Shop;
using BonkersLib.Core;
using BonkersLib.Utils;
using Il2CppSystem.Collections.Generic;
using Inventory__Items__Pickups.Xp_and_Levels;
using UnityEngine;

namespace BonkersLib.Services;

public class PlayerService
{
    private const float SAFE_RANGE = 15.0f;
    private const float Y_OFFSET = 10.0f;

    private Vector3 _teleportTarget;
    private bool _tryingToTeleport;

    internal PlayerService()
    {
    }

    private MyPlayer _player => BonkersAPI.Game.PlayerController;

    public string CharacterName => MainThreadDispatcher.Evaluate(() =>
        _player ? _player.character.ToString() : "N/A");

    public Vector3 Position => MainThreadDispatcher.Evaluate(() =>
        _player ? _player.feet.transform.position : Vector3.zero);

    public int InstanceId => MainThreadDispatcher.Evaluate(() =>
        _player ? _player.gameObject.GetInstanceID() : -1);

    public GameObject GameObject => MainThreadDispatcher.Evaluate(() => _player?.gameObject);

    public PlayerInventory Inventory => MainThreadDispatcher.Evaluate(() => _player?.inventory);

    public PlayerHealth PlayerHealth =>
        MainThreadDispatcher.Evaluate(() => _player?.inventory?.playerHealth);

    public PlayerXp PlayerXp => MainThreadDispatcher.Evaluate(() => _player?.inventory?.playerXp);

    public PlayerStatsNew Stats => MainThreadDispatcher.Evaluate(() => _player?.inventory?.playerStats);

    public Dictionary<EStat, float> StatsDict =>
        MainThreadDispatcher.Evaluate(() => _player?.inventory?.playerStats?.stats.ToSafeCopy());

    public Dictionary<EStat, float> RawStatsDict =>
        MainThreadDispatcher.Evaluate(() => _player?.inventory?.playerStats?.rawStats.ToSafeCopy());

    public int Health => MainThreadDispatcher.Evaluate(() => PlayerHealth?.hp ?? 0);
    public int MaxHealth => MainThreadDispatcher.Evaluate(() => PlayerHealth?.maxHp ?? 0);
    public float Shield => MainThreadDispatcher.Evaluate(() => PlayerHealth?.shield ?? 0);
    public float MaxShield => MainThreadDispatcher.Evaluate(() => PlayerHealth?.maxShield ?? 0);
    public float Gold => MainThreadDispatcher.Evaluate(() => Inventory?.gold ?? 0);
    public int Level => MainThreadDispatcher.Evaluate(() => Inventory?.GetCharacterLevel() ?? 0);


    internal void Update()
    {
        if (!BonkersAPI.Game.IsInGame) return;

        if (_tryingToTeleport)
        {
            TryToTeleport();
        }
    }

    private void TryToTeleport()
    {
        if (!_player) return;

        var distance = Vector3.Distance(_player.transform.position, _teleportTarget);

        if (distance <= SAFE_RANGE)
        {
            _tryingToTeleport = false;
            ModLogger.LogDebug("[PlayerService] Teleport completed/arrived");
        }
        else
        {
            _player.transform.position = _teleportTarget;
        }
    }

    public void TeleportTo(Vector3 targetPosition)
    {
        MainThreadDispatcher.Enqueue(() =>
        {
            if (!BonkersAPI.Game.IsInGame) return;

            _teleportTarget = targetPosition + new Vector3(0, Y_OFFSET, 0);
            _tryingToTeleport = true;

            ModLogger.LogDebug(
                $"[PlayerService] Starting teleport to {_teleportTarget} from {_player.transform.position}");
        });
    }

    public void GiveGold(int amount)
    {
        MainThreadDispatcher.Enqueue(() =>
        {
            if (!BonkersAPI.Game.IsInGame) return;
            _player?.inventory?.ChangeGold(amount);
            ModLogger.LogDebug($"[PlayerService] Gave {amount} gold to player");
        });
    }

    public void SetGold(int amount)
    {
        MainThreadDispatcher.Enqueue(() =>
        {
            if (!BonkersAPI.Game.IsInGame || _player?.inventory == null) return;
            _player.inventory.gold = amount;
            BonkersAPI.Ui.RefreshUi();
            ModLogger.LogDebug($"[PlayerService] Set gold to {amount}");
        });
    }

    public void AddLevel(int amount)
    {
        MainThreadDispatcher.Enqueue(() =>
        {
            if (!BonkersAPI.Game.IsInGame || _player?.inventory == null) return;
            for (var i = 0; i < amount; i++)
                _player.inventory.AddLevel();

            ModLogger.LogDebug($"[PlayerService] Added {amount} Levels to player");
        });
    }

    public void GiveItem(string itemId, int quantity)
    {
        MainThreadDispatcher.Enqueue(() =>
        {
            if (!BonkersAPI.Game.IsInGame || _player?.inventory == null) return;

            var rawItemData = BonkersAPI.Item.GetAllRawItems()
                .FirstOrDefault(item =>
                    string.Equals(item.eItem.ToString(), itemId, StringComparison.OrdinalIgnoreCase));

            if (rawItemData)
            {
                _player.inventory.itemInventory.AddItem(rawItemData.eItem, quantity);
                ModLogger.LogDebug($"[PlayerService] Gave Player Item: {quantity}x {itemId}");
            }
            else
            {
                ModLogger.LogDebug($"[PlayerService] Item-ID '{itemId}' could not be found");
            }
        });
    }

    public void PickUpAllXp()
    {
        MainThreadDispatcher.Enqueue(() =>
        {
            if (!BonkersAPI.Game.IsInGame) return;
            BonkersAPI.Game.PickupManagerInstance?.PickupAllXp();
            ModLogger.LogDebug("[PlayerService] Picked up all XP");
        });
    }
}