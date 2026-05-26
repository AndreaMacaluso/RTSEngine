namespace RTSEngine.Core.Map.Loading;

using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Map.Definitions;
using RTSEngine.Core.Entities.Resources;
public static class ResourceFactory
{
    public static ResourceNode Create(
        ResourceDefinition definition)
    {
        var position = new GridPosition(
            definition.X,
            definition.Y);

        return definition.Type switch
        {
            "tree" => new Tree(position),

            "berry_bush" => new BerryBush(position),

            "gold_mine" => new GoldMine(position),

            "stone_mine" => new StoneMine(position),

            _ => throw new Exception(
                $"Unknown resource type: {definition.Type}")
        };
    }
}