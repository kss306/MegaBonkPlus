using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using Il2CppInterop.Runtime.Injection;
using MegaBonkPlusMod.Utils;
using UnityEngine;

namespace MegaBonkPlusMod.Core;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.kss.bonkerslib")]
public class Plugin : BasePlugin
{
    public override void Load()
    {
        var configPath = Path.Combine(Paths.ConfigPath, $"{PluginInfo.PLUGIN_GUID}.cfg");
        var sharedConfig = new ConfigFile(configPath, true);

        ModConfig.Initialize(sharedConfig);

        ModLogger.InitLog(Log);

        HotkeyManager.Initialize(sharedConfig);

        ClassInjector.RegisterTypeInIl2Cpp<ModManager>();

        var managerGameObject = new GameObject("MegaBonkPlus");
        var managerComponent = managerGameObject.AddComponent<ModManager>();
        managerComponent.Initialize();
        Object.DontDestroyOnLoad(managerGameObject);

        ModLogger.LogInfo($"{PluginInfo.PLUGIN_NAME} Initialized");
        ModLogger.LogInfo($"Logging-Level: {ModConfig.LogLevel.Value}");
    }
}