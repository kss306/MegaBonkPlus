using BonkersLib.Core;
using MegaBonkPlusMod.Actions.Base;
using MegaBonkPlusMod.GameLogic.Minimap;
using MegaBonkPlusMod.Infrastructure.Http;
using MegaBonkPlusMod.Infrastructure.Http.Controllers;
using MegaBonkPlusMod.Infrastructure.Services;
using MegaBonkPlusMod.Utils;
using UnityEngine;

namespace MegaBonkPlusMod.Core;

public class ModManager : MonoBehaviour
{
    private ActionHandler _actionHandler;
    private MinimapCaptureService _minimapCaptureService;
    private HttpServer _server;
    private TrackerRegistryService _trackerRegistry;

    private void Update()
    {
        MainThreadActionQueue.ExecuteAll();
        _trackerRegistry.UpdateAll();
        _actionHandler.UpdateActions();
        _minimapCaptureService.Update();
    }

    private void OnDestroy()
    {
        ModLogger.LogDebug("Object destroyed, stopping HttpServer…");
        BonkersAPI.Game.GameStarted -= OnGameStarted;
        _server?.Stop();
    }

    private void OnApplicationQuit()
    {
        ModLogger.LogDebug("Application quitting, stopping HttpServer…");
        BonkersAPI.Game.GameStarted -= OnGameStarted;
        _server?.Stop();
    }

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
            new HotkeyController(_actionHandler),
            new ActionController(_actionHandler),
            new TrackerController(_trackerRegistry.TrackersDictionary),
            new MinimapController(minimapStreamer),
            new GameStateController(),
            new InventoryController()
        );

        _server = new HttpServer(controllerRouter);
        _server.Start();

        BonkersAPI.Game.GameStarted += OnGameStarted;

        ModLogger.LogDebug("ModManager initialized.");
    }

    private void OnGameStarted()
    {
        ModLogger.LogDebug("New run detected, starting minimap capture...");
        _minimapCaptureService.StartCapture();
        _actionHandler.StopAllLoopingActions();
    }
}