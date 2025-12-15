using System;
using Assets.Scripts.Actors.Player;
using Assets.Scripts.Game.Other;
using Assets.Scripts.Managers;
using BonkersLib.Core;
using BonkersLib.Enums;
using BonkersLib.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace BonkersLib.Services;

public class GameStateService
{
    private const float TIMEOUT_WORLD_LOAD_DELAY = 2.0f;
    private const float DELAY_AFTER_FOUND = 0.5f;

    private float _worldLoadTimer;
    private bool _shrinesDetected;
    private float _delayAfterFoundTimer;

    private bool _isTimeScaleLocked;
    private float _targetTimeScale = 1.0f;

    private GameManager _gameManagerInstance;
    private EnemyManager _enemyManagerInstance;
    private PickupManager _pickupManagerInstance;
    private RunConfig _currentRunConfig;

    public GameStateService()
    {
        CurrentStateEnum = GameStateEnum.MainMenu;
    }

    public GameManager GameManagerInstance => MainThreadDispatcher.Evaluate(() => _gameManagerInstance);
    public EnemyManager EnemyManagerInstance => MainThreadDispatcher.Evaluate(() => _enemyManagerInstance);
    public PickupManager PickupManagerInstance => MainThreadDispatcher.Evaluate(() => _pickupManagerInstance);
    public MyPlayer PlayerController => MainThreadDispatcher.Evaluate(() => _gameManagerInstance?.player);

    public GameStateEnum CurrentStateEnum { get; private set; }

    public bool IsInGame => MainThreadDispatcher.Evaluate(() => CurrentStateEnum == GameStateEnum.InGame);

    public RunConfig CurrentRunConfig => MainThreadDispatcher.Evaluate(() => _currentRunConfig);

    public MapData CurrentMapData => MainThreadDispatcher.Evaluate(() => _currentRunConfig?.mapData);

    public float StageTime => MainThreadDispatcher.Evaluate(() => _gameManagerInstance?.lastFoundStageTime ?? 0);
    public float TimeAlive => MainThreadDispatcher.Evaluate(() => _gameManagerInstance?.GetAliveTime() ?? 0);
    public int BossCurses => MainThreadDispatcher.Evaluate(() => _gameManagerInstance?.bossCurses ?? 0);

    public int StageTier => MainThreadDispatcher.Evaluate(() => _currentRunConfig?.mapTierIndex + 1 ?? -1);

    public string StageName => MainThreadDispatcher.Evaluate(() => CurrentMapData?.GetName() ?? "N/A");
    public float CurrentTimeScale => MainThreadDispatcher.Evaluate(() => Time.timeScale);

    public event Action GameStarted;
    public event Action SceneChanged;

    public void RestartRun()
    {
        MainThreadDispatcher.Enqueue(() =>
        {
            MapController.RestartRun();
            ModLogger.LogDebug("[GameStateService] Restart run requested");
        });
    }

    internal void Update()
    {
        
        if (_isTimeScaleLocked)
        {
            ApplyTimeScale(_targetTimeScale);
        }
        
        switch (CurrentStateEnum)
        {
            case GameStateEnum.Loading:
                HandleLoadingState();
                break;

            case GameStateEnum.WaitingForWorldLoad:
                HandleWaitingForWorldLoadState();
                break;
        }
    }

    private void HandleLoadingState()
    {
        if (!_gameManagerInstance)
            _gameManagerInstance = Object.FindObjectOfType<GameManager>();

        if (_gameManagerInstance && _gameManagerInstance.player)
        {
            ModLogger.LogDebug(
                "[GameStateService] Found GameManager and Player, waiting for world objects to spawn...");

            _enemyManagerInstance = Object.FindObjectOfType<EnemyManager>();
            _pickupManagerInstance = Object.FindObjectOfType<PickupManager>();

            _currentRunConfig = MapController.runConfig;

            CurrentStateEnum = GameStateEnum.WaitingForWorldLoad;
            _worldLoadTimer = TIMEOUT_WORLD_LOAD_DELAY;
            _shrinesDetected = false;
            _delayAfterFoundTimer = 0f;
        }
    }

    private void HandleWaitingForWorldLoadState()
    {
        if (!_shrinesDetected && HasWorldStartedBasedOnShrines())
        {
            _shrinesDetected = true;
            _delayAfterFoundTimer = DELAY_AFTER_FOUND;
            ModLogger.LogDebug(
                $"[GameStateService] Shrine detected, waiting additional {DELAY_AFTER_FOUND:0.00}s before starting game");
        }

        if (_shrinesDetected)
        {
            _delayAfterFoundTimer -= Time.unscaledDeltaTime;
            if (_delayAfterFoundTimer <= 0f)
            {
                SwitchToInGame("Detected shrines and post-delay elapsed");
                return;
            }
        }

        _worldLoadTimer -= Time.unscaledDeltaTime;
        if (_worldLoadTimer <= 0f)
        {
            ModLogger.LogDebug("[GameStateService] World load timeout reached, switching to InGame state anyway");
            SwitchToInGame("Timeout reached");
        }
    }

    internal void OnSceneChanged(Scene scene)
    {
        ModLogger.LogDebug($"[GameStateService] Scene changed: {scene.name}");

        if (scene.name is "MainMenu")
        {
            CurrentStateEnum = GameStateEnum.MainMenu;
            ClearGameInstances();
            BonkersAPI.World.OnSceneChanged();
            SceneChanged?.Invoke();
        }
        else
        {
            CurrentStateEnum = GameStateEnum.Loading;
            ClearGameInstances();
            BonkersAPI.World.OnSceneChanged();
        }
        
        SetTimeScale(1f, true);
    }

    private bool HasWorldStartedBasedOnShrines()
    {
        var greed = Object.FindObjectOfType<InteractableShrineGreed>(true);
        var magnet = Object.FindObjectOfType<InteractableShrineMagnet>(true);
        var cursed = Object.FindObjectOfType<InteractableShrineCursed>(true);

        return greed || magnet || cursed;
    }

    private void SwitchToInGame(string reason)
    {
        ModLogger.LogDebug($"[GameStateService] World load complete ({reason}), switching to InGame state");
        CurrentStateEnum = GameStateEnum.InGame;

        BonkersAPI.World.OnGameStarted();
        GameStarted?.Invoke();
    }

    private void ClearGameInstances()
    {
        _gameManagerInstance = null;
        _enemyManagerInstance = null;
        _currentRunConfig = null;
        _pickupManagerInstance = null;

        _worldLoadTimer = 0f;
        _shrinesDetected = false;
        _delayAfterFoundTimer = 0f;
    }

    public void SetTimeScale(float newScale, bool lockValue = false)
    {
        MainThreadDispatcher.Enqueue(() =>
        {
            if (newScale < 0f) newScale = 0f;

            _targetTimeScale = newScale;
            _isTimeScaleLocked = lockValue;

            ApplyTimeScale(newScale);

            ModLogger.LogDebug($"[GameStateService] TimeScale set to: {newScale} (Locked: {lockValue})");
        });
    }

    public void UnlockTimeScale()
    {
        MainThreadDispatcher.Enqueue(() =>
        {
            _isTimeScaleLocked = false;
            ModLogger.LogDebug("[GameStateService] TimeScale Lock removed.");
        });
    }

    private void ApplyTimeScale(float scale)
    {
        if (!(Math.Abs(Time.timeScale - scale) > 0.001f)) return;
        Time.timeScale = scale;
        Time.fixedDeltaTime = 0.02f * scale;
    }
}