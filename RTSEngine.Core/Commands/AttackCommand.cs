using RTSEngine.Core.Map.Runtime;

namespace RTSEngine.Core.Commands;

public class AttackCommand : ICommand
{
    public required List<int> UnitIds { get; init; }

    public required int PlayerId { get; init; }

    public required AttackMode Mode { get; init; }

    public int? TargetEntityId { get; init; }

    public GridPosition? TargetPosition { get; init; }

    public FormationType Formation { get; init; } = FormationType.None;
}
