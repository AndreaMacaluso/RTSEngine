using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Systems;
using RTSEngine.Tests.TestHelpers;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Commands;
using RTSEngine.Core.Settings;
using RTSEngine.Core.Systems.Pathfinding;
namespace RTSEngine.Tests.Systems.QueuedMovementTests;

public class QueuedMovementTests
{
    [Fact]
    [Trait("Category", "Movement")]
    public void Update_ShouldFollowQueuedPathAndEmptyOnCompletion()
    {
        // Arrange
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = new RuntimeContext
        {
            World = world,
            UnitRepository = new UnitDefinitionRepository([]),
            BuildingRepository = new BuildingDefinitionRepository([]),
            CommandQueue = new CommandQueue(),
            PathFinder = new AStarPathFinder(new GroundMovementFilter()),
            Settings = new GameSettings()
        };
        var player = world.GetPlayerById(1)!;

        var villagerDefinition = new UnitDefinition
        {
            Id = "villager",
            Name = "Villager",
            MaxHealth = 50,
            MovementSpeed = 0.25f
        };

        var villager = UnitFactory.Create(
            villagerDefinition,
            1,
            new GridPosition(5, 5));

        villager.Movement.PathQueue.Enqueue(
            new GridPosition(6, 5));

        villager.Movement.PathQueue.Enqueue(
            new GridPosition(7, 5));

        villager.Movement.PathQueue.Enqueue(
            new GridPosition(8, 5));

        world.Entities.Add(villager, player);

        // Act
        for (int i = 0; i <= 12; i++)
        {
            MovementSystem.Update(context);
        }

        // Assert
        Assert.Equal(8, villager.Position.X);
        Assert.Equal(5, villager.Position.Y);

        Assert.Empty(villager.Movement.PathQueue);

        Assert.Null(villager.Movement.Destination);
    }
    [Fact]
    [Trait("Category", "Movement")]
    public void Update_ShouldStopWhenPathIsBlocked()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers(
            TileType.Grass);
        var context = new RuntimeContext
        {
            World = world,
            UnitRepository = new UnitDefinitionRepository([]),
            BuildingRepository = new BuildingDefinitionRepository([]),
            CommandQueue = new CommandQueue(),
            PathFinder = new AStarPathFinder(new GroundMovementFilter()),
            Settings = new GameSettings()
        };
        var player = world.GetPlayerById(1)!;

        world.Map.SetTile(
            7,
            5,
            new Tile
            {
                TerrainType = TileType.Water
            });

        var villagerDefinition = new UnitDefinition
        {
            Id = "villager",
            Name = "Villager",
            MaxHealth = 50,
            MovementSpeed = 0.25f
        };

        var villager = UnitFactory.Create(
            villagerDefinition,
            1,
            new GridPosition(5, 5));

        villager.Movement.PathQueue.Enqueue(
            new GridPosition(6, 5));

        villager.Movement.PathQueue.Enqueue(
            new GridPosition(7, 5));

        world.Entities.Add(villager, player);

        for (int i = 0; i <= 12; i++)
        {
            MovementSystem.Update(context);
        }

        Assert.Equal(6, villager.Position.X);
        Assert.Equal(5, villager.Position.Y);
    }

}
