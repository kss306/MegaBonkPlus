using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using BepInEx.Logging;
using MegaBonkPlusMod.Core;
using UnityEngine;

namespace MegaBonkPlusMod.GameLogic.Common;

public abstract class BasePollingProvider
{
    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
    };

    private readonly float _scanInterval;

    private volatile string _lastJsonData = "{\"count\":0,\"items\":[]}";
    private float _nextScanTime;
    protected ManualLogSource Logger;

    public BasePollingProvider(ManualLogSource logger, float scanIntervalInSeconds)
    {
        Logger = logger;
        _scanInterval = scanIntervalInSeconds;
    }

    public void Update()
    {
        if (!ModManager.IsInGame && _scanInterval > 0)
        {
            _lastJsonData = "{\"count\":0,\"items\":[]}";
            return;
        }

        if (Time.time < _nextScanTime) return;
        _nextScanTime = Time.time + _scanInterval;

        try
        {
            var payload = BuildDataPayload();

            _lastJsonData = JsonSerializer.Serialize(payload, JsonOptions);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Fehler im Update von '{GetType().Name}': {ex.Message}");
            _lastJsonData = "{\"count\":0,\"items\":[]}"; // Setze bei Fehler zurück
            OnError(ex);
        }
    }

    public string GetJsonData()
    {
        return _lastJsonData;
    }

    protected abstract object BuildDataPayload();

    protected virtual void OnError(Exception ex)
    {
    }
}