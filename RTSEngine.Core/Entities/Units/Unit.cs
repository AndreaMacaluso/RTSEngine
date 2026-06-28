using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Entities.Component;
namespace RTSEngine.Core.Entities.Units;

public abstract class Unit : Entity
{
    public int OwnerId { get; init; }
    public UnitDefinition Definition { get;}
    public MovementComponent Movement { get; }
    public int GatherCapacity { get; init; }
    public int BlockedTicks { get; set; }
    public override bool IsBlocking => true; 

    public Unit(
        int ownerId,
        GridPosition position,
        UnitDefinition definition
        )
        {
            Definition = definition;
            OwnerId = ownerId;
            Position = position;
            Movement = new MovementComponent
            {
                Speed = definition.MovementSpeed
            };
        }
}

