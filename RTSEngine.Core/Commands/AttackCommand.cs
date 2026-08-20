namespace RTSEngine.Core.Commands;

public class AttackCommand : ICommand
{
    public required List<int> UnitIds { get; init; }

    public required int TargetEntityId { get; init; }
}
