namespace RTSEngine.Core.Map;
public static class TileTypeMapper
{
    public static TileType FromSymbol(char symbol)
    {
        return symbol switch
        {
            'G' => TileType.Grass,
            'W' => TileType.Water,
            'F' => TileType.Forest,
            'M' => TileType.Mountain,
            _ => throw new Exception("Unknown tile symbol")
        };
    }

    public static char ToSymbol(TileType type)
    {
        return type switch
        {
            TileType.Grass => 'G',
            TileType.Water => 'W',
            TileType.Forest => 'F',
            TileType.Mountain => 'M',
            _ => '?'
        };
    }

    
}