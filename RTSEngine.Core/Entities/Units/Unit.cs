using RTSEngine.Core.Map.Runtime;
namespace RTSEngine.Core.Entities.Units;

public abstract class Unit : Entity
{
    public override bool IsBlocking => true; 
    public int OwnerId { get; init; }

    public float MovementSpeed { get; protected set; }

    public float MovementProgress { get; set; }

    public GridPosition? TargetPosition { get; set; }

    public Unit(
        int ownerId,
        GridPosition position,
        float movementSpeed
        )
        {
            OwnerId = ownerId;
            MovementSpeed = movementSpeed;
            Position = position;
        }
}

