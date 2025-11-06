using BepInEx.Logging;
using MegaBonkPlusMod.GameLogic.Common;
using MegaBonkPlusMod.GameLogic.Trackers;
using MegaBonkPlusMod.GameLogic.Minimap;
using System.Collections.Generic;
using System.Net;
using System.Linq;

namespace MegaBonkPlusMod.API
{
    public class ApiRouter
    {
        private readonly ManualLogSource _logger;
        private readonly Dictionary<string, BaseTracker> _routeRegistry;
        private readonly MinimapStreamer _minimapStreamer; 

        // Nimmt getrennte Listen entgegen
        public ApiRouter(ManualLogSource logger, List<BaseTracker> allTrackers, MinimapStreamer minimapStreamer)
        {
            _logger = logger;
            _routeRegistry = new Dictionary<string, BaseTracker>();
            
            _minimapStreamer = minimapStreamer; 

            _logger.LogInfo("ApiRouter registriert Tracker-Routen...");
            foreach (var tracker in allTrackers)
            {
                if (string.IsNullOrEmpty(tracker.ApiRoute)) continue;
                _routeRegistry[tracker.ApiRoute] = tracker;
                _logger.LogInfo($"  -> Tracker-Route '{tracker.ApiRoute}' registriert.");
            }
            if (_minimapStreamer != null)
            {
                _logger.LogInfo("  -> MinimapStreamer für API registriert.");
            }
        }

        // HandleApiRequest bleibt 100% identisch
        public bool HandleApiRequest(HttpListenerContext context)
        {
            string path = context.Request.Url.AbsolutePath;

            if (path == "/api/stream/minimap" && _minimapStreamer != null)
            {
                string jsonData = _minimapStreamer.GetJsonData();
                JsonResponse.Send(context, jsonData);
                return true;
            }
            
            if (_routeRegistry.TryGetValue(path, out BaseTracker tracker))
            {
                string jsonData = tracker.GetJsonData();
                JsonResponse.Send(context, jsonData);
                return true;
            }
            
            return false;
        }
    }
}