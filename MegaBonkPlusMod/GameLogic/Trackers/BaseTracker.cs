using System;
using BepInEx.Logging;
using MegaBonkPlusMod.GameLogic.Common;

namespace MegaBonkPlusMod.GameLogic.Trackers;

public abstract class BaseTracker : BasePollingProvider
{
    public BaseTracker(ManualLogSource logger, float scanIntervalInSeconds = 2.0f) : base(logger, scanIntervalInSeconds)
    {
    }

    public abstract string ApiRoute { get; }

    protected override void OnError(Exception ex)
    {
        OnTrackerError(ex);
    }

    protected virtual void OnTrackerError(Exception ex)
    {
    }
}