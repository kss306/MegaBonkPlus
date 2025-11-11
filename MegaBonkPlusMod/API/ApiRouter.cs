using System;
using BepInEx.Logging;
using MegaBonkPlusMod.GameLogic.Common;
using MegaBonkPlusMod.GameLogic.Trackers;
using MegaBonkPlusMod.GameLogic.Minimap;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;

using Assets.Scripts.Actors.Player;
using BonkersLib.Core;
using MegaBonkPlusMod.Actions;
using MegaBonkPlusMod.Models;
using MegaBonkPlusMod.Utils;
using Object = UnityEngine.Object;


namespace MegaBonkPlusMod.API
{
    public class ApiRouter
    {

        private readonly Dictionary<string, BaseTracker> _routeRegistry;
        private readonly MinimapStreamer _minimapStreamer; 
        private readonly ActionHandler _actionHandler;

        public ApiRouter(List<BaseTracker> allTrackers, MinimapStreamer minimapStreamer, ActionHandler actionHandler)
        {
            _routeRegistry = new Dictionary<string, BaseTracker>();
            _minimapStreamer = minimapStreamer; 
            _actionHandler = actionHandler;

            ModLogger.LogDebug("ApiRouter registering Tracker-Routes...");
            foreach (var tracker in allTrackers)
            {
                if (string.IsNullOrEmpty(tracker.ApiRoute)) continue;
                _routeRegistry[tracker.ApiRoute] = tracker;
                ModLogger.LogDebug($"  -> Tracker-Route '{tracker.ApiRoute}' registered.");
            }
            if (_minimapStreamer != null)
            {
                ModLogger.LogDebug("  -> MinimapStreamer for API registered");
            }
        }
        
        public bool HandleApiGetRequest(HttpListenerContext context)
        {
            string path = context.Request.Url.AbsolutePath;
            
            if (path.Equals("/api/items/all", StringComparison.OrdinalIgnoreCase))
            {
                var rawItems = BonkersAPI.Item.GetAllRawItems();
                var frontendItems = rawItems.Select(itemData =>
                {
                    string itemId = itemData.eItem.ToString().ToLowerInvariant();
                    return new ItemViewModel
                    {
                        id = itemId,
                        name = itemData.name, 
                        description = itemData.description,
                        inItemPool = itemData.inItemPool,
                        rarity = itemData.rarity.ToString()
                    };
                }).ToList();
                
                ModLogger.LogDebug($"[ApiRouter] Sending {frontendItems.Count} items to client");
                
                JsonResponse.Send(context, JsonSerializer.Serialize(frontendItems));
                return true;
            }

            if (path.Equals("/api/stream/minimap", StringComparison.OrdinalIgnoreCase) && _minimapStreamer != null)
            {
                string jsonData = _minimapStreamer.GetJsonData();
                JsonResponse.Send(context, jsonData);
                return true;
            }
            
            if (path.Equals("/api/actions/state", StringComparison.OrdinalIgnoreCase))
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
                ModLogger.LogDebug($"Error in POST request: {ex.Message}");
                JsonResponse.Send(context, "{\"status\":\"error\"}", 500, "application/json");
            }
        }
        
        private void ExecuteAction(string actionName, JsonElement payload)
        {
            ModLogger.LogDebug($"Routing action '{actionName}' to the ActionHandler...");
            _actionHandler.HandleAction(actionName, payload);
        }
    }
}