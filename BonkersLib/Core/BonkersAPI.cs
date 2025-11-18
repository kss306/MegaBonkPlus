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
    public static TomeService Tome { get; private set; }
    public static UiService Ui { get; private set; }
    public static DataService Data { get; private set; }
    public static AchievementService Achievements { get; private set; }

    internal static void Initialize()
    {
        Game = new GameStateService();
        Data = new DataService();
        World = new WorldService();
        Player = new PlayerService();
        Item = new ItemService();
        Weapon = new WeaponService();
        Tome = new TomeService();
        Ui = new UiService();
        Achievements = new AchievementService();

        Game.SceneChanged += Data.SetDataManager;
        Game.GameStarted += Ui.OnGameStarted;
        Game.SceneChanged += Item.CacheAllRawItems;
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