using RTSEngine.Core.Map;
using RTSEngine.Core.State;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Entities;
namespace RTSEngine.DebugClient.Renders;

public static class ConsoleRenderer
{
    public static void Render(GameWorld world)
    {
        Console.WriteLine();

        for (int y = 0; y < world.Map.Height; y++)
        {
            for (int x = 0; x < world.Map.Width; x++)
            {
                var tile = world.Map.GetTile(x, y);

                string symbol = tile.TerrainType switch
                {
                    TileType.Grass => "██",
                    TileType.Water => "██",
                    TileType.Mountain => "██",
                    TileType.Sand => "██",
                    _ => "??"
                };

                var entity = world.Entities.FirstOrDefault(e => e.Position.X == x && e.Position.Y == y);
                
                if (entity != null)
                {
                    symbol = entity switch
                    {
                        Villager => "vv",
                        _ => "??"
                    };

                    //Console.ForegroundColor = ConsoleColor.Yellow;
                    //Console.Write('V');
                }

                Console.ForegroundColor = GetTileColor(tile.TerrainType);
                Console.Write(symbol);
               
            }

            Console.WriteLine();
            Console.ResetColor();
        }
    }
    

    private static ConsoleColor GetTileColor(TileType type)
    {
        return type switch
        {
            TileType.Grass => ConsoleColor.Green,
            TileType.Water => ConsoleColor.Blue,
            TileType.Forest => ConsoleColor.DarkGreen,
            TileType.Mountain => ConsoleColor.DarkGray,
            _ => ConsoleColor.White
        };
    }
}