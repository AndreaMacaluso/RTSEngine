namespace RTSEngine.Core.Settings;

public sealed class GameSettings
{
    public int PopulationCap { get; set; } = 200;
    public int DecayTicks { get; set; } = 300;
    public GameSpeed Speed { get; set; } = GameSpeed.Normal;
    public VisibilityMode Visibility { get; set; } = VisibilityMode.FullMap;
    public VictoryCondition Victory { get; set; } = new();
    public bool DebugMode { get; set; } = false;
}

public enum GameSpeed
{
    Normal = 1,
    Fast = 2,
    VeryFast = 4
}

public enum VisibilityMode
{
    FullMap
}
