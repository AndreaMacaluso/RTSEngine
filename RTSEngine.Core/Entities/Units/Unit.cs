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
    public int CurrentHealth { get; set; }
    public override bool IsDead => CurrentHealth <= 0;
    public override bool IsBlocking => !IsDead;

    public override void TakeDamage(int amount)
    {
        CurrentHealth -= amount;

        if (CurrentHealth < 0)
        {
            CurrentHealth = 0;
        }
    }

    public Unit(
        int ownerId,
        GridPosition position,
        UnitDefinition definition
        )
        {
            Definition = definition;
            OwnerId = ownerId;
            Position = position;
            CurrentHealth = definition.MaxHealth;
            Movement = new MovementState(definition);
            Gather = new GatherState(definition);
            Build = new BuildState();
            Combat = new CombatState(definition);
        }
}

