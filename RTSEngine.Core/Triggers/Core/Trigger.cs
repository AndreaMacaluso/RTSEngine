namespace RTSEngine.Core.Triggers;

public sealed class Trigger : ITrigger
{
    public string Id { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public bool Looping { get; set; } = false;
    public List<ITriggerCondition> Conditions { get; set; } = new();
    public List<ITriggerEffect> Effects { get; set; } = new();
    public bool HasExecuted { get; set; } = false;
}
