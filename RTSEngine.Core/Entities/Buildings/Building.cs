using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Entities.Definitions;
namespace RTSEngine.Core.Entities.Buildings;

public sealed class Building : Entity
{
    public int OwnerId { get; }

    public BuildingDefinition Definition { get; }

    public int CurrentHealth { get; set; }

    public int ConstructionProgress { get; set; }

    public bool IsCompleted { get; set; }

    public override bool IsBlocking => true;

        public Building(
        int ownerId,
        GridPosition position,
        BuildingDefinition definition)
    {
        OwnerId = ownerId;
        Position = position;
        Definition = definition;
    }
}