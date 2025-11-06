using BepInEx;
using BepInEx.Unity.IL2CPP;
using Il2CppInterop.Runtime.Injection;
using MegaBonkPlusMod.Core;
using UnityEngine;

namespace MegaBonkPlusMod;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    public override void Load()
    {
        Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} wird geladen...");

        // 1. Registriere unsere Haupt-MonoBehaviour-Klasse
        ClassInjector.RegisterTypeInIl2Cpp<ModManager>();

        // 2. Erstelle das Manager-GameObject
        var managerGameObject = new GameObject("MeinWebMod_Manager");

        // 3. Füge die Komponente hinzu und übergebe den Logger
        var managerComponent = managerGameObject.AddComponent<ModManager>();
        managerComponent.Initialize(Log); // Übergibt den BepInEx-Logger

        // 4. Mache es unzerstörbar
        Object.DontDestroyOnLoad(managerGameObject);

        Log.LogInfo("ModManager-Komponente erfolgreich injiziert.");
    }
}