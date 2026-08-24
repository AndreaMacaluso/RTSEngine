using RTSEngine.Core.Commands;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Helpers;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Players;
using RTSEngine.DebugClient.StartingConditions;


namespace RTSEngine.DebugClient.Scenarios;

public static class ScenarioBuilder
{
    public static void CreateStartingBaseScenario(
        RuntimeContext context)
    {
        MatchStartingConditions.CreateStandard(context);
    }

    // for the time being this will help with testing but the concept of scenario 
    // will be separete  form the spawn + command
    public static void CreateMovementScenario(
        RuntimeContext context)
    {
        var world = context.World;
        var villagerDefinition =
            context.UnitRepository.Get(EntityIds.Villager);
        foreach (Player player in world.Players)
        {
            SpawnUnitWithMoveCommand(
            context,
            villagerDefinition.Id,
            ownerId: player.Id,
            spawnPosition: new GridPosition(5, 12),
            targetPosition: new GridPosition(20, 25));            
        }        
    }

    public static void CreateGatheringScenario(
        RuntimeContext context)
    {
        var world = context.World;
        var villagerDefinition =
            context.UnitRepository.Get(EntityIds.Villager);
        foreach (Player player in world.Players)
        {
            var resource = world.Entities.Resources.Values.FirstOrDefault();
            if (resource == null)
            {
                  return;
            }
  
            SpawnUnitWithGatherCommand(
            context,
            villagerDefinition.Id,
            ownerId: player.Id,
            spawnPosition: new GridPosition(5, 12),
            resourceId:resource.Id);            
        }        
    }
    private static void SpawnUnitWithMoveCommand(
        RuntimeContext context,
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

        var player = world.GetPlayerById(ownerId)
            ?? throw new InvalidOperationException(
                $"Cannot spawn unit for unknown player {ownerId}.");

        world.Entities.Add(unit, player);

        world.AddCommand(new MoveCommand
        {
            UnitIds = [unit.Id],
            Target = targetPosition
        });
    }
    private static void SpawnUnitWithGatherCommand(
    RuntimeContext context,
    string unitDefinitionId,
    int ownerId,
    GridPosition spawnPosition,
    int resourceId)
    {
        var world = context.World;
        var definition = context.UnitRepository.Get(unitDefinitionId);

        var unit = UnitFactory.Create(
            definition,
            ownerId,
            spawnPosition);

        var player = world.GetPlayerById(ownerId)
            ?? throw new InvalidOperationException(
                $"Cannot spawn unit for unknown player {ownerId}.");

        world.Entities.Add(unit, player);

        world.AddCommand(new GatherCommand
        {
            UnitIds = [unit.Id],
            ResourceId = resourceId
        });
    }
}
