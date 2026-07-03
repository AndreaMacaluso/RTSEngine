using  RTSEngine.Core.Map.Runtime;
namespace RTSEngine.Core.Entities.Definitions;

public class BuildingDefinition
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public int MaxHealth { get; init; }

    public int Width { get; init; }

    public int Height { get; init; }

    public int BuildTimeTicks { get; init; }

    public Dictionary<ResourceType, int> Costs { get; init; } = [];

    public List<string> Produces { get; init; } = [];

    public List<ResourceType> AcceptedResources { get; init; } = [];
    public bool CanDeposit(ResourceType resource)
    {
            return AcceptedResources.Contains(resource);
    }
}