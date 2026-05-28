using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Systems;
using RTSEngine.Tests.TestHelpers;

namespace RTSEngine.Tests.Systems.QueuedMovementTests;

public class MovementSystemTests
{
    [Fact]
    [Trait("Category", "Movement")]
    public void Update_ShouldFollowQueuedPath()
    {
        // Arrange
        var world = TestWorldFactory.CreateWorld();

        var villager = new Villager(
            ownerId: 1,
            position: new GridPosition(5, 5));

        villager.PathQueue.Enqueue(
            new GridPosition(6, 5));

        villager.PathQueue.Enqueue(
            new GridPosition(7, 5));

        villager.PathQueue.Enqueue(
            new GridPosition(8, 5));

        world.AddEntity(villager);

        // Act
        for (int i = 0; i <= 12; i++)
        {
            MovementSystem.Update(world);
        }

        // Assert
        Assert.Equal(8, villager.Position.X);
        Assert.Equal(5, villager.Position.Y);

        Assert.Empty(villager.PathQueue);
    }
    [Fact]
    [Trait("Category", "Movement")]
    public void Update_ShouldEmptyQueueAfterPathCompletion()
    {
        var world = TestWorldFactory.CreateWorld();

        var villager = new Villager(
            1,
            new GridPosition(5, 5));

        villager.PathQueue.Enqueue(
            new GridPosition(6, 5));

        world.AddEntity(villager);

        for (int i = 0; i <= 4; i++)
        {
            MovementSystem.Update(world);
        }

        Assert.Empty(villager.PathQueue);

        Assert.Null(villager.TargetPosition);
    }
    [Fact]
    [Trait("Category", "Movement")]
    public void Update_ShouldStopWhenPathIsBlocked()
    {
        var world = TestWorldFactory.CreateWorld(
            TileType.Grass);

        world.Map.SetTile(
            7,
            5,
            new Tile
            {
                TerrainType = TileType.Water
            });

        var villager = new Villager(
            1,
            new GridPosition(5, 5));

        villager.PathQueue.Enqueue(
            new GridPosition(6, 5));

        villager.PathQueue.Enqueue(
            new GridPosition(7, 5));

        world.AddEntity(villager);

        for (int i = 0; i <= 12; i++)
        {
            MovementSystem.Update(world);
        }

        Assert.Equal(6, villager.Position.X);
        Assert.Equal(5, villager.Position.Y);
    }

}