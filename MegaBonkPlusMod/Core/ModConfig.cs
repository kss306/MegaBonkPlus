using BepInEx.Configuration;
using MegaBonkPlusMod.Enums;

namespace MegaBonkPlusMod.Core;

internal static class ModConfig
{
    public static ConfigEntry<LoggingLevelEnum> LogLevel { get; private set; }
    public static ConfigEntry<int> WebServerPort { get; private set; }

    internal static void Initialize(ConfigFile config)
    {
        LogLevel = config.Bind(
            "General",
            "LoggingLevel",
            LoggingLevelEnum.Info,
            "Controls log verbosity: Info = only normal logs, Debug = Info + debug, Http = Debug + HTTP, Trace = everything"
        );

        WebServerPort = config.Bind(
            "General",
            "WebServerPort",
            8080,
            "Port for the WebServer"
        );
    }
}