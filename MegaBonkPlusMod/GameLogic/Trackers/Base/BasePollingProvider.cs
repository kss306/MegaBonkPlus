using BepInEx.Logging;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using BonkersLib.Core;
using MegaBonkPlusMod.Utils;
using UnityEngine;

namespace MegaBonkPlusMod.GameLogic.Trackers.Base;

public abstract class BasePollingProvider
{
    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
    };

    private volatile string _lastJsonData = "{\"count\":0,\"items\":[]}";
    private readonly float _scanInterval;
    private float _nextScanTime = 0f;

    public BasePollingProvider(float scanIntervalInSeconds)
    {
        _scanInterval = scanIntervalInSeconds;
    }

    public bool CheckTimer()
    {
        if (!BonkersAPI.Game.IsInGame && _scanInterval > 0)
        {
            _lastJsonData = "{\"count\":0,\"items\":[]}";
            return false;
        }

        if (Time.time < _nextScanTime)
        {
            return false;
        }

        _nextScanTime = Time.time + _scanInterval;
        return true;
    }

    public virtual void ForceUpdatePayload()
    {
        try
        {
            object payload = BuildDataPayload();
            _lastJsonData = JsonSerializer.Serialize(payload, JsonOptions);
        }
        catch (Exception ex)
        {
            ModLogger.LogDebug($"Error updating '{GetType().Name}': {ex.Message}");
            _lastJsonData = "{\"count\":0,\"items\":[]}";
            OnError(ex);
        }
    }

    public virtual void Update()
    {
        if (CheckTimer())
        {
            ForceUpdatePayload();
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