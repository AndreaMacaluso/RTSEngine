using RTSEngine.Core.Entities;
using RTSEngine.Core.Entities.Resources;
using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Helpers;
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
        var result = WorldQueries.IsInsideBounds(world, 5, 5);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsInsideBounds_ShouldReturnFalse_ForNegativePosition()
    {
        // Arrange
        var world = TestWorldFactory.CreateWorld(TileType.Water);

        // Act
        var result = WorldQueries.IsInsideBounds(world, -1, 0);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsInsideBounds_ShouldReturnFalse_ForOutOfBoundsPosition()
    {
        // Arrange
        var world = TestWorldFactory.CreateWorld(TileType.Water);

        // Act
        var result = WorldQueries.IsInsideBounds(world, 10, 10);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetEntityAt_ShouldReturnEntity_WhenTileIsOccupied()
    {
        // Arrange
        var world = TestWorldFactory.CreateWorld(TileType.Water);

        var tree = new Tree(new GridPosition(2, 2));

        world.AddEntity(tree);

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

        world.AddEntity(
            new Tree(new GridPosition(1, 1)));

        // Act
        var result = WorldQueries.IsTileOccupied(world, 1, 1);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsTileOccupied_ShouldReturnFalse_WhenTileIsEmpty()
    {
        // Arrange
        var world = TestWorldFactory.CreateWorld(TileType.Water);

        // Act
        var result = WorldQueries.IsTileOccupied(world, 1, 1);

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
        var result = WorldQueries.IsTileBlocked(world, 0, 0);

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
        var result = WorldQueries.IsTileBlocked(world, 0, 0);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsTileBlocked_ShouldReturnTrue_ForBlockingEntity()
    {
        // Arrange
        var world = TestWorldFactory.CreateWorld(TileType.Water);

        world.AddEntity(
            new Tree(new GridPosition(3, 3)));

        // Act
        var result = WorldQueries.IsTileBlocked(world, 3, 3);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsTileBlocked_ShouldReturnTrue_ForOutOfBoundsTile()
    {
        // Arrange
        var world = TestWorldFactory.CreateWorld(TileType.Water);

        // Act
        var result = WorldQueries.IsTileBlocked(world, -1, 0);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsTileBlocked_ShouldReturnTrue_ForResourceNode()
    {
        var world = TestWorldFactory.CreateWorld();

        var tree = new Tree(new GridPosition(4, 4));
        world.AddResource(tree);

        Assert.True(WorldQueries.IsTileBlocked(world, 4, 4));
    }

    [Fact]
    public void IsTileBlocked_ShouldReturnFalse_ForDepletedResource()
    {
        var world = TestWorldFactory.CreateWorld();

        var tree = new Tree(new GridPosition(4, 4));
        tree.Amount = 0;
        world.AddResource(tree);

        Assert.False(WorldQueries.IsTileBlocked(world, 4, 4));
    }

    [Fact]
    public void IsTileBlocked_ShouldReturnFalse_ForAdjacentTileOfResource()
    {
        var world = TestWorldFactory.CreateWorld();

        var tree = new Tree(new GridPosition(4, 4));
        world.AddResource(tree);

        Assert.False(WorldQueries.IsTileBlocked(world, 3, 4));
        Assert.False(WorldQueries.IsTileBlocked(world, 5, 4));
        Assert.False(WorldQueries.IsTileBlocked(world, 4, 3));
        Assert.False(WorldQueries.IsTileBlocked(world, 4, 5));
    }

    [Fact]
    public void IsTileBlocked_ShouldReturnTrue_ForAllBuildingFootprintTiles()
    {
        var world = TestWorldFactory.CreateWorld();

        var definition = new BuildingDefinition
        {
            Id = "barracks",
            Name = "Barracks",
            Width = 3,
            Height = 2
        };

        var building = BuildingFactory.Create(
            definition,
            ownerId: 1,
            position: new GridPosition(2, 2));

        world.AddEntity(building);

        Assert.True(WorldQueries.IsTileBlocked(world, 2, 2));
        Assert.True(WorldQueries.IsTileBlocked(world, 3, 2));
        Assert.True(WorldQueries.IsTileBlocked(world, 4, 2));
        Assert.True(WorldQueries.IsTileBlocked(world, 2, 3));
        Assert.True(WorldQueries.IsTileBlocked(world, 3, 3));
        Assert.True(WorldQueries.IsTileBlocked(world, 4, 3));
    }

    [Fact]
    public void IsTileBlocked_ShouldReturnFalse_OutsideBuildingFootprint()
    {
        var world = TestWorldFactory.CreateWorld();

        var definition = new BuildingDefinition
        {
            Id = "barracks",
            Name = "Barracks",
            Width = 3,
            Height = 2
        };

        var building = BuildingFactory.Create(
            definition,
            ownerId: 1,
            position: new GridPosition(2, 2));

        world.AddEntity(building);

        Assert.False(WorldQueries.IsTileBlocked(world, 1, 2));
        Assert.False(WorldQueries.IsTileBlocked(world, 2, 1));
        Assert.False(WorldQueries.IsTileBlocked(world, 5, 2));
        Assert.False(WorldQueries.IsTileBlocked(world, 2, 4));
    }

    [Fact]
    public void IsResourceAt_ShouldReturnTrue_WhenResourceExists()
    {
        var world = TestWorldFactory.CreateWorld();

        var tree = new Tree(new GridPosition(3, 3));
        world.AddResource(tree);

        Assert.True(WorldQueries.IsResourceAt(world, 3, 3));
    }

    [Fact]
    public void IsResourceAt_ShouldReturnFalse_WhenNoResource()
    {
        var world = TestWorldFactory.CreateWorld();

        Assert.False(WorldQueries.IsResourceAt(world, 3, 3));
    }

    [Fact]
    public void IsBuildingAt_ShouldReturnTrue_ForAnyFootprintTile()
    {
        var world = TestWorldFactory.CreateWorld();

        var definition = new BuildingDefinition
        {
            Id = "house",
            Name = "House",
            Width = 2,
            Height = 2
        };

        var building = BuildingFactory.Create(
            definition,
            ownerId: 1,
            position: new GridPosition(5, 5));

        world.AddEntity(building);

        Assert.True(WorldQueries.IsBuildingAt(world, 5, 5));
        Assert.True(WorldQueries.IsBuildingAt(world, 6, 5));
        Assert.True(WorldQueries.IsBuildingAt(world, 5, 6));
        Assert.True(WorldQueries.IsBuildingAt(world, 6, 6));
    }

    [Fact]
    public void IsBuildingAt_ShouldReturnFalse_WhenNoBuilding()
    {
        var world = TestWorldFactory.CreateWorld();

        Assert.False(WorldQueries.IsBuildingAt(world, 5, 5));
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