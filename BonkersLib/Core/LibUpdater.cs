using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BonkersLib.Core;

public class LibUpdater : MonoBehaviour
{
    private void Update()
    {
        BonkersAPI.Update();
    }

    public void Initialize()
    {
        BonkersAPI.Initialize();
        SceneManager.activeSceneChanged += new Action<Scene, Scene>(BonkersAPI.Internal_OnSceneChanged);
    }
}