using System.Collections.Generic;
using BonkersLib.Core;
using MegaBonkPlusMod.Actions.Base;
using MegaBonkPlusMod.GameLogic.Minimap;
using MegaBonkPlusMod.Infrastructure.Http;
using MegaBonkPlusMod.Infrastructure.Http.Controllers;
using MegaBonkPlusMod.Infrastructure.Services;
using MegaBonkPlusMod.Utils;
using UnityEngine;

namespace MegaBonkPlusMod.Core
{
    public class ModManager : MonoBehaviour
    {
        private HttpServer _server;
        private TrackerRegistryService _trackerRegistry;
        private MinimapCaptureService _minimapCaptureService;
        private ActionHandler _actionHandler;
        private bool _wasInGame = false;

        public void Initialize()
        {
            ModLogger.LogDebug("ModManager initializing...");

            _trackerRegistry = new TrackerRegistryService();
            _trackerRegistry.RegisterDefaultTrackers();

            var minimapStreamer = new MinimapStreamer();
            _minimapCaptureService = new MinimapCaptureService(_trackerRegistry.TrackersList, minimapStreamer);

            _actionHandler = new ActionHandler();

            var controllerRouter = new ControllerRouter();
            controllerRouter.RegisterControllers(
                new ItemController(),
                new HotkeyController(),
                new ActionController(_actionHandler),
                new TrackerController(_trackerRegistry.TrackersDictionary),
                new MinimapController(minimapStreamer),
                new GameStateController()
            );

            _server = new HttpServer(controllerRouter);
            _server.Start();

            ModLogger.LogDebug("ModManager initialized.");
        }

        private void Update()
        {
            MainThreadActionQueue.ExecuteAll();
            _trackerRegistry.UpdateAll();
            _actionHandler.UpdateActions();

            bool isNowInGame = BonkersAPI.Game.IsInGame;
            if (isNowInGame && !_wasInGame)
            {
                ModLogger.LogDebug("New run detected, starting minimap capture...");
                _minimapCaptureService.StartCapture();
            }

            _wasInGame = isNowInGame;

            _minimapCaptureService.Update();
        }

        private void OnDestroy()
        {
            ModLogger.LogDebug("Object destroyed, stopping HttpServer…");
            _server?.Stop();
        }
        
        private void OnApplicationQuit()
        {
            ModLogger.LogDebug("Application quitting, stopping HttpServer…");
            _server?.Stop();
        }
    }
}