using BepInEx;
using BepInEx.Unity.IL2CPP;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

namespace MegaBonkPlusMod
{
    [BepInPlugin("com.deinname.meinspielewebmod", "Mein Spiel Web Mod", "1.0.0")]
    public class Plugin : BasePlugin
    {
        public override void Load()
        {
            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} wird geladen...");
            
            ClassInjector.RegisterTypeInIl2Cpp<WebserverComponent>();

            var webserverGameObject = new GameObject("MeinWebserverManager");
            
            webserverGameObject.AddComponent<WebserverComponent>();

            UnityEngine.Object.DontDestroyOnLoad(webserverGameObject);            
            Log.LogInfo("Webserver-Komponente erfolgreich injiziert und gestartet.");
        }
    }
    
    public static class PluginInfo
    {
        public const string PLUGIN_GUID = "com.deinname.meinspielewebmod";
        public const string PLUGIN_NAME = "Mein Spiel Web Mod";
        public const string PLUGIN_VERSION = "1.0.0";
    }
}