using System;
using System.Collections.Generic;
using Assets.Scripts.Actors.Player;
using BepInEx.Logging;
using MegaBonkPlusMod.Models;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MegaBonkPlusMod.GameLogic.Trackers;

public class PlayerTracker : BaseTracker
{
    private MyPlayer _player;
    private Transform _playerTransform;

    public PlayerTracker(ManualLogSource logger, float scanIntervalInSeconds) : base(logger, scanIntervalInSeconds)
    {
        Logger.LogInfo("PlayerTracker (Echtzeit, Super-Slim) initialisiert.");
    }

    public override string ApiRoute => "/api/tracker/player";

    protected override object BuildDataPayload()
    {
        var trackedObjects = new List<TrackedObjectData>();

        if (!_playerTransform)
        {
            _player = Object.FindObjectOfType<MyPlayer>();
            if (_player)
            {
                _playerTransform = _player.transform;
                Logger.LogInfo("PlayerTracker: Spieler-Transform gefunden!");
            }
        }

        if (_playerTransform)
        {
            var playerData = new TrackedObjectData
            {
                Position = PositionData.FromVector3(_playerTransform.position)
            };
            playerData.CustomProperties["character"] = _player.character.ToString();
            trackedObjects.Add(playerData);
        }

        return new ApiListResponse<TrackedObjectData>(trackedObjects);
    }

    protected override void OnTrackerError(Exception ex)
    {
        Logger.LogWarning($"PlayerTracker-Fehler: {ex.Message}");
        _playerTransform = null;
    }
}