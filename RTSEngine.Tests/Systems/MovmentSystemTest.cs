using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Map;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.State;
using RTSEngine.Core.Systems;
using RTSEngine.Tests.TestHelpers;

namespace RTSEngine.Tests.Systems;

public class MovementSystemTests
{
    [Fact]
    [Trait("Category", "Movement")]
    public void Update_ShouldMoveUnitAfterEnoughProgress()
    {
        // Arrange
      

        var world = TestWorldFactory.CreateWorld();

        var villager = new Villager(
            ownerId: 1,
            position: new GridPosition(5, 5));

        villager.TargetPosition =
            new GridPosition(6, 5);

        world.AddEntity(villager);

        // Act
        for (int i = 0; i < 4; i++)
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

        var villager = new Villager(
            ownerId: 1,
            position: new GridPosition(5, 5));

        villager.TargetPosition =
            new GridPosition(6, 5);

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
        var villager = new Villager(
            ownerId: 1,
            position: new GridPosition(5, 5));

        villager.TargetPosition =
            new GridPosition(6, 5);

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

        var villagerA = new Villager(
            ownerId: 1,
            position: new GridPosition(5, 5));

        villagerA.TargetPosition =
            new GridPosition(6, 5);

        var villagerB = new Villager(
            ownerId: 2,
            position: new GridPosition(6, 5));

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

        var villager = new Villager(
            ownerId: 1,
            position: new GridPosition(5, 5));

        villager.TargetPosition =
            new GridPosition(9, 9);

        world.AddEntity(villager);

        // Act
        for (int i = 0; i < 10; i++)
        {
            MovementSystem.Update(world);
        }

        // Assert
        Assert.Equal(5, villager.Position.X);
        Assert.Equal(5, villager.Position.Y);
    }
}