using RTSEngine.Core.Map.Runtime;

namespace RTSEngine.Core.Commands;

public class MoveCommand : ICommand
{
    public required List<int> UnitIds { get; init; }

    public required GridPosition Target { get; init; }
}