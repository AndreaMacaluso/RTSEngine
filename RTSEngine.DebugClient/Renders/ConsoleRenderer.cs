using RTSEngine.Core.Entities;
using RTSEngine.Core.Entities.Resources;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Map;
using RTSEngine.Core.Map.Definitions;
using RTSEngine.Core.State;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Helpers;

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
        RenderPlayerStats(world); 
        Console.ResetColor();
    }

    private static void RenderTile(
        GameWorld world,
        int x,
        int y,
        RenderMode mode)
    {
        var tile = world.Map.GetTile(x, y);

        var position = new GridPosition(x, y);

        var unit = world.Entities.Units.Values
            .FirstOrDefault(entity => entity.Position == position);

        if (unit != null)
        {
            RenderUnit(unit);

            return;
        }

        var building = world.Entities.Buildings.Values
            .FirstOrDefault(entity => BuildingQueries.OccupiesTile(entity, position));

        if (building != null)
        {
            RenderBuilding(building, position);

            return;
        }

        var resource = world.Entities.Resources.Values
            .FirstOrDefault(r =>
                r.Position.X == x &&
                r.Position.Y == y);

        if (resource != null)
        {
            RenderResource(resource);

            return;
        }

        var spawn = world.Spawns
            .FirstOrDefault(s => s.X == x && s.Y == y);

        if (spawn != null)
        {
            RenderSpawn(spawn);

            return;
        }

        var projectile = world.Projectiles.All
            .FirstOrDefault(p =>
                p.IsActive &&
                p.X.Raw / 1000 == x &&
                p.Y.Raw / 1000 == y);

        if (projectile != null)
        {
            RenderProjectile(projectile);

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

    private static void RenderUnit(Unit unit)
    {
        if (unit.IsDead)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("x ");
            return;
        }

        Console.ForegroundColor = GetOwnerColor(unit.OwnerId);

        string symbol = unit.Definition.Category switch
        {
            EntityCategory.Villager => "● ",
            EntityCategory.Infantry => "▲ ",
            EntityCategory.Ranged => "* ",
            EntityCategory.Cavalry => "♦ ",
            EntityCategory.Siege => "⊕ ",
            _ => "? "
        };

        Console.Write(symbol);
    }

    private static void RenderProjectile(Projectile projectile)
    {
        Console.ForegroundColor = projectile.IsSingleTarget
            ? ConsoleColor.Yellow
            : ConsoleColor.Red;

        Console.Write("· ");
    }

    private static void RenderBuilding(
        Building building,
        GridPosition position)
    {
        Console.ForegroundColor = GetOwnerColor(building.OwnerId);

        var symbol = building.IsCompleted
            ? GetBuildingSymbol(building, position)
            : "░░";

        Console.Write(symbol);
    }

    private static string GetBuildingSymbol(
        Building building,
        GridPosition position)
    {
        if (position != building.Position)
        {
            return "▓▓";
        }

        return building.Definition.Id switch
        {
            EntityIds.TownCenter => "TC",
            EntityIds.House => "H ",
            EntityIds.LumberCamp => "LC",
            EntityIds.MineCamp => "MC",
            EntityIds.Mill => "ML",
            EntityIds.Barracks => "BR",
            EntityIds.ArcheryRange => "AR",
            EntityIds.Stable => "SB",
            EntityIds.SiegeWorkshop => "SW",
            EntityIds.WatchTower => "WT",
            _ => "B "
        };
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

    private static ConsoleColor GetOwnerColor(int ownerId)
    {
        return ownerId switch
        {
            1 => ConsoleColor.Blue,
            2 => ConsoleColor.Red,
            _ => ConsoleColor.White
        };
    }

    private static ConsoleColor GetTileColor(
        TileType type)
    {
        return type switch
        {
            TileType.Grass => ConsoleColor.Green,

            TileType.Water => ConsoleColor.Blue,

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

            BerryBush => "♣",//"♦",

            GoldMine => "■",

            StoneMine => "■",

            _ => "?"
        };
    }

    public static void RenderPlayerStats(GameWorld world)
    {
        Console.WriteLine();
        Console.WriteLine("=== PLAYERS ===                                                                                           ");

        foreach (var player in world.Players)
        {
            Console.ForegroundColor = GetOwnerColor(player.Id);

            var units = world.Entities.Units.Values
                .Where(u => u.OwnerId == player.Id && !u.IsDead)
                .ToList();

            int economic = units.Count(u => !u.Definition.CanAttack);
            int military = units.Count(u => u.Definition.CanAttack);
            int alive = units.Count;
            int dead = world.Entities.Units.Values
                .Count(u => u.OwnerId == player.Id && u.IsDead);
            int idle = units.Count(u => u.CurrentTask == EntityState.Idle);

            Console.WriteLine(
                $"P{player.Id} {player.Name} | " +
                $"Pop {player.Population.Current}/{player.Population.Capacity} | " +
                $"U {alive} D {dead} | " +
                $"I {idle} | " +
                $"W {player.Economy.Get(ResourceType.Wood),-4} " +
                $"F {player.Economy.Get(ResourceType.Food),-4} " +
                $"G {player.Economy.Get(ResourceType.Gold),-4} " +
                $"S {player.Economy.Get(ResourceType.Stone),-4} " +
                $"| {player.Score}pt                                                                                           ");
        }

        Console.ResetColor();
    }
}
