namespace RTSEngine.Core.Settings;

public sealed class VictoryCondition
{
    public VictoryConditionType Type { get; set; } = VictoryConditionType.Conquest;
    public int ScoreTarget { get; set; } = 5000;
}
