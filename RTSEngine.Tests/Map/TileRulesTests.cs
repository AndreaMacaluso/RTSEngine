using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Map.Rules;

namespace RTSEngine.Tests.Map;

public class TileRulesTest
{
    [Theory]
    [InlineData(TileType.Grass, true)]
    [InlineData(TileType.Water, false)]
    [InlineData(TileType.Mountain, false)]
    public void IsWalkable(TileType terrainType, bool expected)
    {
        var tile = new Tile { TerrainType = terrainType };

        var result = TileRules.IsWalkable(tile);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(TileType.Grass, true)]
    [InlineData(TileType.Water, false)]
    [InlineData(TileType.Mountain, false)]
    public void IsBuildable(TileType terrainType, bool expected)
    {
        var tile = new Tile { TerrainType = terrainType };

        var result = TileRules.IsBuildable(tile);

        Assert.Equal(expected, result);
    }
}
