using RTSEngine.Core.Map.Runtime;
namespace RTSEngine.Core.Entities.Units;

public abstract class Unit : Entity
{
    public override bool IsBlocking => true; 
    public int OwnerId { get; init; }
    public UnitDefinition Definition { get;}
    public float MovementSpeed { get; protected set; }
    public float MovementProgress { get; set; }
    public GridPosition? TargetPosition { get; set; }
    public Queue<GridPosition> PathQueue { get; set; } = [];

    public GridPosition? FinalDestination { get; set; }

    public int BlockedTicks { get; set; }

    public Unit(
        int ownerId,
        GridPosition position,
        UnitDefinition definition
        )
        {
            Definition = definition;
            OwnerId = ownerId;
            MovementSpeed = definition.MovementSpeed;
            Position = position;
        }
}

