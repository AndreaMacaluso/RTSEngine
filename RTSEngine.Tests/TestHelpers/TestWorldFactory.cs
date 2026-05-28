using RTSEngine.Core.Map;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.State;

namespace RTSEngine.Tests.TestHelpers;

public static class TestWorldFactory
{
    
    public static GameWorld CreateWorld( TileType tileType = TileType.Grass )
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
                    TerrainType = tileType
                };
                map.SetTile(x, y, tile);
            }
        }

        return new GameWorld(map);
    }
}