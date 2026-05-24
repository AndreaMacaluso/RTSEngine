using RTSEngine.Core.Map;
using RTSEngine.Core.State;

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
                    TileType.Grass => "GG",
                    TileType.Water => "~~",
                    _ => "??"
                };

                Console.Write(symbol);
            }

            Console.WriteLine();
        }
    }
}