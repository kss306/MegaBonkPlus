using BepInEx.Logging;
using MegaBonkPlusMod.GameLogic.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MegaBonkPlusMod.GameLogic.Trackers
{
    public abstract class BaseTracker : BasePollingProvider
    {
        public abstract string ApiRoute { get; }
        protected readonly List<GameObject> _cachedMinimapIcons = new List<GameObject>();
        
        public int LastKnownCacheCount { get; private set; } = 0;

        public BaseTracker(ManualLogSource logger, float scanIntervalInSeconds = 2.0f) 
            : base(logger, scanIntervalInSeconds)
        {
        }
        
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
            if (CheckTimer())
            {
                ForceUpdatePayload();
            }
        }

        public void HideIcons()
        {
            foreach (var icon in _cachedMinimapIcons)
            {
                if (icon != null && icon.activeSelf)
                    icon.SetActive(false);
            }
        }
        public void ShowIcons()
        {
            foreach (var icon in _cachedMinimapIcons)
            {
                if (icon != null)
                    icon.SetActive(true);
            }
        }
        
        protected abstract override object BuildDataPayload();
        protected override void OnError(Exception ex) => OnTrackerError(ex);
        protected virtual void OnTrackerError(Exception ex) { }
    }
}