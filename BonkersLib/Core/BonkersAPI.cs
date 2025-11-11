using BonkersLib.Services;
using BepInEx.Logging;
using UnityEngine.SceneManagement;

namespace BonkersLib.Core
{
    public static class BonkersAPI
    {
        public static GameStateService Game { get; private set; }
        public static WorldService World { get; private set; }
        public static PlayerService Player { get; private set; }

        internal static void Initialize(ManualLogSource log)
        {
            Game = new GameStateService(log);
            World = new WorldService(log);
            Player = new PlayerService(log);
        }

        internal static void Update()
        {
            Game.Update();
            World.Update();
            Player.Update();
        }
        
        internal static void Internal_OnSceneChanged(Scene oldScene, Scene newScene)
        {
            Game?.OnSceneChanged(newScene);
        }
    }
}