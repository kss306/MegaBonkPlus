using System;
using System.Collections.Generic;
using BepInEx.Logging;
using MegaBonkPlusMod.Actions;
using MegaBonkPlusMod.API;
using MegaBonkPlusMod.GameLogic.Common;
using MegaBonkPlusMod.GameLogic.Minimap;
using MegaBonkPlusMod.GameLogic.Trackers;
using MegaBonkPlusMod.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MegaBonkPlusMod.Core
{
    public class ModManager : MonoBehaviour
    {
        public static bool IsInGame = false;

        private HttpServer _server;
        private ApiRouter _router;

        private readonly List<BaseTracker> _trackers = new List<BaseTracker>();
        private MinimapStreamer _minimapStreamer;
        private ActionHandler _actionHandler;

        private enum CaptureState
        {
            Idle,
            WaitingForSpawn,
            WaitingForMapDelay,
            HidingIcons,
            TakingPicture
        }

        private CaptureState _captureState = CaptureState.Idle;
        private float _captureTimer = 0f;

        private const float SPAWN_DELAY = 0.5f;
        private const float MINIMAP_CAPTURE_DELAY = 1.5f;

        public void Initialize()
        {
            ModLogger.LogDebug("ModManager initializing...");

            ModLogger.LogDebug("Creating API-Router and Trackers...");

            _minimapStreamer = new MinimapStreamer();
            _actionHandler = new ActionHandler();

            _trackers.Add(new PlayerTracker(0.1f));
            _trackers.Add(new BossSpawnerTracker(5.0f));
            _trackers.Add(new ShadyGuyTracker(5.0f));
            _trackers.Add(new ChestTracker(2.0f));
            _trackers.Add(new ChargeShrineTracker(2.0f));
            _trackers.Add(new MaoiShrineTracker(2.0f));
            _trackers.Add(new CursedShrineTracker(2.0f));
            _trackers.Add(new GreedShrineTracker(2.0f));
            _trackers.Add(new MagnetShrineTracker(2.0f));
            _trackers.Add(new MicrowaveTracker(2.0f));
            _trackers.Add(new ChallengeShrineTracker(2.0f));
            
            _router = new ApiRouter(_trackers, _minimapStreamer, _actionHandler);
            _server = new HttpServer(_router);
            _server.Start();
            
            ModLogger.LogDebug("ModManager initialized.");

            SceneManager.activeSceneChanged += new Action<Scene, Scene>(OnSceneChanged);
            SetInitialSceneState(SceneManager.GetActiveScene());
        }

        private void SetInitialSceneState(Scene scene)
        {
            if (!scene.name.Contains("GeneratedMap"))
                IsInGame = false;
            else
                IsInGame = true;
        }

        private void OnSceneChanged(Scene current, Scene next)
        {
            if (!next.name.Contains("GeneratedMap"))
            {
                IsInGame = false;
                _captureState = CaptureState.Idle;
                _minimapStreamer.ClearData();
            }
            else
            {
                IsInGame = true;
                _minimapStreamer.ClearData();
                _captureState = CaptureState.WaitingForSpawn;
                _captureTimer = SPAWN_DELAY;
            }
        }


        private void Update()
        {
            
            MainThreadActionQueue.ExecuteAll();
            _actionHandler.UpdateActions();
            
            switch (_captureState)
            {
                case CaptureState.WaitingForSpawn:
                {
                    _captureTimer -= Time.deltaTime;
                    if (_captureTimer <= 0f)
                    {
                        foreach (var tracker in _trackers)
                        {
                            tracker.ForceUpdatePayload();
                        }

                        _captureState = CaptureState.WaitingForMapDelay;
                        _captureTimer = MINIMAP_CAPTURE_DELAY;
                    }

                    break;
                }

                case CaptureState.WaitingForMapDelay:
                {
                    _captureTimer -= Time.deltaTime;
                    if (_captureTimer <= 0f)
                    {
                        try
                        {
                            foreach (var tracker in _trackers)
                            {
                                tracker.HideIcons();
                            }

                            Canvas.ForceUpdateCanvases();
                            _captureState = CaptureState.HidingIcons;
                        }
                        catch (Exception ex)
                        {
                            _captureState = CaptureState.Idle;
                            ModLogger.LogDebug($"Error hiding Icons: {ex.Message}");
                        }
                    }

                    break;
                }

                case CaptureState.HidingIcons:
                {
                    _captureState = CaptureState.TakingPicture;

                    try
                    {
                        _minimapStreamer.TriggerMinimapUpdate();
                    }
                    catch (Exception ex)
                    {
                        ModLogger.LogDebug($"Error creating the MinimapImage: {ex.Message}");
                    }
                    finally
                    {
                        foreach (var tracker in _trackers)
                        {
                            tracker.ShowIcons();
                        }
                    }

                    _captureState = CaptureState.Idle;
                    break;
                }

                case CaptureState.TakingPicture:
                case CaptureState.Idle:
                {
                    break;
                }
            }

            foreach (var provider in _trackers)
            {
                provider.Update();
            }
        }

        private void OnDestroy()
        {
            ModLogger.LogDebug("Stopping WebServer...");
            _server?.Stop();
            SceneManager.activeSceneChanged -= new Action<Scene, Scene>(OnSceneChanged);
        }
    }
}