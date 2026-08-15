
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
using RTSEngine.Core.Diagnostics;
using RTSEngine.Core.State;

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
            DebugSession.Log.Info(
                "ConstructionAIActions: no idle villager found",
                [("PlayerId", player.Id)]);
            return false;
        }

        DebugSession.Log.Info(
            "ConstructionAIActions: found idle villager",
            [
                ("PlayerId", player.Id),
                ("VillagerId", builder.Id),
                ("Position", builder.Position)
            ]);

        GridPosition? position =
            BuildingPlanner.FindBuildPosition(
                world,
                player,
                definition);

        if (position == null)
        {
            DebugSession.Log.Info(
                "ConstructionAIActions: no build position found",
                [
                    ("PlayerId", player.Id),
                    ("BuildingId", definition.Id)
                ]);
            return false;
        }

        DebugSession.Log.Info(
            "ConstructionAIActions: found build position",
            [
                ("PlayerId", player.Id),
                ("BuildingId", definition.Id),
                ("Position", position.Value)
            ]);

        Building? building =
            BuildingPlacementActions.PlaceFoundation(
                world,
                player,
                definition,
                position.Value);

        if (building == null)
        {
            DebugSession.Log.Info(
                "ConstructionAIActions: PlaceFoundation failed",
                [
                    ("PlayerId", player.Id),
                    ("BuildingId", definition.Id),
                    ("Position", position.Value),
                    ("Wood", player.Economy.Get(Map.Runtime.ResourceType.Wood))
                ]);
            return false;
        }

        DebugSession.Log.Info(
            "ConstructionAIActions: foundation placed, queuing build command",
            [
                ("PlayerId", player.Id),
                ("BuildingId", building.Id),
                ("DefinitionId", definition.Id),
                ("VillagerId", builder.Id)
            ]);

        builder.CurrentTask = UnitTask.Building;

        world.AddCommand(new BuildCommand
        {
            UnitIds = [builder.Id],
            BuildingId = building.Id
        });

        return true;
    }
}