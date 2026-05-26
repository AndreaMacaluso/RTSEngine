using RTSEngine.Core.Map;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Map.Loading;
using RTSEngine.Core.Map.Definitions;

using Xunit;

namespace RTSEngine.Tests.Map;
public class MapTest
{
    [Fact]
    public void TileMap_HasCorrectSize()
    {
        var map = new TileMap(40, 40);

        Assert.Equal(40, map.Width);
        Assert.Equal(40, map.Height);
    }


    [Fact]
    public void TileMap_InitializesTiles()
    {
        var map = new TileMap(10, 10);

        var newTile = new Tile
        {
            TerrainType = TileType.Grass
        };

        map.SetTile(0, 0, newTile);
        var tile = map.GetTile(0, 0);

        Assert.NotNull(tile);
    }

    [Fact]
    public void GetTile_OutOfBounds_ThrowsException()
    {
        var map = new TileMap(10, 10);

        Assert.Throws<IndexOutOfRangeException>(() =>
        {
            map.GetTile(20, 20);
        });
    }

    [Fact]
    public void Load_ShouldDeserializeMapData()
    {
        // Arrange
        var json = """
        {
            "name": "test_map",

            "width": 2,
            "height": 2,

            "rows": [
                "GG",
                "WW"
            ],

            "resources": [
                {
                    "type": "tree",
                    "x": 0,
                    "y": 1
                }
            ],

            "spawns": [
                {
                    "playerId": 1,
                    "x": 1,
                    "y": 0
                }
            ]
        }
        """;

        var path = Path.GetTempFileName();

        File.WriteAllText(path, json);

        var loader = new JsonMapLoader();

        // Act
        var mapData = loader.Load(path);

        // Assert
        Assert.Equal("test_map", mapData.Name);

        Assert.Equal(2, mapData.Width);
        Assert.Equal(2, mapData.Height);

        Assert.Equal(2, mapData.Rows.Count);

        Assert.Equal("GG", mapData.Rows[0]);
        Assert.Equal("WW", mapData.Rows[1]);

        Assert.Single(mapData.Resources);

        Assert.Equal("tree", mapData.Resources[0].Type);

        Assert.Single(mapData.Spawns);

        Assert.Equal(1, mapData.Spawns[0].PlayerId);
    }
    [Fact]
    public void Build_ShouldCreateCorrectTileMap()
    {
        // Arrange
        var data = new MapData
        {
            Name = "test",

            Width = 2,
            Height = 2,

            Rows =
            [
                "GW",
                "SM"
            ],

            Resources = [],

            Spawns = []
        };

        var builder = new TileMapBuilder();

        // Act
        var map = builder.Build(data);

        // Assert
        Assert.Equal(
            TileType.Grass,
            map.GetTile(0, 0).TerrainType);

        Assert.Equal(
            TileType.Water,
            map.GetTile(1, 0).TerrainType);

        Assert.Equal(
            TileType.Sand,
            map.GetTile(0, 1).TerrainType);

        Assert.Equal(
            TileType.Mountain,
            map.GetTile(1, 1).TerrainType);
    }

 
}