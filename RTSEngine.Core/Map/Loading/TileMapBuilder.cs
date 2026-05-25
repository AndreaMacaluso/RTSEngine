namespace RTSEngine.Core.Map.Loading;

public class TileMapBuilder
{
    public Runtime.TileMap Build(Definitions.MapData data)
    {
        var map = new Runtime.TileMap(data.Width, data.Height);

        for (int y = 0; y < data.Height; y++)
        {
            var row = data.Rows[y];

            for (int x = 0; x < data.Width; x++)
            {
                var symbol = row[x];

                var tile = new Runtime.Tile
                {
                    TerrainType = TileTypeMapper.FromSymbol(symbol)
                };

                map.SetTile(x, y, tile);
            }
        }

        return map;
    }
}