using RTSEngine.Core.Helpers;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Entities.Definitions;

namespace RTSEngine.Core.Entities.States;
public class MovementState
{
    public FixedPoint Speed { get; set; }

    public FixedPoint Progress { get; set; }

    public GridPosition? Destination { get; set; }

    public GridPosition? CurrentStep { get; set; }

    public Queue<GridPosition> PathQueue { get; } = [];

    public int BlockedTicks { get; set; } = 0;

    public bool NeedsRepath { get; set; }

    public MovementState(UnitDefinition definition)
    {
        Speed = definition.MovementSpeed;
    }
}