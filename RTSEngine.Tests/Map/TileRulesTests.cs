using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Map.Rules;

namespace RTSEngine.Tests.Map;

public class TileRulesTest
{
    [Fact]
    public void Grass_ShouldBeWalkable()
    {
        // Arrange
        var tile = new Tile { TerrainType = TileType.Grass };

        // Act
        var result = TileRules.IsWalkable(tile);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Water_ShouldNotBeWalkable()
    {
        // Arrange
        var tile = new Tile { TerrainType = TileType.Water };

        // Act
        var result = TileRules.IsWalkable(tile);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Mountain_ShouldNotBeWalkable()
    {
        // Arrange
        var tile = new Tile { TerrainType = TileType.Mountain };

        // Act
        var result = TileRules.IsWalkable(tile);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Grass_ShouldBeBuildable()
    {
        // Arrange
        var tile = new Tile { TerrainType = TileType.Grass };

        // Act
        var result = TileRules.IsBuildable(tile);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Water_ShouldNotBeBuildable()
    {
        // Arrange
        var tile = new Tile { TerrainType = TileType.Water };

        // Act
        var result = TileRules.IsBuildable(tile);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Mountain_ShouldNotBeBuildable()
    {
        // Arrange
        var tile = new Tile { TerrainType = TileType.Mountain };

        // Act
        var result = TileRules.IsBuildable(tile);

        // Assert
        Assert.False(result);
    }
}