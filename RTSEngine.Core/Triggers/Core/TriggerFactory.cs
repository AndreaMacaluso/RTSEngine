namespace RTSEngine.Core.Triggers;

public static class TriggerFactory
{
    public static ITrigger Create(TriggerDefinition definition)
    {
        if (string.IsNullOrWhiteSpace(definition.TriggerName))
            throw new ArgumentException("TriggerName is required");

        var conditions = definition.Conditions
            .Select(c => ConditionFactory.Create(c))
            .ToList();

        var effects = definition.Effects
            .Select(e => EffectFactory.Create(e))
            .ToList();

        return new Trigger
        {
            Id = definition.TriggerName,
            Enabled = definition.Enabled == 1,
            Looping = definition.Looping == 1,
            Conditions = conditions,
            Effects = effects
        };
    }
}
