using RTSEngine.Core.Map.Runtime;
namespace RTSEngine.Core.Entities.Definitions;

public class BuildingDefinition
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public int MaxHealth { get; init; }

    public int Width { get; init; }

    public int Height { get; init; }

    public int BuildTimeTicks { get; init; }
    public List<ResourceCost> Costs { get; init; } = [];
    public List<string> Produces { get; init; } = [];

    public List<ResourceType> AcceptedResources { get; init; } = [];
    public bool CanDeposit(ResourceType resource)
    {
        return AcceptedResources.Contains(resource);
    }

    public int PopulationBonus { get; init; }

    public int MeleeArmor { get; init; }
    public int RangedArmor { get; init; }

    public int Attack { get; init; }
    public int AttackRange { get; init; }
    public int AttackCooldownTicks { get; init; } = 2;

    public bool CanAttack => Attack > 0;
}
