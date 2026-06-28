using RTSEngine.Core.Map.Runtime;

namespace RTSEngine.Core.Entities.Component;
public class MovementComponent
{
    public float Speed { get; set; }

    public float Progress { get; set; }

    public GridPosition? TargetPosition { get; set; }

    public GridPosition? FinalDestination { get; set; }

    public Queue<GridPosition> PathQueue { get; set; } = [];

    public int BlockedTicks { get; set; }
}