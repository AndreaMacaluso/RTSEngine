using RTSEngine.Core.Map.Runtime;

namespace RTSEngine.Core.Commands;

public class MoveCommand : ICommand
{
    public List<int> UnitIds { get; set; } = [];

    public GridPosition Target { get; set; }
}