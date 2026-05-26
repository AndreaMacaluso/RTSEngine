using RTSEngine.Core.Map.Runtime;
namespace RTSEngine.Core.Map.Rules;

public static class TileRules
{
    public static bool IsWalkable(Tile tile)
    {
        var type = tile.TerrainType;
        return type switch
        {
            TileType.Water => false,
            TileType.Mountain => false,
            _ => true
        };
    }

    public static bool IsBuildable(Tile tile)
    {
         var type = tile.TerrainType;
        return type switch
        {
            TileType.Water => false,
            TileType.Mountain => false,
            _ => true
        };
    }
}
