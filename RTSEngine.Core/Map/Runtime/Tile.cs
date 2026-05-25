namespace RTSEngine.Core.Map.Runtime;
public class Tile
{
    public TileType TerrainType { get; set; }

    public ResourceType? ResourceType { get; set; }

    public bool HasBuilding { get; set; }
}