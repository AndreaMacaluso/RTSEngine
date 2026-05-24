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
    }
}