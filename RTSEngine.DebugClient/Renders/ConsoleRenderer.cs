using RTSEngine.Core.Entities.Resources;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Map;
using RTSEngine.Core.Map.Definitions;
using RTSEngine.Core.State;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Entities;

namespace RTSEngine.DebugClient.Renders;

public static class ConsoleRenderer
{
    public static void Render(
        GameWorld world,
        RenderMode mode = RenderMode.Extended)
    {
        Console.WriteLine();

        for (int y = 0; y < world.Map.Height; y++)
        {
            for (int x = 0; x < world.Map.Width; x++)
            {
                RenderTile(world, x, y, mode);
            }

            Console.WriteLine();
        }

        Console.ResetColor();
    }

    private static void RenderTile(
        GameWorld world,
        int x,
        int y,
        RenderMode mode)
    {
        var tile = world.Map.GetTile(x, y);

        var spawn = world.Spawns
            .FirstOrDefault(s => s.X == x && s.Y == y);

        if (spawn != null)
        {
            RenderSpawn(spawn);

            return;
        }

        var resource = world.Resources
            .FirstOrDefault(r =>
                r.Position.X == x &&
                r.Position.Y == y);

        if (resource != null)
        {
            RenderResource(resource);

            return;
        }

        var entity = world.Entities
            .FirstOrDefault(e =>
                e.Position.X == x &&
                e.Position.Y == y);

        if (entity != null)
        {
            RenderEntity(entity);

            return;
        }

        RenderTerrain(tile.TerrainType, mode);
    }

    private static void RenderTerrain(
        TileType type,
        RenderMode mode)
    {
        if (mode == RenderMode.Minimal)
        {
            Console.Write("  ");

            return;
        }

        Console.ForegroundColor = GetTileColor(type);

        string symbol = type switch
        {
            TileType.Grass => "██",
            TileType.Water => "██",
            TileType.Mountain => "██",
            TileType.Sand => "██",
            _ => "??"
        };

        Console.Write(symbol);
    }

    private static void RenderEntity(Entity entity)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;

        string symbol = entity switch
        {
            Villager => "vv",
            _ => "??"
        };

        Console.Write(symbol);
    }

    private static void RenderResource(
        ResourceNode resource)
    {
        Console.ForegroundColor = GetResourceColor(resource);

        Console.Write(GetResourceSymbol(resource));
        Console.Write(" ");
    }

    private static void RenderSpawn(
        SpawnPointDefinition spawn)
    {
        Console.ForegroundColor =
            spawn.PlayerId switch
            {
                1 => ConsoleColor.Blue,
                2 => ConsoleColor.Red,
                _ => ConsoleColor.White
            };

        Console.Write("⌂ ");
    }

    private static ConsoleColor GetTileColor(
        TileType type)
    {
        return type switch
        {
            TileType.Grass => ConsoleColor.Green,

            TileType.Water => ConsoleColor.Blue,

            TileType.Forest => ConsoleColor.DarkGreen,

            TileType.Mountain => ConsoleColor.DarkGray,

            TileType.Sand => ConsoleColor.Yellow,

            _ => ConsoleColor.White
        };
    }

    private static ConsoleColor GetResourceColor(
        ResourceNode resource)
    {
        return resource switch
        {
            Tree => ConsoleColor.DarkGreen,

            BerryBush => ConsoleColor.DarkRed,

            GoldMine => ConsoleColor.DarkYellow,

            StoneMine => ConsoleColor.Gray,

            _ => ConsoleColor.White
        };
    }

    private static string GetResourceSymbol(
        ResourceNode resource)
    {
        return resource switch
        {
            Tree => "♣",

            BerryBush => "●",//"♦",

            GoldMine => "■",

            StoneMine => "■",

            _ => "?"
        };
    }
}