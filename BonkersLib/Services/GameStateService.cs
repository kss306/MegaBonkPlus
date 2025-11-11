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
        if (CurrentStateEnum == GameStateEnum.Loading)
        {
            if (!GameManagerInstance) GameManagerInstance = Object.FindObjectOfType<GameManager>();

            if (GameManagerInstance && GameManagerInstance.player)
            {
                CurrentStateEnum = GameStateEnum.InGame;

                EnemyManagerInstance = Object.FindObjectOfType<EnemyManager>();
                CurrentRunConfig = MapController.runConfig;
                ModLogger.LogDebug("Loaded Managers and RunConfig");
                ;
            }
        }
    }

    internal void OnSceneChanged(Scene scene)
    {
        ModLogger.LogDebug($"Scene changed: {scene.name}");

        if (scene.name is "MainMenu")
        {
            CurrentStateEnum = GameStateEnum.MainMenu;
            ClearGameInstances();
            BonkersAPI.Item.CacheAllRawItems();
        }
        else
        {
            CurrentStateEnum = GameStateEnum.Loading;
            ClearGameInstances();
        }
    }

    private void FindGameInstances()
    {
        GameManagerInstance = Object.FindObjectOfType<GameManager>();
        EnemyManagerInstance = Object.FindObjectOfType<EnemyManager>();
        CurrentRunConfig = MapController.runConfig;
    }

    private void ClearGameInstances()
    {
        GameManagerInstance = null;
        EnemyManagerInstance = null;
        CurrentRunConfig = null;
    }
}