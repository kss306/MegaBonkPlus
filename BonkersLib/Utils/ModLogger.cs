using BepInEx.Logging;
using BonkersLib.Core;

namespace BonkersLib.Utils;

internal static class ModLogger
{
    private static ManualLogSource _modLogger;

    public static void InitLog(ManualLogSource logger)
    {
        _modLogger = logger;
    }

    public static void LogDebug(string message)
    {
        if (ModConfig.IsDebugLoggingEnabled.Value)
            LogInfo(message);
    }

    public static void LogInfo(string message)
    {
        _modLogger?.LogInfo(message);
    }

    public static void LogWarning(string message)
    {
        _modLogger?.LogWarning(message);
    }

    public static void LogError(string message)
    {
        _modLogger?.LogError(message);
    }
}