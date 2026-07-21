using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Entities.Definitions;

namespace RTSEngine.Core.Entities.States;
public class MovementState
{
    public float Speed { get; set; }

    public float Progress { get; set; }

    public GridPosition? Destination { get; set; }

    public GridPosition? CurrentStep { get; set; }

    public Queue<GridPosition> PathQueue { get; set; } = [];

    public int BlockedTicks { get; set; }

    public bool NeedsRepath { get; set; }

    public MovementState(UnitDefinition definition)
    {
        Speed = definition.MovementSpeed;
    }
}