namespace RTSEngine.Core.Triggers;

public sealed class TriggerDefinition
{
    public string TriggerName { get; set; } = string.Empty;
    public int Enabled { get; set; } = 1;
    public int Looping { get; set; } = 0;
    public List<ConditionDefinition> Conditions { get; set; } = new();
    public List<EffectDefinition> Effects { get; set; } = new();
}
