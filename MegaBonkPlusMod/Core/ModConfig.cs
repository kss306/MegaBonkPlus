using BepInEx.Configuration;

namespace MegaBonkPlusMod.Core;

internal static class ModConfig
{
    public static ConfigEntry<bool> IsDebugLoggingEnabled { get; private set; }
    public static ConfigEntry<int> WebServerPort { get; private set; }

    internal static void Initialize(ConfigFile config)
    {
        IsDebugLoggingEnabled = config.Bind(
            "1. General",
            "DebugLogging",
            false,
            "Enables detailed Debug-Logs"
        );
        WebServerPort = config.Bind(
            "1. General",
            "WebServerPort",
            8080,
            "Port for the WebServer"
        );
    }
}