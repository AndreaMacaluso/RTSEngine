using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Entities.States;
using RTSEngine.Core.State;
namespace RTSEngine.Core.Entities.Units;

public class Unit : Entity
{
    public int OwnerId { get; init; }
    public UnitDefinition Definition { get;}
    public MovementState Movement { get; }
    public GatherState Gather { get; }
    public UnitTask CurrentTask { get; set; } = UnitTask.Idle;
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
            Movement = new MovementState(definition);
            Gather = new GatherState(definition);
        }
}

