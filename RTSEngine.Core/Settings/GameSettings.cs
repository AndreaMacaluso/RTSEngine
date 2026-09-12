namespace RTSEngine.Core.Settings;

public sealed class GameSettings
{
    public string MapPath { get; set; } = "";
    public int Width { get; set; } = 40;
    public int Height { get; set; } = 40;
    public int PopulationCap { get; set; } = 200;
    public GameSpeed Speed { get; set; } = GameSpeed.Normal;
    public VisibilityMode Visibility { get; set; } = VisibilityMode.FullMap;
    public VictoryCondition Victory { get; set; } = new();
}
