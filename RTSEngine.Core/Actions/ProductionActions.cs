using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.States;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Commands;
namespace RTSEngine.Core.Actions;

public static class ProductionActions
{
    public static void ProduceOneTick(
        RuntimeContext context,
        Building building)
    {
        var task = building.Production.Current;

        if (task == null)
        {
            return;
        }

        task.Tick();

        if (!task.Completed)
        {
            return;
        }
        //@ToDo if spawn point does not exist the produced unit is lost
        SpawnUnit(
            context,
            building,
            task);

        building.Production.RemoveCurrent();
    }

    private static void SpawnUnit(
        RuntimeContext context,
        Building building,
        ProductionTask task)
    {
        var definition =
            context.UnitRepository.Get(task.ProductId);

        var spawnPosition = building.Production.SpawnPoint;

        if (spawnPosition is null)
        {
            return;
        }
        
        Unit unit = UnitFactory.Create(
            definition,
            building.OwnerId,
            spawnPosition.Value);

        context.World.AddEntity(unit);

        CompleteUnitSpawned(context, building);
    }

    public static bool TrainUnit(
    RuntimeContext context,
    Building building,
    string unitId)
    {
        if (!context.UnitRepository.Exists(unitId))
        {
            return false;
        }

        if (!building.Definition.Produces.Contains(unitId))
        {
            return false;
        }

        context.World.AddCommand(
            new QueueProductionCommand
            (
                building.OwnerId,
                building.Id,
                unitId
        ));

        return true;
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
            if (!player.Economy.Has(cost.Type, cost.Amount)) { return false; }

        if (!PopulationActions.TryReservePopulation(player, 1)) { return false; }

        foreach (var cost in unitDefinition.Costs)
            player.Economy.Spend(cost.Type, cost.Amount);

        return TrainUnit(context, building, unitId);
    }

    private static void CompleteUnitSpawned(
        RuntimeContext context,
        Building building)
    {
        var player = context.World.GetPlayerById(building.OwnerId)!;
        PopulationActions.CompleteReservedPopulation(player, 1);
    }
}