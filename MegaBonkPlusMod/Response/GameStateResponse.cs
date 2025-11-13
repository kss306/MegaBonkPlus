namespace MegaBonkPlusMod.Response;

public class GameStateResponse
{
    public bool IsInGame { get; set; }
    public string CurrentMap { get; set; }
    public int MapTier { get; set; }
    public float StageTime { get; set; }
    public float TimeAlive { get; set; }
    public int BossCurses { get; set; }
}