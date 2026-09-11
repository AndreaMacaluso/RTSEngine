namespace RTSEngine.Core.Triggers;

public static class ConditionFactory
{
    public static ITriggerCondition Create(ConditionDefinition definition)
    {
        if (definition.ConditionType == 0)
            throw new ArgumentException("ConditionType is required");

        return definition.ConditionType switch
        {
            _ => throw new NotSupportedException($"Condition type {definition.ConditionType} not supported")
        };
    }
}
