using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Systems;
using RTSEngine.Core.Settings;
using RTSEngine.Core.Systems.Pathfinding;
using RTSEngine.Core.State;
using RTSEngine.Core.Commands;
using RTSEngine.Tests.TestHelpers;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.Definitions;
namespace RTSEngine.Tests.Systems;

public class MovementSystemTests
{
    [Theory]
    [Trait("Category", "Movement")]
    [InlineData(5, 6, 5)]
    [InlineData(3, 5, 5)]
    public void Update_ShouldMoveUnitOnlyAfterEnoughProgress(int iterations, int expectedX, int expectedY)
    {
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

        CommandSystem.AssignMoveTarget(
            villager,
            new GridPosition(6,5),
            context);

        world.Entities.Add(villager, player);

        for (int i = 0; i < iterations; i++)
        {
            MovementSystem.Update(context);
        }

        Assert.Equal(expectedX, villager.Position.X);
        Assert.Equal(expectedY, villager.Position.Y);
    }

    [Theory]
    [Trait("Category", "Movement")]
    [InlineData("BlockedByTerrain")]
    [InlineData("BlockedByOccupant")]
    public void Update_ShouldNotMoveIntoBlockedTile(string scenario)
    {
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

        CommandSystem.AssignMoveTarget(
            villager,
            new GridPosition(6,5),
            context);

        if (scenario == "BlockedByTerrain")
        {
            world.Map.SetTile(6, 5,
                new Tile
                {
                    TerrainType = TileType.Water
                });
            world.Entities.Add(villager, player);
        }
        else
        {
            var villagerB = UnitFactory.Create(
                villagerDefinition,
                1,
                new GridPosition(6, 5));
            world.Entities.Add(villager, player);
            world.Entities.Add(villagerB, player);
        }

        for (int i = 0; i < 4; i++)
        {
            MovementSystem.Update(context);
        }

        Assert.Equal(5, villager.Position.X);
        Assert.Equal(5, villager.Position.Y);
    }

    [Fact]
    [Trait("Category", "Movement")]
    public void Update_ShouldNotTeleportToDistantTile()
    {
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

        
        CommandSystem.AssignMoveTarget(
            villager,
            new GridPosition(9,9),
            context);

        world.Entities.Add(villager, player);

        // Act
        for (int i = 0; i < 10; i++)
        {
            MovementSystem.Update(context);
        }

        // Assert
        Assert.NotEqual(9, villager.Position.X);
        Assert.NotEqual(9, villager.Position.Y);
    }

    [Theory]
    [Trait("Category", "Movement")]
    [InlineData(EntityState.Moving)]
    [InlineData(EntityState.Gathering)]
    public void Repath_ShouldRecomputePath_WhenBlocked(EntityState initialTask)
    {
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

        var def = new UnitDefinition
        {
            Id = "villager",
            Name = "Villager",
            MaxHealth = 50,
            MovementSpeed = 1f
        };

        var villager = UnitFactory.Create(
            def,
            1,
            new GridPosition(2, 2));

        villager.CurrentTask = initialTask;
        villager.Movement.NeedsRepath = true;

        world.Entities.Add(villager, player);

        MovementSystem.Update(context);

        if (initialTask == EntityState.Moving)
        {
            Assert.False(villager.Movement.NeedsRepath);
        }
        else
        {
            Assert.True(villager.Movement.NeedsRepath);
        }
    }

    [Theory]
    [Trait("Category", "Movement")]
    [InlineData(EntityState.Moving)]
    [InlineData(EntityState.Gathering)]
    public void MoveCommand_ShouldSetTaskToMoving(EntityState initialTask)
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player = world.GetPlayerById(1)!;

        var def = new UnitDefinition
        {
            Id = "villager",
            Name = "Villager",
            MaxHealth = 50,
            MovementSpeed = 1f
        };

        var villager = UnitFactory.Create(
            def,
            1,
            new GridPosition(2, 2));

        villager.CurrentTask = initialTask;

        world.Entities.Add(villager, player);

        var queue = new CommandQueue();
        queue.Enqueue(new MoveCommand
        {
            UnitIds = [villager.Id],
            Target = new GridPosition(5, 2)
        });

        CommandSystem.Update(
            new RuntimeContext
            {
                World = world,
                UnitRepository = new UnitDefinitionRepository([]),
                BuildingRepository = new BuildingDefinitionRepository([]),
                CommandQueue = queue,
                PathFinder = new AStarPathFinder(new GroundMovementFilter()),
                Settings = new GameSettings()
            });

        Assert.Equal(EntityState.Moving, villager.CurrentTask);
    }

    [Fact]
    [Trait("Category", "Movement")]
    public void PathCompletion_ShouldSetTaskToIdle()
    {
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

        var def = new UnitDefinition
        {
            Id = "villager",
            Name = "Villager",
            MaxHealth = 50,
            MovementSpeed = 1f
        };

        var villager = UnitFactory.Create(
            def,
            1,
            new GridPosition(2, 2));

        villager.CurrentTask = EntityState.Moving;

        world.Entities.Add(villager, player);

        CommandSystem.AssignMoveTarget(
            villager,
            new GridPosition(3, 2),
            context);

        for (int i = 0; i < 5; i++)
        {
            MovementSystem.Update(context);
        }

        Assert.Equal(EntityState.Idle, villager.CurrentTask);
    }

    [Fact]
    [Trait("Category", "Movement")]
    public void HighSpeed_ShouldMoveMultipleTilesPerTick()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers(width: 20, height: 20);
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

        var scoutDef = new UnitDefinition
        {
            Id = "scout",
            Name = "Scout",
            MaxHealth = 45,
            MovementSpeed = 1.8f
        };

        var scout = UnitFactory.Create(
            scoutDef,
            1,
            new GridPosition(2, 2));

        world.Entities.Add(scout, player);

        CommandSystem.AssignMoveTarget(
            scout,
            new GridPosition(18, 2),
            context);

        for (int i = 0; i < 5; i++)
        {
            MovementSystem.Update(context);
        }

        Assert.True(scout.Position.X > 7,
            $"Scout with speed 1.8 should move ~9 tiles in 5 ticks, was at X={scout.Position.X}");
    }

    [Fact]
    [Trait("Category", "Movement")]
    public void fractionalSpeed_ShouldNotLoseProgress()
    {
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

        var def = new UnitDefinition
        {
            Id = "unit",
            Name = "Unit",
            MaxHealth = 50,
            MovementSpeed = 0.6f
        };

        var unit = UnitFactory.Create(
            def,
            1,
            new GridPosition(0, 0));

        world.Entities.Add(unit, player);

        CommandSystem.AssignMoveTarget(
            unit,
            new GridPosition(9, 0),
            context);

        for (int i = 0; i < 10; i++)
        {
            MovementSystem.Update(context);
        }

        Assert.Equal(6, unit.Position.X);
        Assert.Equal(0, unit.Position.Y);
    }
}
