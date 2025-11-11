using Assets.Scripts.Actors.Player;
using Assets.Scripts.Inventory__Items__Pickups;
using Assets.Scripts.Inventory__Items__Pickups.Stats;
using Assets.Scripts.Menu.Shop;
using BepInEx.Logging;
using BonkersLib.Core;
using UnityEngine;
using BonkersLib.Utils;
using Il2CppSystem.Collections.Generic;
using Inventory__Items__Pickups.Xp_and_Levels;

namespace BonkersLib.Services;

public class PlayerService
{
    private const float SAFE_RANGE = 15.0f;
    private const float Y_OFFSET = 10.0f;
    
    private readonly ManualLogSource _log;

    private MyPlayer _player => BonkersAPI.Game.PlayerController;
    
    //Teleport Actions
    private bool _tryingToTeleport;
    private Vector3 _teleportTarget;

    internal PlayerService(ManualLogSource log)
    {
        _log = log;
    }

    internal void Update()
    {
        if (!BonkersAPI.Game.IsInGame) return;

        if(_tryingToTeleport) TryToTeleport();
    }

    public string CharacterName => _player?.character.ToString() ?? "N/A";
    public Vector3 Position => _player?.feet.transform.position ?? Vector3.zero;
    public int InstanceId => _player?.gameObject.GetInstanceID() ?? -1;
    public GameObject GameObject => _player?.gameObject;

    public PlayerInventory Inventory => _player?.inventory;
    public PlayerStatsNew Stats => Inventory?.playerStats;
    public Dictionary<EStat, float> StatsDict => Stats?.stats ?? new Dictionary<EStat, float>();
    public Dictionary<EStat, float> RawStatsDict => Stats?.rawStats ?? new Dictionary<EStat, float>();
    public PlayerHealth PlayerHealth => Inventory?.playerHealth;
    public PlayerXp PlayerXp => Inventory?.playerXp;

    public int Health => PlayerHealth?.hp ?? 0;
    public int MaxHealth => PlayerHealth?.maxHp ?? 0;
    public float Shield => PlayerHealth?.shield ?? 0;
    public float MaxShield => PlayerHealth?.maxShield ?? 0;

    public float Gold => Inventory?.gold ?? 0;
    public int Level => Inventory?.GetCharacterLevel() ?? 0;

    public void GiveGold(int amount)
    {
        if (!BonkersAPI.Game.IsInGame) return;
        Inventory?.ChangeGold(amount);
    }

    public void SetGold(int amount)
    {
        if (!BonkersAPI.Game.IsInGame || Inventory == null) return;
        Inventory.gold = amount;
    }

    public void AddLevel(int amount)
    {
        if (!BonkersAPI.Game.IsInGame) return;
        for (int i = 0; i < amount; i++)
        {
            Inventory?.AddLevel();
        }
    }

    public void TeleportTo(Vector3 targetPosition)
    {
        if (!BonkersAPI.Game.IsInGame) return;
        _teleportTarget = targetPosition + new Vector3(0, Y_OFFSET, 0);
        _tryingToTeleport = true;
    }
    
    private void TryToTeleport()
    {
        if (!_player) return;
        
        float distance = Vector3.Distance(_player.transform.position, _teleportTarget);
            
        if (distance <= SAFE_RANGE)
            _tryingToTeleport = false;
            
        _player.transform.position = _teleportTarget;
    }
}