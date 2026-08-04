using RTSEngine.Core.AI.Actions;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Players;

namespace RTSEngine.Core.AI.Decisions;

public static class ConstructionDecision
{
    private const int PopulationBuffer = 2;

    public static void Execute(
        RuntimeContext context,
        Player player)
    {
        string? buildingId = ChooseBuilding(
            context,
            player);

        if (buildingId == null)
        {
            return;
        }

        ConstructionAIActions.RequestConstruction(
            context,
            player,
            buildingId);
    }

    private static string? ChooseBuilding(
        RuntimeContext context,
        Player player)
    {
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