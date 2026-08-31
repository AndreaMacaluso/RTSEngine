namespace RTSEngine.Core.Settings;

public enum VictoryConditionType
{
    Conquest,
    ScoreLimit
}

public sealed class VictoryCondition
{
    public VictoryConditionType Type { get; set; } = VictoryConditionType.Conquest;
    public int ScoreTarget { get; set; } = 5000;
}
