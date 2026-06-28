using RTSEngine.Core.Commands;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.DebugClient.Bootstrap;

namespace RTSEngine.DebugClient.Scenarios;

public static class ScenarioBuilder
{
    // for the time being this will help with testing but the concept of scenario 
    // will be separete  form the spawn + comand
    public static void CreateMovementScenario(
        SimulationContext context)
    {
        var world = context.World;
        var villagerDefinition =
            context.UnitRepository.Get("villager");

        SpawnUnitWithMoveCommand(
            context,
            villagerDefinition.Id,
            ownerId: 1,
            spawnPosition: new GridPosition(5, 12),
            targetPosition: new GridPosition(20, 25));

        SpawnUnitWithMoveCommand(
            context,
            villagerDefinition.Id,
            ownerId: 2,
            spawnPosition: new GridPosition(35, 30),
            targetPosition: new GridPosition(10, 15));
    }

    private static void SpawnUnitWithMoveCommand(
        SimulationContext context,
        string unitDefinitionId,
        int ownerId,
        GridPosition spawnPosition,
        GridPosition targetPosition)
    {
        var world = context.World;
        var definition = context.UnitRepository.Get(unitDefinitionId);

        var unit = UnitFactory.Create(
            definition,
            ownerId,
            spawnPosition);

        world.AddEntity(unit);

        world.AddCommand(new MoveCommand
        {
            UnitIds = [unit.Id],
            Target = targetPosition
        });
    }
}