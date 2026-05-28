using RTSEngine.Core.Entities;
using RTSEngine.Core.Entities.Resources;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.State;
using RTSEngine.Tests.TestHelpers;

namespace RTSEngine.Tests.State;

public class GameWorldTest
{
    [Fact]
    public void IsInsideBounds_ShouldReturnTrue_ForValidPosition()
    {
        // Arrange
        var world = TestWorldFactory.CreateWorld(TileType.Water);

        // Act
        var result = world.IsInsideBounds(5, 5);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsInsideBounds_ShouldReturnFalse_ForNegativePosition()
    {
        // Arrange
        var world = TestWorldFactory.CreateWorld(TileType.Water);

        // Act
        var result = world.IsInsideBounds(-1, 0);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsInsideBounds_ShouldReturnFalse_ForOutOfBoundsPosition()
    {
        // Arrange
        var world = TestWorldFactory.CreateWorld(TileType.Water);

        // Act
        var result = world.IsInsideBounds(10, 10);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetEntityAt_ShouldReturnEntity_WhenTileIsOccupied()
    {
        // Arrange
        var world = TestWorldFactory.CreateWorld(TileType.Water);

        var tree = new Tree(new GridPosition(2, 2));

        world.Entities.Add(tree);

        // Act
        var entity = world.GetEntityAt(2, 2);

        // Assert
        Assert.NotNull(entity);
        Assert.Equal(tree, entity);
    }

    [Fact]
    public void GetEntityAt_ShouldReturnNull_WhenTileIsEmpty()
    {
        // Arrange
        var world = TestWorldFactory.CreateWorld(TileType.Water);

        // Act
        var entity = world.GetEntityAt(2, 2);

        // Assert
        Assert.Null(entity);
    }

    [Fact]
    public void IsTileOccupied_ShouldReturnTrue_WhenEntityExists()
    {
        // Arrange
        var world = TestWorldFactory.CreateWorld(TileType.Water);

        world.Entities.Add(
            new Tree(new GridPosition(1, 1)));

        // Act
        var result = world.IsTileOccupied(1, 1);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsTileOccupied_ShouldReturnFalse_WhenTileIsEmpty()
    {
        // Arrange
        var world = TestWorldFactory.CreateWorld(TileType.Water);

        // Act
        var result = world.IsTileOccupied(1, 1);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsTileBlocked_ShouldReturnTrue_ForWaterTile()
    {
        // Arrange
        var map = new TileMap(2, 2);

        var tile = new Tile
        {
            TerrainType = TileType.Water
        };

        map.SetTile(0,0,tile);

        var world = new GameWorld(map);

        // Act
        var result = world.IsTileBlocked(0, 0);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsTileBlocked_ShouldReturnTrue_ForMountainTile()
    {
        // Arrange
        var map = new TileMap(2, 2);

        var tile = new Tile
        {
            TerrainType = TileType.Mountain
        };

        map.SetTile(0,0,tile);
        var world = new GameWorld(map);

        // Act
        var result = world.IsTileBlocked(0, 0);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsTileBlocked_ShouldReturnTrue_ForBlockingEntity()
    {
        // Arrange
        var world = TestWorldFactory.CreateWorld(TileType.Water);

        world.Entities.Add(
            new Tree(new GridPosition(3, 3)));

        // Act
        var result = world.IsTileBlocked(3, 3);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsTileBlocked_ShouldReturnTrue_ForOutOfBoundsTile()
    {
        // Arrange
        var world = TestWorldFactory.CreateWorld(TileType.Water);

        // Act
        var result = world.IsTileBlocked(-1, 0);

        // Assert
        Assert.True(result);
    }

    private static GameWorld CreateWorld()
    {
        int width = 10;
        int height = 10;
        var map = new TileMap(width, height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var tile = new Tile
                {
                    TerrainType = TileType.Water
                };
                map.SetTile(x, y, tile);
            }
        }

        return new GameWorld(map);
    }
}