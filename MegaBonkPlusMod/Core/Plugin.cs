using BepInEx;
using BepInEx.Unity.IL2CPP;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

namespace MegaBonkPlusMod.Core
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("com.kss.bonkerslib")]
    public class Plugin : BasePlugin
    {
        public override void Load()
        {
            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} wird geladen...");

            ClassInjector.RegisterTypeInIl2Cpp<ModManager>();

            var managerGameObject = new GameObject("MegaBonkPlus");

            var managerComponent = managerGameObject.AddComponent<ModManager>();
            managerComponent.Initialize(Log);

            Object.DontDestroyOnLoad(managerGameObject);
        }
    }
}