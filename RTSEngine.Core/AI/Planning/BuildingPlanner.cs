using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Entities.Rules;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Players;
using RTSEngine.Core.State;

namespace RTSEngine.Core.AI.Planning;

public static class BuildingPlanner
{
    // public static GridPosition? FindHousePosition(
    //     GameWorld world,
    //     Player player,
    //     BuildingDefinition definition)
    // {
    //     return FindBuildPosition(
    //         world,
    //         player,
    //         definition);
    // }

    // public static GridPosition? FindTownCenterPosition(
    //     GameWorld world,
    //     Player player,
    //     BuildingDefinition definition)
    // {
    //     return FindBuildPosition(
    //         world,
    //         player,
    //         definition);
    // }

    // public static GridPosition? FindBarracksPosition(
    //     GameWorld world,
    //     Player player,
    //     BuildingDefinition definition)
    // {
    //     return FindBuildPosition(
    //         world,
    //         player,
    //         definition);
    // }

    public static GridPosition? FindBuildPosition(
        GameWorld world,
        Player player,
        BuildingDefinition definition)
    {
        var townCenter = world.Entities
            .OfType<Building>()
            .FirstOrDefault(building =>
                building.OwnerId == player.Id &&
                building.Definition.Id == "town_center");
        
        if (townCenter == null)
        {
            return null;
        }

        return BuildingPlacementRules.FindFreePosition(
            world,
            definition,
            townCenter.Position);
    }
}