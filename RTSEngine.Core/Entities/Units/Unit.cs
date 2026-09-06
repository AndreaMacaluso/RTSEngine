using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Entities.States;
using RTSEngine.Core.Entities.Definitions;

namespace RTSEngine.Core.Entities.Units;

public sealed class Unit : Entity, IHittable
{
    public int OwnerId { get; init; }
    public UnitDefinition Definition { get;}
    public HealthComponent Health { get; }
    public CombatState Combat { get; }
    public bool IsDead => Health.IsDead;
    public void TakeDamage(int amount) => Health.TakeDamage(amount);
    public MovementState Movement { get; }
    public GatherState Gather { get; }
    public BuildState Build { get; }
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
            Health = new HealthComponent(definition.MaxHealth, definition.MaxHealth);
            Movement = new MovementState(definition);
            Gather = new GatherState(definition);
            Build = new BuildState();
            Combat = new CombatState(definition);
        }
}

