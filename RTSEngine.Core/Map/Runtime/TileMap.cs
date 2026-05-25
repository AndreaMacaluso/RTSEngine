namespace RTSEngine.Core.Map.Runtime;
public class TileMap
{
    public int Width { get; }

    public int Height { get; }

    private readonly Tile[,] _tiles;

    public TileMap(int width, int height)
    {
        Width = width;
        Height = height;

        _tiles = new Tile[width, height];
    }

    public Tile GetTile(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            throw new IndexOutOfRangeException($"Tile coordinates ({x}, {y}) are out of bounds.");
        } 
        return _tiles[x, y];
    }

    public void SetTile(int x, int y, Tile tile)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            throw new IndexOutOfRangeException($"Tile coordinates ({x}, {y}) are out of bounds.");
        }
        _tiles[x, y] = tile;
    }
}