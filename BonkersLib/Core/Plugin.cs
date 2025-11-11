using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using BonkersLib.Utils;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

namespace BonkersLib.Core;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    public override void Load()
    {
        var configPath = Path.Combine(Paths.ConfigPath, $"{PluginInfo.PLUGIN_GUID}.cfg");
        var sharedConfig = new ConfigFile(configPath, true);
        ModConfig.Initialize(sharedConfig);

        ModLogger.InitLog(Log);

        ClassInjector.RegisterTypeInIl2Cpp<LibUpdater>();

        var updaterGameObject = new GameObject("BonkersLibUpdater");
        var updaterComponent = updaterGameObject.AddComponent<LibUpdater>();
        updaterComponent.Initialize();
        Object.DontDestroyOnLoad(updaterComponent);

        ModLogger.LogInfo($"{PluginInfo.PLUGIN_NAME} Initialized");
        ModLogger.LogInfo($"Debug-Logging: {(ModConfig.IsDebugLoggingEnabled.Value ? "ENABLED" : "DISABLED")}");
    }
}