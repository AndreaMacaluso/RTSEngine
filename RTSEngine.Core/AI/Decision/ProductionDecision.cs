using RTSEngine.Core.AI.Actions;
using RTSEngine.Core.AI.Planning;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Players;
using RTSEngine.Core.Diagnostics;

namespace RTSEngine.Core.AI.Decisions;

public static class ProductionDecision
{
    public static void Execute(
        RuntimeContext context,
        Player player)
    {
        DebugSession.Log.Info(
            "ProductionDecision.Execute",
            [("PlayerId", player.Id)]);

        var townCenter = ProductionPlanner.FindTownCenter(
            context.World,
            player);

        if (townCenter == null)
        {
            DebugSession.Log.Info(
                "ProductionDecision.Execute: no TC found, skipping",
                [("PlayerId", player.Id)]);
            return;
        }

        if (!ProductionPlanner.CanTrainVillager(player, townCenter))
        {
            DebugSession.Log.Info(
                "ProductionDecision.Execute: cannot train villager, skipping",
                [("PlayerId", player.Id)]);
            return;
        }

        DebugSession.Log.Info(
            "ProductionDecision.Execute: training villager",
            [
                ("PlayerId", player.Id),
                ("BuildingId", townCenter.Id),
                ("Food", player.Economy.Get(Map.Runtime.ResourceType.Food)),
                ("Pop", player.Population.Current),
                ("Cap", player.Population.Capacity)
            ]);

        var result = ProductionAIActions.TrainVillager(
            context,
            townCenter);

        DebugSession.Log.Info(
            "ProductionDecision.Execute: train result",
            [
                ("PlayerId", player.Id),
                ("Success", result)
            ]);
    }
}
