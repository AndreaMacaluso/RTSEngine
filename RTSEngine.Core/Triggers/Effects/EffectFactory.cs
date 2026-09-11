namespace RTSEngine.Core.Triggers;

public static class EffectFactory
{
    public static ITriggerEffect Create(EffectDefinition definition)
    {
        if (definition.EffectType == 0)
            throw new ArgumentException("EffectType is required");

        return definition.EffectType switch
        {
            _ => throw new NotSupportedException($"Effect type {definition.EffectType} not supported")
        };
    }
}
