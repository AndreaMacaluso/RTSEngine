using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Entities.States;

namespace RTSEngine.Core.Entities.Buildings;

public sealed class Building : Entity, IHittable
{
    public int OwnerId { get; init; }

    public BuildingDefinition Definition { get; }
    public HealthComponent Health { get; }
    public CombatState Combat { get; }
    public bool IsDead => IsCompleted && Health.IsDead;
    public void TakeDamage(int amount) => Health.TakeDamage(amount);
    public int ConstructionProgress { get; set; }
    public bool IsCompleted { get; set; }
    public ProductionState Production { get; } = new();
    public override bool IsBlocking => IsCompleted && !IsDead;

    public Building(
        int ownerId,
        GridPosition position,
        BuildingDefinition definition)
    {
        OwnerId = ownerId;
        Position = position;
        Definition = definition;
        Health = new HealthComponent(definition.MaxHealth, 0);
        Combat = new CombatState();
        ConstructionProgress = 0;
        IsCompleted = false;

        if (definition.CanAttack)
        {
            Combat = new CombatState(definition);
        }
    }
}