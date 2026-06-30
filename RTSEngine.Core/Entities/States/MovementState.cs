using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Entities.Definitions;

namespace RTSEngine.Core.Entities.States;
public class MovementState
{
    public float Speed { get; set; }

    public float Progress { get; set; }

    public GridPosition? TargetPosition { get; set; }

   // public GridPosition? FinalDestination { get; set; }

    public Queue<GridPosition> PathQueue { get; set; } = [];

    public int BlockedTicks { get; set; }

    public MovementState(UnitDefinition definition)
    {
        Speed = definition.MovementSpeed;
    }
}