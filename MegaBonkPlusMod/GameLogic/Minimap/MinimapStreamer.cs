using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using BepInEx.Logging;
using MegaBonkPlusMod.Models;
using UnityEngine;
using UnityEngine.UI;
using UnityObject = UnityEngine.Object;

namespace MegaBonkPlusMod.GameLogic.Minimap;

public class MinimapStreamer
{
    private const int TARGET_WIDTH = 256;
    private const int TARGET_HEIGHT = 256;

    private const string MINIMAP_PATH = "GameUI/Map/FullMapUi/MapRender";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
    };

    private volatile string _lastJsonData = "{\"count\":0,\"items\":[]}";
    private readonly ManualLogSource _logger;
    private RawImage _minimapImageComponent;

    public MinimapStreamer(ManualLogSource logger)
    {
        _logger = logger;
        _logger.LogInfo($"MinimapStreamer (Ereignis-gesteuert, SKALIERT auf {TARGET_WIDTH}px) initialisiert.");
    }

    public string GetJsonData()
    {
        return _lastJsonData;
    }

    public void TriggerMinimapUpdate()
    {
        try
        {
            var payload = BuildDataPayload();
            _lastJsonData = JsonSerializer.Serialize(payload, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError($"[MinimapStreamer] SCHWERER FEHLER in BuildDataPayload: {ex.Message}\n{ex.StackTrace}");
            _lastJsonData = "{\"count\":0,\"items\":[]}";
            OnError(ex);
        }
    }

    private object BuildDataPayload()
    {
        var trackedObjects = new List<TrackedObjectData>();
        Texture2D readableTex = null;

        try
        {
            if (_minimapImageComponent == null)
            {
                var pathToMinimap = MINIMAP_PATH;
                var go = GameObject.Find(pathToMinimap);

                if (go != null)
                {
                    _minimapImageComponent = go.GetComponent<RawImage>();
                }
                else
                {
                    _logger.LogWarning($"[MinimapStreamer] Minimap-GameObject auf '{pathToMinimap}' NICHT gefunden.");
                    return new ApiListResponse<TrackedObjectData>();
                }
            }

            if (_minimapImageComponent.mainTexture == null)
                return new ApiListResponse<TrackedObjectData>();

            readableTex = GetReadableAndScaledTexture(_minimapImageComponent.mainTexture);
            if (readableTex == null)
                return new ApiListResponse<TrackedObjectData>();

            var rawData = GetRawDataFromPixels32(readableTex);
            if (rawData == null || rawData.Length == 0)
                return new ApiListResponse<TrackedObjectData>();

            var base64PixelData = Convert.ToBase64String(rawData);

            var imageData = new TrackedObjectData
            {
                Position = PositionData.FromVector3(_minimapImageComponent.transform.position)
            };

            imageData.CustomProperties["width"] = readableTex.width; // 256
            imageData.CustomProperties["height"] = readableTex.height; // 256
            imageData.CustomProperties["format"] = "RGBA32_Raw";
            imageData.CustomProperties["rawPixelData"] = base64PixelData;

            trackedObjects.Add(imageData);

            return new ApiListResponse<TrackedObjectData>(trackedObjects);
        }
        finally
        {
            if (readableTex != null) UnityObject.Destroy(readableTex);
        }
    }

    public void ClearData()
    {
        _logger.LogInfo("[MinimapStreamer] Alte Daten werden gelöscht (ClearData).");
        _lastJsonData = "{\"count\":0,\"items\":[]}";
    }

    private void OnError(Exception ex)
    {
        _minimapImageComponent = null;
    }

    private Texture2D GetReadableAndScaledTexture(Texture mainTexture)
    {
        RenderTexture rt = null;
        Texture2D readableTexture = null;
        try
        {
            rt = RenderTexture.GetTemporary(TARGET_WIDTH, TARGET_HEIGHT);

            Graphics.Blit(mainTexture, rt);

            var previous = RenderTexture.active;
            RenderTexture.active = rt;

            readableTexture = new Texture2D(TARGET_WIDTH, TARGET_HEIGHT);
            readableTexture.ReadPixels(new Rect(0, 0, TARGET_WIDTH, TARGET_HEIGHT), 0, 0);
            readableTexture.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);

            return readableTexture;
        }
        catch (Exception ex)
        {
            _logger.LogError($"[GetReadableTexture] GPU-Read/Skalierungs-Fehler: {ex.Message}");
            if (rt != null) RenderTexture.ReleaseTemporary(rt);
            if (readableTexture != null) UnityObject.Destroy(readableTexture);
            return null;
        }
    }

    private byte[] GetRawDataFromPixels32(Texture2D texture)
    {
        try
        {
            Color32[] pixels = texture.GetPixels32();
            if (pixels == null || pixels.Length == 0) return null;

            var rawData = new byte[pixels.Length * 4];
            var byteIndex = 0;

            foreach (var p in pixels)
            {
                rawData[byteIndex++] = p.r;
                rawData[byteIndex++] = p.g;
                rawData[byteIndex++] = p.b;
                rawData[byteIndex++] = p.a;
            }

            return rawData;
        }
        catch (Exception ex)
        {
            _logger.LogError($"[GetRawDataFromPixels32] Fehler: {ex.Message}");
            return null;
        }
    }
}