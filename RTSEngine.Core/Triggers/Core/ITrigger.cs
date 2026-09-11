namespace RTSEngine.Core.Triggers;

public interface ITrigger
{
    string Id { get; }
    bool Enabled { get; set; }
    bool Looping { get; }
    List<ITriggerCondition> Conditions { get; }
    List<ITriggerEffect> Effects { get; }
    bool HasExecuted { get; set; }
}
