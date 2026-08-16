using RTSEngine.Core.AI.Actions;
using RTSEngine.Core.AI.Planning;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Players;
using RTSEngine.Core.Diagnostics;

namespace RTSEngine.Core.AI.Decisions;

public static class ConstructionDecision
{
    private const int PopulationBuffer = 2;

    public static void Execute(
        RuntimeContext context,
        Player player)
    {
        DebugSession.Log.Info(
            "ConstructionDecision.Execute",
            [("PlayerId", player.Id)]);

        string? buildingId = ChooseBuilding(
            context,
            player);

        if (buildingId == null)
        {
            DebugSession.Log.Info(
                "ConstructionDecision.Execute: no building needed, skipping",
                [("PlayerId", player.Id)]);
            return;
        }

        DebugSession.Log.Info(
            "ConstructionDecision.Execute: requesting construction",
            [
                ("PlayerId", player.Id),
                ("BuildingId", buildingId),
                ("Pop", player.Population.Current),
                ("Cap", player.Population.Capacity)
            ]);

        var result = ConstructionAIActions.RequestConstruction(
            context,
            player,
            buildingId);

        DebugSession.Log.Info(
            "ConstructionDecision.Execute: construction result",
            [
                ("PlayerId", player.Id),
                ("BuildingId", buildingId),
                ("Success", result)
            ]);
    }

    private static string? ChooseBuilding(
        RuntimeContext context,
        Player player)
    {
        if (player.Population.Current >= ProductionPlanner.GetTargetPopulation()
            && !ProductionPlanner.HasBarracks(context.World, player))
        {
            DebugSession.Log.Info(
                "ConstructionDecision.ChooseBuilding: barracks needed",
                [("PlayerId", player.Id)]);
            return "barracks";
        }

        if (NeedMorePopulation(player))
        {
            return "house";
        }

        return null;
    }

    private static bool NeedMorePopulation(
        Player player)
    {
        return
            player.Population.Current + PopulationBuffer >=
            player.Population.Capacity;
    }
}