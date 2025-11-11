using BepInEx.Configuration;

namespace BonkersLib.Core;

internal static class ModConfig
{
    public static ConfigEntry<bool> IsDebugLoggingEnabled { get; private set; }

    internal static void Initialize(ConfigFile config)
    {
        IsDebugLoggingEnabled = config.Bind(
            "1. General",
            "DebugLogging",
            false,
            "Enables detailed Debug-Logs"
        );
    }
}