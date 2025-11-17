using System;
using System.Collections.Generic;
using System.Text.Json;
using UnityEngine;

namespace MegaBonkPlusMod.GameLogic.Trackers.Base;

public abstract class BaseTracker : BasePollingProvider
{
    private readonly List<GameObject> _cachedMinimapIcons = new();

    public BaseTracker(float scanIntervalInSeconds = 2.0f)
        : base(scanIntervalInSeconds)
    {
    }

    public int LastKnownCacheCount { get; private set; }

    protected void CacheIconsForObject(GameObject minimapIcon)
    {
        _cachedMinimapIcons.Add(minimapIcon);
    }

    public override void ForceUpdatePayload()
    {
        _cachedMinimapIcons.Clear();

        base.ForceUpdatePayload();

        LastKnownCacheCount = _cachedMinimapIcons.Count;
    }

    public override void Update()
    {
        if (CheckTimer()) ForceUpdatePayload();
    }

    public void HideIcons()
    {
        foreach (var icon in _cachedMinimapIcons)
            if (icon && icon.activeSelf)
                icon.SetActive(false);
    }

    public void ShowIcons()
    {
        foreach (var icon in _cachedMinimapIcons)
            if (icon)
                icon.SetActive(true);
    }

    public object GetData()
    {
        var jsonString = GetJsonData();
        if (!string.IsNullOrEmpty(jsonString))
            try
            {
                return JsonSerializer.Deserialize<object>(jsonString);
            }
            catch
            {
                return BuildDataPayload();
            }

        return BuildDataPayload();
    }

    protected abstract override object BuildDataPayload();

    protected override void OnError(Exception ex)
    {
        OnTrackerError(ex);
    }

    protected virtual void OnTrackerError(Exception ex)
    {
    }
}