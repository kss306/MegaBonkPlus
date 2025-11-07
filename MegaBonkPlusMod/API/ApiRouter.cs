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
using UnityEngine;

using Assets.Scripts.Actors.Player;
using Assets.Scripts.Inventory__Items__Pickups.Chests;
using Object = UnityEngine.Object;


namespace MegaBonkPlusMod.API
{
    public class ApiRouter
    {
        private readonly ManualLogSource _logger;
        private readonly Dictionary<string, BaseTracker> _routeRegistry;
        private readonly MinimapStreamer _minimapStreamer; 

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
        
        public bool HandleApiGetRequest(HttpListenerContext context)
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
        
        public void HandleApiPostRequest(HttpListenerContext context)
        {
            string jsonPayload;
            try
            {
                using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
                {
                    jsonPayload = reader.ReadToEnd();
                }
                
                var doc = JsonDocument.Parse(jsonPayload);
                var root = doc.RootElement;
                
                string action = root.GetProperty("action").GetString();
                int instanceId = root.GetProperty("instanceId").GetInt32();

                MainThreadActionQueue.QueueAction(() =>
                {
                    ExecuteAction(action, instanceId);
                });

                JsonResponse.Send(context, "{\"status\":\"ok\"}", 200, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Fehler bei POST-Anfrage: {ex.Message}");
                JsonResponse.Send(context, "{\"status\":\"error\"}", 500, "application/json");
            }
        }
        
        private void ExecuteAction(string action, int instanceId)
        {
            var allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            GameObject target = allGameObjects.FirstOrDefault(g => g.GetInstanceID() == instanceId);

            if (!target) return;
            
            var player = Object.FindObjectOfType<MyPlayer>();
            if (!player) return;
            
            switch (action)
            {
                case "teleport":
                    TeleportService.StartTeleport(player.gameObject, target);
                    break;
                
                case "interact":
                    
                    var chest = target.GetComponent<InteractableChest>();
                    if (chest) { chest.Interact(); return; }

                    var maoiShrine = target.GetComponent<InteractableShrineMoai>();
                    if (maoiShrine) { maoiShrine.Interact(); return; }

                    var bossSpawner = target.GetComponent<InteractableBossSpawner>();
                    if (bossSpawner) { bossSpawner.Interact(); return; }
                    
                    var challengeShrine = target.GetComponent<InteractableShrineChallenge>();
                    if (challengeShrine) { challengeShrine.Interact(); return; }
                    
                    var cursedShrine = target.GetComponent<InteractableShrineCursed>();
                    if (cursedShrine) { cursedShrine.Interact(); return; }
                    
                    var greedShrine = target.GetComponent<InteractableShrineGreed>();
                    if (greedShrine) { greedShrine.Interact(); return; }
                    
                    var magnetShrine = target.GetComponent<InteractableShrineMagnet>();
                    if (magnetShrine) { magnetShrine.Interact(); return; }
                    
                    var microwave = target.GetComponent<InteractableMicrowave>();
                    if (microwave) { microwave.Interact(); return; }
                    
                    var shadyGuy = target.GetComponent<InteractableShadyGuy>();
                    if (shadyGuy) { shadyGuy.Interact(); return; }
                    
                    break;
            }
        }
    }
}