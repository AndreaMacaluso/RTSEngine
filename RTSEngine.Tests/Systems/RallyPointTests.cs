using RTSEngine.Core.State;
using RTSEngine.Core.Systems;
using RTSEngine.Core.Commands;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Helpers;
using RTSEngine.Core.Actions;
using RTSEngine.Tests.TestHelpers;
using RTSEngine.Core.Settings;
using RTSEngine.Core.Systems.Pathfinding;

namespace RTSEngine.Tests.Systems;

public class RallyPointTests
{
    private RuntimeContext CreateContext(GameWorld world)
    {
        return new RuntimeContext
        {
            World = world,
            UnitRepository = new UnitDefinitionRepository([
                TestDefinitionFactory.CreateVillager(),
                TestDefinitionFactory.CreateMilitia()
            ]),
            BuildingRepository = new BuildingDefinitionRepository([
                TestDefinitionFactory.CreateTownCenter(),
                TestDefinitionFactory.CreateBarracks()
            ]),
            CommandQueue = new CommandQueue(),
            PathFinder = new AStarPathFinder(new GroundMovementFilter()),
            Settings = new GameSettings()
        };
    }

    [Fact]
    [Trait("Category", "SpawnPoint")]
    public void SetSpawnPoint_ShouldSetSpawnPoint()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world);
        var player = world.GetPlayerById(1)!;

        var building = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            ownerId: 1,
            position: new GridPosition(5, 5));
        building.Health.CurrentHealth = building.Health.MaxHealth;
        building.IsCompleted = true;
        world.Entities.Add(building, player);

        context.CommandQueue.Enqueue(new ProductionCommand
        {
            PlayerId = 1,
            BuildingId = building.Id,
            Action = ProductionActionType.SetSpawnPoint,
            Target = new GridPosition(8, 8)
        });

        CommandSystem.Update(context);

        Assert.Equal(new GridPosition(8, 8), building.Production.SpawnPoint);
    }

    [Fact]
    [Trait("Category", "RallyPoint")]
    public void SetRallyPoint_ShouldSetRallyPoint()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world);
        var player = world.GetPlayerById(1)!;

        var building = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            ownerId: 1,
            position: new GridPosition(5, 5));
        building.Health.CurrentHealth = building.Health.MaxHealth;
        building.IsCompleted = true;
        world.Entities.Add(building, player);

        context.CommandQueue.Enqueue(new ProductionCommand
        {
            PlayerId = 1,
            BuildingId = building.Id,
            Action = ProductionActionType.SetRallyPoint,
            Target = new GridPosition(8, 8)
        });

        CommandSystem.Update(context);

        Assert.Equal(new GridPosition(8, 8), building.Production.RallyPoint);
    }

    [Fact]
    [Trait("Category", "SpawnPoint")]
    public void SpawnedUnit_ShouldAppearAtSpawnPoint()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world);
        var player = world.GetPlayerById(1)!;

        player.Economy.Add(ResourceType.Food, 100);
        PopulationActions.IncreaseCap(player, 5);

        var building = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            ownerId: 1,
            position: new GridPosition(5, 5));
        building.Health.CurrentHealth = building.Health.MaxHealth;
        building.IsCompleted = true;
        building.Production.SpawnPoint = new GridPosition(5, 8);
        world.Entities.Add(building, player);

        var success = ProductionActions.TryTrainUnit(context, building, "villager");
        Assert.True(success);

        CommandSystem.Update(context);

        for (int i = 0; i < 30; i++)
        {
            ProductionSystem.Update(context);
            MovementSystem.Update(context);
        }

        var units = world.Entities.GetUnits(player).ToList();
        Assert.Single(units);

        bool isAdjacentToBuilding = false;
        foreach (var tile in BuildingQueries.GetOccupiedTiles(building))
        {
            if (WorldQueries.IsAdjacent(tile, units[0].Position))
            {
                isAdjacentToBuilding = true;
                break;
            }
        }
        Assert.True(isAdjacentToBuilding);
    }

    [Fact]
    [Trait("Category", "RallyPoint")]
    public void SpawnedUnit_ShouldMoveToRallyPoint()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world);
        var player = world.GetPlayerById(1)!;

        player.Economy.Add(ResourceType.Food, 100);
        PopulationActions.IncreaseCap(player, 5);

        var building = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            ownerId: 1,
            position: new GridPosition(5, 5));
        building.Health.CurrentHealth = building.Health.MaxHealth;
        building.IsCompleted = true;
        building.Production.RallyPoint = new GridPosition(8, 8);
        world.Entities.Add(building, player);

        var success = ProductionActions.TryTrainUnit(context, building, "villager");
        Assert.True(success);

        CommandSystem.Update(context);

        for (int i = 0; i < 30; i++)
        {
            ProductionSystem.Update(context);
            MovementSystem.Update(context);
        }

        var units = world.Entities.GetUnits(player).ToList();
        Assert.Single(units);
        Assert.NotEqual(new GridPosition(5, 5), units[0].Position);
    }
}
