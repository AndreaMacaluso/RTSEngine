using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Systems;
using RTSEngine.Core.State;
using RTSEngine.Core.Commands;
using RTSEngine.Tests.TestHelpers;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.Definitions;
namespace RTSEngine.Tests.Systems;

public class MovementSystemTests
{
    [Fact]
    [Trait("Category", "Movement")]
    public void Update_ShouldMoveUnitAfterEnoughProgress()
    {
        var world = TestWorldFactory.CreateWorld();
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
            world);

        world.AddEntity(villager);

        // Act
        for (int i = 0; i < 5; i++)
        {
            MovementSystem.Update(world);
        }

        // Assert
        Assert.Equal(6, villager.Position.X);
        Assert.Equal(5, villager.Position.Y);
    }

    [Fact]
    [Trait("Category", "Movement")]
    public void Update_ShouldNotMoveUnitBeforeEnoughProgress()
    {
        // Arrange
      

        var world = TestWorldFactory.CreateWorld();

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
            world);

        world.AddEntity(villager);

        // Act
        MovementSystem.Update(world);
        MovementSystem.Update(world);
        MovementSystem.Update(world);

        // Assert
        Assert.Equal(5, villager.Position.X);
        Assert.Equal(5, villager.Position.Y);
    }

    [Fact]
    [Trait("Category", "Movement")]
    public void Update_ShouldNotMoveIntoBlockedTile()
    {
       

        var world = TestWorldFactory.CreateWorld();
        world.Map.SetTile(6, 5,
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

       
        CommandSystem.AssignMoveTarget(
            villager,
            new GridPosition(6,5),
            world);

        world.AddEntity(villager);

        // Act
        for (int i = 0; i < 4; i++)
        {
            MovementSystem.Update(world);
        }

        // Assert
        Assert.Equal(5, villager.Position.X);
        Assert.Equal(5, villager.Position.Y);
    }

    [Fact]
    [Trait("Category", "Movement")]

    public void Update_ShouldNotMoveIntoOccupiedTile()
    {
        // Arrange
        var world = TestWorldFactory.CreateWorld();
        var villagerDefinition = new UnitDefinition
        {
            Id = "villager",
            Name = "Villager",
            MaxHealth = 50,
            MovementSpeed = 0.25f
        };

        var villagerA = UnitFactory.Create(
            villagerDefinition,
            1,
            new GridPosition(5, 5));

        
        CommandSystem.AssignMoveTarget(
            villagerA,
            new GridPosition(6,5),
            world);
       

        var villagerB = UnitFactory.Create(
            villagerDefinition,
            1,
            new GridPosition(6, 5));
       
        world.AddEntity(villagerA);
        world.AddEntity(villagerB);

        // Act
        for (int i = 0; i < 4; i++)
        {
            MovementSystem.Update(world);
        }

        // Assert
        Assert.Equal(5, villagerA.Position.X);
        Assert.Equal(5, villagerA.Position.Y);
    }

    [Fact]
    [Trait("Category", "Movement")]
    public void Update_ShouldNotTeleportToDistantTile()
    {
        // Arrange
        var world = TestWorldFactory.CreateWorld();

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
            world);

        world.AddEntity(villager);

        // Act
        for (int i = 0; i < 10; i++)
        {
            MovementSystem.Update(world);
        }

        // Assert
        Assert.NotEqual(9, villager.Position.X);
        Assert.NotEqual(9, villager.Position.Y);
    }

    [Fact]
    [Trait("Category", "Movement")]
    public void Repath_ShouldRecomputePath_WhenBlocked()
    {
        var world = TestWorldFactory.CreateWorld();

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

        world.AddEntity(villager);

        CommandSystem.AssignMoveTarget(
            villager,
            new GridPosition(6, 2),
            world);

        villager.CurrentTask = UnitTask.Moving;

        world.Map.SetTile(4, 2,
            new Tile { TerrainType = TileType.Water });

        for (int i = 0; i < 20; i++)
        {
            MovementSystem.Update(world);
        }

        Assert.Equal(6, villager.Position.X);
        Assert.Equal(2, villager.Position.Y);
        Assert.False(villager.Movement.NeedsRepath);
    }

    [Fact]
    [Trait("Category", "Movement")]
    public void Repath_ShouldNotAffectGatheringUnits()
    {
        var world = TestWorldFactory.CreateWorld();

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

        villager.CurrentTask = UnitTask.Gathering;
        villager.Movement.NeedsRepath = true;

        world.AddEntity(villager);

        MovementSystem.Update(world);

        Assert.True(villager.Movement.NeedsRepath);
    }

    [Fact]
    [Trait("Category", "Movement")]
    public void MoveCommand_ShouldSetTaskToMoving()
    {
        var world = TestWorldFactory.CreateWorld();

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

        world.AddEntity(villager);

        world.AddCommand(new MoveCommand
        {
            UnitIds = [villager.Id],
            Target = new GridPosition(5, 2)
        });

        CommandSystem.Update(
            new RTSEngine.Core.Entities.Runtime.RuntimeContext
            {
                World = world,
                UnitRepository = new UnitDefinitionRepository([]),
                BuildingRepository = new BuildingDefinitionRepository([])
            });

        Assert.Equal(UnitTask.Moving, villager.CurrentTask);
    }

    [Fact]
    [Trait("Category", "Movement")]
    public void MoveCommand_ShouldNotOverwriteGatheringTask()
    {
        var world = TestWorldFactory.CreateWorld();

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

        villager.CurrentTask = UnitTask.Gathering;

        world.AddEntity(villager);

        world.AddCommand(new MoveCommand
        {
            UnitIds = [villager.Id],
            Target = new GridPosition(5, 2)
        });

        CommandSystem.Update(
            new RTSEngine.Core.Entities.Runtime.RuntimeContext
            {
                World = world,
                UnitRepository = new UnitDefinitionRepository([]),
                BuildingRepository = new BuildingDefinitionRepository([])
            });

        Assert.Equal(UnitTask.Gathering, villager.CurrentTask);
    }

    [Fact]
    [Trait("Category", "Movement")]
    public void PathCompletion_ShouldSetTaskToIdle()
    {
        var world = TestWorldFactory.CreateWorld();

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

        villager.CurrentTask = UnitTask.Moving;

        world.AddEntity(villager);

        CommandSystem.AssignMoveTarget(
            villager,
            new GridPosition(3, 2),
            world);

        for (int i = 0; i < 5; i++)
        {
            MovementSystem.Update(world);
        }

        Assert.Equal(UnitTask.Idle, villager.CurrentTask);
    }
}