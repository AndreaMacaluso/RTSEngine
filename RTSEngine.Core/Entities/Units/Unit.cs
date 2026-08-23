using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Entities.States;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.State;
namespace RTSEngine.Core.Entities.Units;

public sealed class Unit : Entity
{
    public int OwnerId { get; init; }
    public UnitDefinition Definition { get;}
    public MovementState Movement { get; }
    public GatherState Gather { get; }
    public BuildState Build { get; }
    public CombatState Combat { get; }
    public UnitTask CurrentTask { get; set; } = UnitTask.Idle;
    public HealthState Health { get; }
    public override bool IsDead => Health.IsDead;
    public override bool IsBlocking => !IsDead;

    public Unit(
        int ownerId,
        GridPosition position,
        UnitDefinition definition
        )
        {
            Definition = definition;
            OwnerId = ownerId;
            Position = position;
            Health = new HealthState(definition);
            Movement = new MovementState(definition);
            Gather = new GatherState(definition);
            Build = new BuildState();
            Combat = new CombatState(definition);
        }
}

