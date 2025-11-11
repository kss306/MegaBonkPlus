using System;
using Assets.Scripts.Actors.Player;
using Assets.Scripts.Game.Other;
using Assets.Scripts.Managers;
using BepInEx.Logging;
using BonkersLib.Enums;
using UnityEngine.SceneManagement;

namespace BonkersLib.Services;

public class GameStateService
{
    private readonly ManualLogSource _log;

    public GameManager GameManagerInstance { get; private set; }
    public EnemyManager EnemyManagerInstance { get; private set; }
    public MyPlayer PlayerController => GameManagerInstance?.player;
    public GameState CurrentState { get; private set; }
    public RunConfig CurrentRunConfig { get; private set; }
    public MapData CurrentMapData => CurrentRunConfig?.mapData;
    public bool IsInGame => CurrentState == GameState.InGame;
    public float StageTime => GameManagerInstance?.totalStageTime ?? 0;
    public float TimeAlive => GameManagerInstance?.GetAliveTime() ?? 0;
    public int BossCurses => GameManagerInstance?.bossCurses ?? 0;
    public int StageTier => CurrentRunConfig?.mapTierIndex + 1 ?? -1;
    public string StageName => CurrentMapData?.GetName() ?? "N/A";
    public void RestartRun() => MapController.RestartRun();

    public GameStateService(ManualLogSource log)
    {
        _log = log;
        CurrentState = GameState.MainMenu;
    }

    internal void Update()
    {
        if (CurrentState == GameState.Loading)
        {
            if (!GameManagerInstance)
            {
                GameManagerInstance = UnityEngine.Object.FindObjectOfType<GameManager>();
            }
            
            if (GameManagerInstance && GameManagerInstance.player)
            {
                CurrentState = GameState.InGame;
                
                EnemyManagerInstance = UnityEngine.Object.FindObjectOfType<EnemyManager>();
                CurrentRunConfig = MapController.runConfig;
            }
        }
    }

    internal void OnSceneChanged(Scene scene)
    {
        _log.LogInfo($"Szene gewechselt: {scene.name}");

        if (scene.name is "MainMenu")
        {
            CurrentState = GameState.MainMenu;
            ClearGameInstances();
        }
        else
        {
            CurrentState = GameState.Loading;
            ClearGameInstances();
        }
    }

    private void FindGameInstances()
    {
        GameManagerInstance = UnityEngine.Object.FindObjectOfType<GameManager>();
        EnemyManagerInstance = UnityEngine.Object.FindObjectOfType<EnemyManager>();
        CurrentRunConfig = MapController.runConfig;
    }

    private void ClearGameInstances()
    {
        GameManagerInstance = null;
        EnemyManagerInstance = null;
        CurrentRunConfig = null;
    }
}