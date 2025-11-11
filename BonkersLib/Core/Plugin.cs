using BepInEx;
using BepInEx.Unity.IL2CPP;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

namespace BonkersLib.Core;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    public override void Load()
    {
        ClassInjector.RegisterTypeInIl2Cpp<LibUpdater>();
        
        GameObject updaterGameObject = new GameObject("BonkersLibUpdater");
        LibUpdater updaterComponent = updaterGameObject.AddComponent<LibUpdater>();
        updaterComponent.Initialize(Log);
        Object.DontDestroyOnLoad(updaterComponent);
    }
}