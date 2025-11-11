using System;
using BepInEx.Logging;
using MegaBonkPlusMod.GameLogic.Common;
using MegaBonkPlusMod.GameLogic.Trackers;
using MegaBonkPlusMod.GameLogic.Minimap;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;

using Assets.Scripts.Actors.Player;
using MegaBonkPlusMod.Actions;
using Object = UnityEngine.Object;


namespace MegaBonkPlusMod.API
{
    public class ApiRouter
    {
        private readonly ManualLogSource _logger;
        private readonly Dictionary<string, BaseTracker> _routeRegistry;
        private readonly MinimapStreamer _minimapStreamer; 
        private readonly ActionHandler _actionHandler;

        public ApiRouter(ManualLogSource logger, List<BaseTracker> allTrackers, MinimapStreamer minimapStreamer, ActionHandler actionHandler)
        {
            _logger = logger;
            _routeRegistry = new Dictionary<string, BaseTracker>();
            _minimapStreamer = minimapStreamer; 
            _actionHandler = actionHandler;

            _logger.LogInfo("ApiRouter registering Tracker-Routes...");
            foreach (var tracker in allTrackers)
            {
                if (string.IsNullOrEmpty(tracker.ApiRoute)) continue;
                _routeRegistry[tracker.ApiRoute] = tracker;
                _logger.LogInfo($"  -> Tracker-Route '{tracker.ApiRoute}' registered.");
            }
            if (_minimapStreamer != null)
            {
                _logger.LogInfo("  -> MinimapStreamer for API registered");
            }
        }
        
        public bool HandleApiGetRequest(HttpListenerContext context)
        {
            string path = context.Request.Url.AbsolutePath;

            if (path == "/api/stream/minimap" && _minimapStreamer != null)
            {
                string jsonData = _minimapStreamer.GetJsonData();
                JsonResponse.Send(context, jsonData);
                return true;
            }
            
            if (path == "/api/actions/state")
            {
                var states = _actionHandler.GetActionStates();
                string jsonData = JsonSerializer.Serialize(states);
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
        
        public void HandleApiPostRequest(HttpListenerContext context)
        {
            string jsonPayload;
            try
            {
                using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding)) {
                    jsonPayload = reader.ReadToEnd();
                }
                
                var doc = JsonDocument.Parse(jsonPayload);
                var root = doc.RootElement;
                
                if (!root.TryGetProperty("action", out var actionElement))
                {
                    throw new Exception("Payload is missing the 'action' property.");
                }
                string action = actionElement.GetString();

                MainThreadActionQueue.QueueAction(() => {
                    ExecuteAction(action, root);
                });
                JsonResponse.Send(context, "{\"status\":\"ok\"}", 200, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Fehler bei POST-Anfrage: {ex.Message}");
                JsonResponse.Send(context, "{\"status\":\"error\"}", 500, "application/json");
            }
        }
        
        private void ExecuteAction(string actionName, JsonElement payload)
        {
            var player = Object.FindObjectOfType<MyPlayer>();
            if (!player)
            {
                _logger.LogWarning($"Action '{actionName}' failed: Player not found.");
                return;
            }

            _logger.LogInfo($"Routing action '{actionName}' to the ActionHandler...");
            _actionHandler.HandleAction(actionName, payload, player);
        }
    }
}