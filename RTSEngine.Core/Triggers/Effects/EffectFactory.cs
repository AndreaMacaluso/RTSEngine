namespace RTSEngine.Core.Triggers;

public static class EffectFactory
{
    public static ITriggerEffect Create(EffectDefinition definition)
    {
        if (definition.EffectType == 0)
            throw new ArgumentException("EffectType is required");

        return definition.EffectType switch
        {
            (int)EffectType.CreateObject => new CreateObjectEffect(
                definition.PlayerId ?? throw new ArgumentException("PlayerId is required for CreateObject"),
                definition.ObjectTypeId ?? throw new ArgumentException("ObjectTypeId is required for CreateObject"),
                definition.LocationX ?? throw new ArgumentException("LocationX is required for CreateObject"),
                definition.LocationY ?? throw new ArgumentException("LocationY is required for CreateObject")),
            (int)EffectType.SendChat => new SendChatEffect(
                definition.PlayerId ?? throw new ArgumentException("PlayerId is required for SendChat"),
                definition.Message ?? throw new ArgumentException("Message is required for SendChat")),
            (int)EffectType.TaskMovementObject => new TaskMovementObjectEffect(
                definition.PlayerId ?? throw new ArgumentException("PlayerId is required for TaskMovementObject"),
                definition.LocationX ?? throw new ArgumentException("LocationX is required for TaskMovementObject"),
                definition.LocationY ?? throw new ArgumentException("LocationY is required for TaskMovementObject")),
            (int)EffectType.AttackMove => new AttackMoveEffect(
                definition.PlayerId ?? throw new ArgumentException("PlayerId is required for AttackMove"),
                definition.LocationX ?? throw new ArgumentException("LocationX is required for AttackMove"),
                definition.LocationY ?? throw new ArgumentException("LocationY is required for AttackMove")),
            _ => throw new NotSupportedException($"Effect type {definition.EffectType} not supported")
        };
    }
}
