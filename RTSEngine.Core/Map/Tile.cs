namespace RTSEngine.Core.Map;
public class Tile
{
    public TileType TerrainType { get; set; }

    public ResourceType? ResourceType { get; set; }

    public bool HasBuilding { get; set; }
}