namespace RTSEngine.Core.Map.Loading;
public static class TileTypeMapper
{
    public static Runtime.TileType FromSymbol(char symbol)
    {
        return symbol switch
        {
            'G' => Runtime.TileType.Grass,
            'W' => Runtime.TileType.Water,
            'M' => Runtime.TileType.Mountain,
            'S' => Runtime.TileType.Sand,
            _ => throw new Exception("Unknown tile symbol")
        };
    }

    public static char ToSymbol(Runtime.TileType type)
    {
        return type switch
        {
            Runtime.TileType.Grass => 'G',
            Runtime.TileType.Water => 'W',
            Runtime.TileType.Mountain => 'M',
            Runtime.TileType.Sand => 'S',
            _ => '?'
        };
    }

    
}
