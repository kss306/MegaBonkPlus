using System;
using System.Collections.Generic;
using BepInEx.Logging;
using MegaBonkPlusMod.API;
using MegaBonkPlusMod.GameLogic.Common;
using MegaBonkPlusMod.GameLogic.Minimap;
using MegaBonkPlusMod.GameLogic.Trackers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MegaBonkPlusMod.Core;

public class ModManager : MonoBehaviour
{
    private const float MINIMAP_LOAD_DELAY = 2.0f;
    public static bool IsInGame;

    private ManualLogSource _logger;
    private MinimapStreamer _minimapStreamer;
    private float _minimapTriggerTimer;

    private bool _pendingMinimapUpdate;

    private readonly List<BasePollingProvider> _providers = new();
    private ApiRouter _router;
    private HttpServer _server;

    private void Update()
    {
        if (_pendingMinimapUpdate)
        {
            _minimapTriggerTimer -= Time.deltaTime;
            if (_minimapTriggerTimer <= 0f)
            {
                _logger.LogInfo("[ModManager] Manueller Timer abgelaufen. Löse Minimap-Update aus.");
                try
                {
                    _minimapStreamer.TriggerMinimapUpdate();
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        $"[ModManager] FEHLER beim Auslösen von TriggerMinimapUpdate: {ex.Message}\n{ex.StackTrace}");
                }

                _pendingMinimapUpdate = false;
            }
        }

        foreach (var provider in _providers) provider.Update();
    }

    private void OnDestroy()
    {
        _logger.LogInfo("ModManager wird zerstört, stoppe Server...");
        _server?.Stop();
        SceneManager.activeSceneChanged -= new Action<Scene, Scene>(OnSceneChanged);
    }

    public void Initialize(ManualLogSource logger)
    {
        _logger = logger;
        _logger.LogInfo("ModManager wird initialisiert...");

        _logger.LogInfo("Erstelle alle Datenanbieter...");

        _minimapStreamer = new MinimapStreamer(logger);

        _providers.Add(new PlayerTracker(logger, 0.1f));
        _providers.Add(new BossSpawnerTracker(logger, 5.0f));
        _providers.Add(new ShadyGuyTracker(logger, 5.0f));
        _providers.Add(new ChestTracker(logger, 2.0f));
        _providers.Add(new ChargeShrineTracker(logger, 2.0f));
        _providers.Add(new MaoiShrineTracker(logger, 2.0f));
        _providers.Add(new CursedShrineTracker(logger, 2.0f));
        _providers.Add(new GreedShrineTracker(logger, 2.0f));
        _providers.Add(new MagnetShrineTracker(logger, 2.0f));

        _logger.LogInfo($"Insgesamt {_providers.Count} Tracker und 1 Streamer geladen.");

        _router = new ApiRouter(logger, _providers, _minimapStreamer);
        _server = new HttpServer(logger, _router);
        _server.Start();

        SceneManager.activeSceneChanged += new Action<Scene, Scene>(OnSceneChanged);
        SetInitialSceneState(SceneManager.GetActiveScene());
    }

    private void SetInitialSceneState(Scene scene)
    {
        if (scene.name.Contains("Menu") || scene.name.Contains("Loading") || scene.name.Contains("Splash"))
            IsInGame = false;
        else
            IsInGame = true;
        _logger.LogInfo($"Initiale Szene '{scene.name}' erkannt. IsInGame = {IsInGame}");
    }


    private void OnSceneChanged(Scene current, Scene next)
    {
        if (!next.name.Contains("GeneratedMap"))
        {
            IsInGame = false;
            _logger.LogInfo($"Szene gewechselt zu '{next.name}'. Anbieter werden pausiert.");
            _pendingMinimapUpdate = false;
            _minimapStreamer.ClearData();
        }
        else
        {
            IsInGame = true;
            _logger.LogInfo($"Szene gewechselt zu '{next.name}'. Anbieter werden aktiviert.");

            _logger.LogInfo("[ModManager] Lösche alte Minimap-Daten...");
            _minimapStreamer.ClearData();

            _logger.LogInfo($"[ModManager] Minimap-Update-Timer gestartet ({MINIMAP_LOAD_DELAY}s).");
            _pendingMinimapUpdate = true;
            _minimapTriggerTimer = MINIMAP_LOAD_DELAY;
        }
    }
}