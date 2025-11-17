using System;
using System.Collections.Generic;
using MegaBonkPlusMod.GameLogic.Minimap;
using MegaBonkPlusMod.GameLogic.Trackers.Base;
using MegaBonkPlusMod.Utils;
using UnityEngine;

namespace MegaBonkPlusMod.Infrastructure.Services;

public class MinimapCaptureService
{
    private const float MINIMAP_CAPTURE_DELAY = 1.5f;
    private readonly MinimapStreamer _minimapStreamer;

    private readonly IReadOnlyList<BaseTracker> _trackers;

    private CaptureState _captureState = CaptureState.Idle;
    private float _captureTimer;

    public MinimapCaptureService(IReadOnlyList<BaseTracker> trackers, MinimapStreamer minimapStreamer)
    {
        _trackers = trackers;
        _minimapStreamer = minimapStreamer;
    }

    public void StartCapture()
    {
        ModLogger.LogDebug("Starting minimap capture logic...");
        _minimapStreamer.ClearData();
        _captureState = CaptureState.WaitingForSpawn;
        _captureTimer = 0;
    }

    public void Update()
    {
        if (_captureState == CaptureState.Idle)
            return;

        _captureTimer -= Time.unscaledDeltaTime;
        if (_captureTimer > 0f)
            return;

        switch (_captureState)
        {
            case CaptureState.WaitingForSpawn:
                HandleWaitingForSpawn();
                break;

            case CaptureState.WaitingForMapDelay:
                HandleWaitingForMapDelay();
                break;

            case CaptureState.HidingIcons:
                HandleTakingPicture();
                break;
        }
    }

    private void HandleWaitingForSpawn()
    {
        foreach (var tracker in _trackers) tracker.ForceUpdatePayload();

        _captureState = CaptureState.WaitingForMapDelay;
        _captureTimer = MINIMAP_CAPTURE_DELAY;
    }

    private void HandleWaitingForMapDelay()
    {
        _captureState = CaptureState.HidingIcons;
        try
        {
            foreach (var tracker in _trackers) tracker.HideIcons();

            Canvas.ForceUpdateCanvases();
        }
        catch (Exception ex)
        {
            _captureState = CaptureState.Idle;
            ModLogger.LogDebug($"Error hiding icons: {ex.Message}");
        }
    }

    private void HandleTakingPicture()
    {
        try
        {
            _minimapStreamer.TriggerMinimapUpdate();
        }
        catch (Exception ex)
        {
            ModLogger.LogDebug($"Error creating the MinimapImage: {ex.Message}");
        }
        finally
        {
            foreach (var tracker in _trackers) tracker.ShowIcons();
        }

        _captureState = CaptureState.Idle;
    }

    private enum CaptureState
    {
        Idle,
        WaitingForSpawn,
        WaitingForMapDelay,
        HidingIcons,
        TakingPicture
    }
}