using Assets.Scripts.Actors.Player;
using Assets.Scripts.Game.Other;
using Assets.Scripts.Managers;
using BonkersLib.Core;
using BonkersLib.Enums;
using BonkersLib.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BonkersLib.Services;

public class GameStateService
{
    private const float WORLD_LOAD_DELAY = 0.5f;
    private float _worldLoadTimer = 0f;

    public GameStateService()
    {
        CurrentStateEnum = GameStateEnum.MainMenu;
    }

    public GameManager GameManagerInstance { get; private set; }
    public EnemyManager EnemyManagerInstance { get; private set; }
    public MyPlayer PlayerController => GameManagerInstance?.player;
    public GameStateEnum CurrentStateEnum { get; private set; }
    public RunConfig CurrentRunConfig { get; private set; }
    public MapData CurrentMapData => CurrentRunConfig?.mapData;
    public bool IsInGame => CurrentStateEnum == GameStateEnum.InGame;
    public float StageTime => GameManagerInstance?.totalStageTime ?? 0;
    public float TimeAlive => GameManagerInstance?.GetAliveTime() ?? 0;
    public int BossCurses => GameManagerInstance?.bossCurses ?? 0;
    public int StageTier => CurrentRunConfig?.mapTierIndex + 1 ?? -1;
    public string StageName => CurrentMapData?.GetName() ?? "N/A";

    public void RestartRun()
    {
        MapController.RestartRun();
    }

    internal void Update()
    {
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
        if (!GameManagerInstance)
            GameManagerInstance = Object.FindObjectOfType<GameManager>();

        if (GameManagerInstance && GameManagerInstance.player)
        {
            ModLogger.LogDebug(
                "[GameStateService] Found GameManager and Player, waiting for world objects to spawn...");

            EnemyManagerInstance = Object.FindObjectOfType<EnemyManager>();
            CurrentRunConfig = MapController.runConfig;

            CurrentStateEnum = GameStateEnum.WaitingForWorldLoad;
            _worldLoadTimer = WORLD_LOAD_DELAY;
        }
    }

    private void HandleWaitingForWorldLoadState()
    {
        _worldLoadTimer -= Time.unscaledDeltaTime;

        if (_worldLoadTimer <= 0f)
        {
            ModLogger.LogDebug("[GameStateService] World load delay complete, switching to InGame state");
            CurrentStateEnum = GameStateEnum.InGame;

            BonkersAPI.World.OnGameStarted();
        }
    }

    internal void OnSceneChanged(Scene scene)
    {
        ModLogger.LogDebug($"[GameStateService] Scene changed: {scene.name}");

        if (scene.name is "MainMenu")
        {
            CurrentStateEnum = GameStateEnum.MainMenu;
            ClearGameInstances();
            BonkersAPI.Item.CacheAllRawItems();
            BonkersAPI.World.OnSceneChanged();
        }
        else
        {
            CurrentStateEnum = GameStateEnum.Loading;
            ClearGameInstances();
            BonkersAPI.World.OnSceneChanged();
        }
    }

    private void ClearGameInstances()
    {
        GameManagerInstance = null;
        EnemyManagerInstance = null;
        CurrentRunConfig = null;
        _worldLoadTimer = 0f;
    }
}