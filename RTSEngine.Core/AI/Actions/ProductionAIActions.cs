using RTSEngine.Core.Actions;
using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Helpers;
using RTSEngine.Core.Players;
using RTSEngine.Core.Diagnostics;

namespace RTSEngine.Core.AI.Actions;

public static class ProductionAIActions
{
    public static bool TrainVillager(
        RuntimeContext context,
        Building townCenter)
    {
        var player = context.World.GetPlayerById(townCenter.OwnerId);

        if (player == null)
        {
            return false;
        }

        DebugSession.Log.Info(
            "ProductionAIActions.TrainVillager: calling TryTrainUnit",
            [
                ("PlayerId", player.Id),
                ("BuildingId", townCenter.Id),
                ("Food", player.Economy.Get(Map.Runtime.ResourceType.Food)),
                ("Pop", player.Population.Current),
                ("Cap", player.Population.Capacity),
                ("Reserved", player.Population.Reserved),
                ("IsProducing", townCenter.Production.IsProducing)
            ]);

        var result = ProductionActions.TryTrainUnit(
            context,
            townCenter,
            EntityIds.Villager);

        DebugSession.Log.Info(
            "ProductionAIActions.TrainVillager: TryTrainUnit result",
            [
                ("PlayerId", player.Id),
                ("Success", result),
                ("FoodAfter", player.Economy.Get(Map.Runtime.ResourceType.Food)),
                ("ReservedAfter", player.Population.Reserved)
            ]);

        return result;
    }

    public static bool TrainMilitia(
        RuntimeContext context,
        Building barracks)
    {
        var player = context.World.GetPlayerById(barracks.OwnerId);

        if (player == null)
        {
            return false;
        }

        var result = ProductionActions.TryTrainUnit(
            context,
            barracks,
            EntityIds.Militia);

        DebugSession.Log.Info(
            "ProductionAIActions.TrainMilitia: TryTrainUnit result",
            [
                ("PlayerId", player.Id),
                ("Success", result),
                ("BuildingId", barracks.Id)
            ]);

        return result;
    }
}
