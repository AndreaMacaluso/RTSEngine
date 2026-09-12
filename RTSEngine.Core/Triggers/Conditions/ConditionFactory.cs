namespace RTSEngine.Core.Triggers;

public static class ConditionFactory
{
    public static ITriggerCondition Create(ConditionDefinition definition)
    {
        if (definition.ConditionType == 0)
            throw new ArgumentException("ConditionType is required");

        return definition.ConditionType switch
        {
            (int)ConditionType.OwnObjects => new OwnObjectsCondition(
                definition.PlayerId ?? throw new ArgumentException("PlayerId is required for OwnObjects"),
                definition.ObjectTypeId ?? throw new ArgumentException("ObjectTypeId is required for OwnObjects"),
                definition.Quantity ?? throw new ArgumentException("Quantity is required for OwnObjects")),
            (int)ConditionType.Timer => new TimerCondition(
                definition.Time ?? throw new ArgumentException("Time is required for Timer"),
                definition.StartTick ?? 0),
            _ => throw new NotSupportedException($"Condition type {definition.ConditionType} not supported")
        };
    }
}
