using RTSEngine.Core.AI.Actions;
using RTSEngine.Core.AI.Planning;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Players;
using RTSEngine.Core.Diagnostics;

namespace RTSEngine.Core.AI.Decisions;

public static class ProductionDecision
{
    private const int TargetMilitiaCount = 5;

    public static void Execute(
        RuntimeContext context,
        Player player)
    {
        DebugSession.Log.Info(
            "ProductionDecision.Execute",
            [("PlayerId", player.Id)]);

        if (player.Population.Current >= ProductionPlanner.GetTargetPopulation())
        {
            TrainMilitia(context, player);
            return;
        }

        TrainVillager(context, player);
    }

    private static void TrainVillager(
        RuntimeContext context,
        Player player)
    {
        var townCenter = ProductionPlanner.FindTownCenter(
            context.World,
            player);

        if (townCenter == null)
        {
            DebugSession.Log.Info(
                "ProductionDecision.TrainVillager: no TC found, skipping",
                [("PlayerId", player.Id)]);
            return;
        }

        if (!ProductionPlanner.CanTrainVillager(player, townCenter))
        {
            DebugSession.Log.Info(
                "ProductionDecision.TrainVillager: cannot train villager, skipping",
                [("PlayerId", player.Id)]);
            return;
        }

        var result = ProductionAIActions.TrainVillager(
            context,
            townCenter);

        DebugSession.Log.Info(
            "ProductionDecision.TrainVillager: train result",
            [
                ("PlayerId", player.Id),
                ("Success", result)
            ]);
    }

    private static void TrainMilitia(
        RuntimeContext context,
        Player player)
    {
        var barracks = ProductionPlanner.FindBarracks(
            context.World,
            player);

        if (barracks == null)
        {
            DebugSession.Log.Info(
                "ProductionDecision.TrainMilitia: no barracks found",
                [("PlayerId", player.Id)]);
            return;
        }

        int militiaCount = ProductionPlanner.CountMilitiaInWorld(
            context.World,
            player);

        if (militiaCount >= TargetMilitiaCount)
        {
            DebugSession.Log.Info(
                "ProductionDecision.TrainMilitia: target militia count reached",
                [
                    ("PlayerId", player.Id),
                    ("Count", militiaCount),
                    ("Target", TargetMilitiaCount)
                ]);
            return;
        }

        if (barracks.Production.IsProducing)
        {
            return;
        }

        if (!player.Economy.Has(Map.Runtime.ResourceType.Food, 50))
        {
            return;
        }

        var result = ProductionAIActions.TrainMilitia(
            context,
            barracks);

        DebugSession.Log.Info(
            "ProductionDecision.TrainMilitia: train result",
            [
                ("PlayerId", player.Id),
                ("Success", result),
                ("MilitiaCount", militiaCount)
            ]);
    }
}
