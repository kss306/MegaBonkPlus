using BepInEx.Logging;
using MegaBonkPlusMod.Core;
using MegaBonkPlusMod.Enums;

namespace MegaBonkPlusMod.Utils;

internal static class ModLogger
{
    private static ManualLogSource _modLogger;

    public static void InitLog(ManualLogSource logger)
    {
        _modLogger = logger;
    }

    private static bool IsEnabled(LoggingLevelEnum level)
    {
        return ModConfig.LogLevel.Value >= level;
    }

    public static void LogDebug(string message)
    {
        if (IsEnabled(LoggingLevelEnum.Debug))
            _modLogger?.LogInfo($"[DEBUG] {message}");
    }

    public static void LogHttp(string message)
    {
        if (IsEnabled(LoggingLevelEnum.Http))
            _modLogger?.LogInfo($"[HTTP] {message}");
    }

    public static void LogTrace(string message)
    {
        if (IsEnabled(LoggingLevelEnum.Trace))
            _modLogger?.LogInfo($"[TRACE] {message}");
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