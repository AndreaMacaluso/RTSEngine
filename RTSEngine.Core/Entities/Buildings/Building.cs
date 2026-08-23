using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Entities.States;
namespace RTSEngine.Core.Entities.Buildings;

public sealed class Building : Entity
{
    public int OwnerId { get; init; }

    public BuildingDefinition Definition { get; }

    public HealthState Health { get; }
    public int ConstructionProgress { get; set; }
    public bool IsCompleted { get; set; }
    public int PopulationBonus { get; set; }
    public ProductionState Production { get; } = new();
    public override bool IsBlocking => !IsDead;
    public override bool IsDead => Health.IsDead;

    public Building(
        int ownerId,
        GridPosition position,
        BuildingDefinition definition)
    {
        OwnerId = ownerId;
        Position = position;
        Definition = definition;
        Health = new HealthState(definition);
        ConstructionProgress = 0;
        IsCompleted = false;
    }
}