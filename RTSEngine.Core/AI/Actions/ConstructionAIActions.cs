
using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Actions;
using RTSEngine.Core.Commands;
using RTSEngine.Core.Players;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.AI.Planning;
using RTSEngine.Core.Helpers;

namespace RTSEngine.Core.AI.Actions;

public static class ConstructionAIActions
{
    public static bool RequestConstruction(
        RuntimeContext context,
        Player player,
        string buildingId)
    {
        var definition = context.BuildingRepository.Get(buildingId);

        return RequestConstruction(
            context,
            player,
            definition);
    }

    private static bool RequestConstruction(
        RuntimeContext context,
        Player player,
        BuildingDefinition definition)
    {
        var world = context.World;

        Unit? builder = UnitQueries
            .FindIdleVillagers(world, player)
            .FirstOrDefault();

        if (builder == null)
        {
            return false;
        }

        GridPosition? position =
            BuildingPlanner.FindBuildPosition(
                world,
                player,
                definition);

        if (position == null)
        {
            return false;
        }

        Building? building =
            BuildingPlacementActions.PlaceFoundation(
                world,
                player,
                definition,
                position.Value);

        if (building == null)
        {
            return false;
        }

        world.AddCommand(new BuildCommand
        {
            UnitIds = [builder.Id],
            BuildingId = building.Id
        });

        return true;
    }
}