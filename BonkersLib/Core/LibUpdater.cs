using System;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BonkersLib.Core;

public class LibUpdater : MonoBehaviour
{
    public void Initialize(ManualLogSource log)
    {
        BonkersAPI.Initialize(log);
        SceneManager.activeSceneChanged += new Action<Scene, Scene>(BonkersAPI.Internal_OnSceneChanged);
    }
    
    private void Update()
    {
        BonkersAPI.Update();
    }
    
}