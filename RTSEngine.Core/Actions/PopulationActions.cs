using RTSEngine.Core.Commands;
using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Players;
using RTSEngine.Core.State;

namespace RTSEngine.Core.Actions;

public static class PopulationActions
{
    public const int MaxPopulation = 75;

    public static bool CanAddPopulation(
        Player player,
        int amount)
    {
        return
            player.Population.Current + player.Population.Reserved + amount <= player.Population.Capacity &&
            player.Population.Current + player.Population.Reserved + amount <= MaxPopulation;
    }

    public static void AddPopulation(
        Player player,
        int amount)
    {
        player.Population.Current += amount;
    }

    public static bool TryReservePopulation(
        Player player,
        int amount)
    {
        if (!CanAddPopulation(player, amount))
        {
            return false;
        }

        player.Population.Reserved += amount;
        return true;
    }

    public static void CompleteReservedPopulation(
        Player player,
        int amount)
    {
        if (amount > player.Population.Reserved)
        {
            throw new InvalidOperationException(
                "Cannot complete more population than is reserved.");
        }

        player.Population.Reserved -= amount;
        player.Population.Current += amount;
    }

    public static void ReleaseReservedPopulation(
        Player player,
        int amount)
    {
        if (amount > player.Population.Reserved)
        {
            throw new InvalidOperationException(
                "Cannot release more population than is reserved.");
        }

        player.Population.Reserved -= amount;
    }

    public static void RemovePopulation(
        Player player,
        int amount)
    {
        player.Population.Current -= amount;
    }

    public static void IncreaseCap(
        Player player,
        int amount)
    {
        player.Population.Capacity =
            Math.Min(
                player.Population.Capacity + amount,
                MaxPopulation);
    }

     public static void DecreaseCap(
         Player player,
         int amount)
    {
        player.Population.Capacity =
            Math.Max(
                0,
                player.Population.Capacity - amount);
    }

    public static bool TryTrainUnit(
        RuntimeContext context,
        Building building,
        string unitId)
    {
        if (!context.UnitRepository.Exists(unitId)) { return false; }
        if (!building.Definition.Produces.Contains(unitId)) { return false; }

        var unitDefinition = context.UnitRepository.Get(unitId);
        var player = context.World.GetPlayerById(building.OwnerId)!;

        foreach (var cost in unitDefinition.Costs)
        {
            if (!player.Economy.Has(cost.Type, cost.Amount)) { return false; }
        }
        if (!TryReservePopulation(player, 1)) { return false; }

        foreach (var cost in unitDefinition.Costs)
        {
            player.Economy.Spend(cost.Type, cost.Amount);
        }
        context.World.AddCommand(new TrainUnitCommand
        {
            BuildingId = building.Id,
            UnitDefinitionId = unitId
        });
        return true;
    }
}
