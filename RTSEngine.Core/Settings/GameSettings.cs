namespace RTSEngine.Core.Settings;

public sealed class GameSettings
{
    public string MapPath { get; set; } = "";
    public int Width { get; set; } = 40;
    public int Height { get; set; } = 40;
    public int NumPlayers { get; set; } = 2;
    public int PopulationCap { get; set; } = 200;
    public GameSpeed Speed { get; set; } = GameSpeed.Normal;
    public VisibilityMode Visibility { get; set; } = VisibilityMode.FullMap;
    public GameMode Mode { get; set; } = GameMode.RandomMap;
    public VictoryType Victory { get; set; } = VictoryType.Conquest;
    //@ToDo ScoreTarget should be read from trigger definition, not settings
    //public int ScoreTarget { get; set; } = 1000;
}
