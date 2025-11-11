using BonkersLib.Services;
using UnityEngine.SceneManagement;

namespace BonkersLib.Core;

public static class BonkersAPI
{
    public static GameStateService Game { get; private set; }
    public static WorldService World { get; private set; }
    public static PlayerService Player { get; private set; }
    public static ItemService Item { get; private set; }

    internal static void Initialize()
    {
        Game = new GameStateService();
        World = new WorldService();
        Player = new PlayerService();
        Item = new ItemService();
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