using BonkersLib.Services;
using BonkersLib.Services.Player;
using BonkersLib.Services.World;
using UnityEngine.SceneManagement;

namespace BonkersLib.Core;

public static class BonkersAPI
{
    public static GameStateService Game { get; private set; }
    public static WorldService World { get; private set; }
    public static PlayerService Player { get; private set; }
    public static ItemService Item { get; private set; }
    public static WeaponService Weapon { get; private set; }
    public static UiService Ui { get; private set; }

    internal static void Initialize()
    {
        Game = new GameStateService();
        World = new WorldService();
        Player = new PlayerService();
        Item = new ItemService();
        Weapon = new WeaponService();
        
        Game.GameStarted += Ui.OnGameStarted;
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