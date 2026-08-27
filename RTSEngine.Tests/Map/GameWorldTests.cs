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

public class GameWorldTests
{
    [Fact]
    public void IsInsideBounds()
    {
        var world = TestWorldFactory.CreateWorld(TileType.Water);

        Assert.True(WorldQueries.IsInsideBounds(world, 5, 5));
        Assert.False(WorldQueries.IsInsideBounds(world, -1, 0));
        Assert.False(WorldQueries.IsInsideBounds(world, 10, 10));
    }

    [Fact]
    public void Units_ShouldContainEntity_WhenTileIsOccupied()
    {
        // Arrange
        var world = TestWorldFactory.CreateWorld(TileType.Water);

        var tree = new Tree(new GridPosition(2, 2));

        world.Entities.Add(tree);

        // Act
        var resource = world.Entities.Resources.Values.FirstOrDefault(
            r => r.Position.X == 2 && r.Position.Y == 2);

        // Assert
        Assert.NotNull(resource);
        Assert.Equal(tree, resource);
    }

    [Fact]
    public void Units_ShouldBeEmpty_WhenTileIsEmpty()
    {
        // Arrange
        var world = TestWorldFactory.CreateWorld(TileType.Water);

        // Act & Assert
        Assert.Empty(world.Entities.Units.Values);
    }

    [Fact]
    public void IsTileOccupied()
    {
        var world = TestWorldFactory.CreateWorld(TileType.Water);

        world.Entities.Add(new Tree(new GridPosition(1, 1)));

        Assert.True(WorldQueries.IsTileOccupied(world, 1, 1));
        Assert.False(WorldQueries.IsTileOccupied(world, 2, 2));
    }

    [Theory]
    [InlineData(TileType.Water)]
    [InlineData(TileType.Mountain)]
    public void IsTileBlocked_Terrain(TileType terrainType)
    {
        var map = new TileMap(2, 2);
        var tile = new Tile { TerrainType = terrainType };
        map.SetTile(0, 0, tile);
        var world = new GameWorld(map);

        Assert.True(WorldQueries.IsTileBlocked(world, 0, 0));
    }

    [Fact]
    public void IsTileBlocked_Entity()
    {
        var world = TestWorldFactory.CreateWorld(TileType.Water);

        world.Entities.Add(new Tree(new GridPosition(3, 3)));
        Assert.True(WorldQueries.IsTileBlocked(world, 3, 3));

        Assert.True(WorldQueries.IsTileBlocked(world, -1, 0));

        var world2 = TestWorldFactory.CreateWorld();
        var tree = new Tree(new GridPosition(4, 4));
        world2.Entities.Add(tree);
        Assert.True(WorldQueries.IsTileBlocked(world2, 4, 4));

        var world2b = TestWorldFactory.CreateWorld();
        var tree2 = new Tree(new GridPosition(4, 4));
        tree2.Amount = 0;
        world2b.Entities.Add(tree2);
        Assert.False(WorldQueries.IsTileBlocked(world2b, 4, 4));

        var world2c = TestWorldFactory.CreateWorld();
        var tree3 = new Tree(new GridPosition(4, 4));
        world2c.Entities.Add(tree3);
        Assert.False(WorldQueries.IsTileBlocked(world2c, 3, 4));
        Assert.False(WorldQueries.IsTileBlocked(world2c, 5, 4));
        Assert.False(WorldQueries.IsTileBlocked(world2c, 4, 3));
        Assert.False(WorldQueries.IsTileBlocked(world2c, 4, 5));

        var world3 = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player = world3.GetPlayerById(1)!;
        var definition = new BuildingDefinition
        {
            Id = "barracks",
            Name = "Barracks",
            Width = 3,
            Height = 2,
            MaxHealth = 100
        };
        var building = BuildingFactory.Create(
            definition,
            ownerId: 1,
            position: new GridPosition(2, 2));
        building.Health.CurrentHealth = definition.MaxHealth;
        world3.Entities.Add(building, player);

        Assert.True(WorldQueries.IsTileBlocked(world3, 2, 2));
        Assert.True(WorldQueries.IsTileBlocked(world3, 3, 2));
        Assert.True(WorldQueries.IsTileBlocked(world3, 4, 2));
        Assert.True(WorldQueries.IsTileBlocked(world3, 2, 3));
        Assert.True(WorldQueries.IsTileBlocked(world3, 3, 3));
        Assert.True(WorldQueries.IsTileBlocked(world3, 4, 3));

        var world4 = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player4 = world4.GetPlayerById(1)!;
        var def4 = new BuildingDefinition
        {
            Id = "barracks",
            Name = "Barracks",
            Width = 3,
            Height = 2
        };
        var building4 = BuildingFactory.Create(
            def4,
            ownerId: 1,
            position: new GridPosition(2, 2));
        world4.Entities.Add(building4, player4);

        Assert.False(WorldQueries.IsTileBlocked(world4, 1, 2));
        Assert.False(WorldQueries.IsTileBlocked(world4, 2, 1));
        Assert.False(WorldQueries.IsTileBlocked(world4, 5, 2));
        Assert.False(WorldQueries.IsTileBlocked(world4, 2, 4));
    }

    [Fact]
    public void IsResourceAt()
    {
        var world = TestWorldFactory.CreateWorld();

        Assert.False(WorldQueries.IsResourceAt(world, 3, 3));

        var tree = new Tree(new GridPosition(3, 3));
        world.Entities.Add(tree);

        Assert.True(WorldQueries.IsResourceAt(world, 3, 3));
    }

    [Fact]
    public void IsBuildingAt()
    {
        var world = TestWorldFactory.CreateWorld();

        Assert.False(WorldQueries.IsBuildingAt(world, 5, 5));

        var world2 = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player = world2.GetPlayerById(1)!;
        var definition = new BuildingDefinition
        {
            Id = "house",
            Name = "House",
            Width = 2,
            Height = 2,
            MaxHealth = 100
        };
        var building = BuildingFactory.Create(
            definition,
            ownerId: 1,
            position: new GridPosition(5, 5));
        building.Health.CurrentHealth = definition.MaxHealth;
        world2.Entities.Add(building, player);

        Assert.True(WorldQueries.IsBuildingAt(world2, 5, 5));
        Assert.True(WorldQueries.IsBuildingAt(world2, 6, 5));
        Assert.True(WorldQueries.IsBuildingAt(world2, 5, 6));
        Assert.True(WorldQueries.IsBuildingAt(world2, 6, 6));
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
