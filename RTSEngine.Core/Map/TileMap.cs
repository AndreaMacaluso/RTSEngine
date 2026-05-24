namespace RTSEngine.Core.Map;
public class TileMap
{
    public int Width { get; }

    public int Height { get; }

    private Tile[,] _tiles;

    public TileMap(int width, int height)
    {
        Width = width;
        Height = height;

        _tiles = new Tile[width, height];
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                _tiles[x, y] = new Tile
                {
                    TerrainType = TileType.Grass
                };
            }
        }
    }

    public Tile GetTile(int x, int y)
    {
        return _tiles[x, y];
    }
}