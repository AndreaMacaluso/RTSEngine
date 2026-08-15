using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Players;
using RTSEngine.Core.State;
using RTSEngine.Core.Diagnostics;

namespace RTSEngine.Core.AI.Planning;

public static class ProductionPlanner
{
    private const int TargetPopulation = 15;
    private const int VillagerCostFood = 50;

    public static Building? FindTownCenter(
        GameWorld world,
        Player player)
    {
        var tc = world.Entities
            .OfType<Building>()
            .FirstOrDefault(building =>
                building.OwnerId == player.Id &&
                building.Definition.Id == "town_center" &&
                building.IsCompleted);

        if (tc == null)
        {
            DebugSession.Log.Info(
                "ProductionPlanner.FindTownCenter: no TC found",
                [("PlayerId", player.Id)]);
        }

        return tc;
    }

    public static bool CanTrainVillager(
        Player player,
        Building townCenter)
    {
        if (player.Population.Current >= TargetPopulation)
        {
            DebugSession.Log.Info(
                "ProductionPlanner.CanTrainVillager: false - target population reached",
                [
                    ("PlayerId", player.Id),
                    ("Current", player.Population.Current),
                    ("Target", TargetPopulation)
                ]);
            return false;
        }

        if (player.Population.Current >= player.Population.Capacity)
        {
            DebugSession.Log.Info(
                "ProductionPlanner.CanTrainVillager: false - population cap reached",
                [
                    ("PlayerId", player.Id),
                    ("Current", player.Population.Current),
                    ("Capacity", player.Population.Capacity)
                ]);
            return false;
        }

        if (townCenter.Production.IsProducing)
        {
            DebugSession.Log.Info(
                "ProductionPlanner.CanTrainVillager: false - TC is busy",
                [
                    ("PlayerId", player.Id),
                    ("BuildingId", townCenter.Id)
                ]);
            return false;
        }

        if (!player.Economy.Has(ResourceType.Food, VillagerCostFood))
        {
            DebugSession.Log.Info(
                "ProductionPlanner.CanTrainVillager: false - not enough Food",
                [
                    ("PlayerId", player.Id),
                    ("Food", player.Economy.Get(ResourceType.Food)),
                    ("Required", VillagerCostFood)
                ]);
            return false;
        }

        DebugSession.Log.Info(
            "ProductionPlanner.CanTrainVillager: true",
            [
                ("PlayerId", player.Id),
                ("Food", player.Economy.Get(ResourceType.Food)),
                ("Pop", player.Population.Current),
                ("Cap", player.Population.Capacity)
            ]);
        return true;
    }

    public static int CountHouses(
        GameWorld world,
        Player player)
    {
        return world.Entities
            .OfType<Building>()
            .Count(building =>
                building.OwnerId == player.Id &&
                building.Definition.Id == "house" &&
                building.IsCompleted);
    }

    public static int GetTargetPopulation()
    {
        return TargetPopulation;
    }
}
