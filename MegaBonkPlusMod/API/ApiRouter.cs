using System.Collections.Generic;
using System.Linq;
using System.Net;
using BepInEx.Logging;
using MegaBonkPlusMod.GameLogic.Common;
using MegaBonkPlusMod.GameLogic.Minimap;
using MegaBonkPlusMod.GameLogic.Trackers;

namespace MegaBonkPlusMod.API;

public class ApiRouter
{
    private readonly ManualLogSource _logger;
    private readonly MinimapStreamer _minimapStreamer;
    private readonly Dictionary<string, BaseTracker> _routeRegistry;

    public ApiRouter(ManualLogSource logger, List<BasePollingProvider> allProviders, MinimapStreamer minimapStreamer)
    {
        _logger = logger;
        _routeRegistry = new Dictionary<string, BaseTracker>();

        _minimapStreamer = minimapStreamer;

        var trackers = allProviders.OfType<BaseTracker>();
        _logger.LogInfo("ApiRouter registriert Tracker-Routen...");
        foreach (var tracker in trackers)
        {
            if (string.IsNullOrEmpty(tracker.ApiRoute)) continue;
            _routeRegistry[tracker.ApiRoute] = tracker;
            _logger.LogInfo($"  -> Tracker-Route '{tracker.ApiRoute}' registriert.");
        }

        if (_minimapStreamer != null) _logger.LogInfo("  -> MinimapStreamer für API registriert.");
    }

    public bool HandleApiRequest(HttpListenerContext context)
    {
        var path = context.Request.Url.AbsolutePath;

        if (path == "/api/stream/minimap" && _minimapStreamer != null)
        {
            var jsonData = _minimapStreamer.GetJsonData();
            JsonResponse.Send(context, jsonData);
            return true;
        }

        if (_routeRegistry.TryGetValue(path, out var tracker))
        {
            var jsonData = tracker.GetJsonData();
            JsonResponse.Send(context, jsonData);
            return true;
        }

        return false;
    }
}