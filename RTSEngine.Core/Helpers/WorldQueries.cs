using RTSEngine.Core.Entities;
using RTSEngine.Core.Entities.Resources;
using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Entities.Rules;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Map.Rules;
using RTSEngine.Core.State;
using RTSEngine.Core.Systems;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Players;

namespace RTSEngine.Core.Helpers;

public static class WorldQueries
{
    public static bool IsAdjacent(
        GridPosition a,
        GridPosition b)
    {
        return
            Math.Abs(a.X - b.X) <= 1 &&
            Math.Abs(a.Y - b.Y) <= 1 &&
            !a.Equals(b);
    }

    public static GridPosition? FindAdjacentWalkableTile(
        GameWorld world,
        GridPosition center)
    {
        foreach (var direction in PathSystem.Directions)
        {
            var candidate = new GridPosition(
                center.X + direction.X,
                center.Y + direction.Y);

            if (!IsInsideBounds(world, candidate.X, candidate.Y))
            {
                continue;
            }

            if (IsTileBlocked(world, candidate.X, candidate.Y))
            {
                continue;
            }

            return candidate;
        }

        return null;
    }

    public static GridPosition? FindClosestAdjacentWalkableTile(
        GameWorld world,
        GridPosition start,
        GridPosition center)
    {
        GridPosition? best = null;
        int bestDistance = int.MaxValue;

        foreach (var direction in PathSystem.Directions)
        {
            var candidate = new GridPosition(
                center.X + direction.X,
                center.Y + direction.Y);

            if (!IsInsideBounds(world, candidate.X, candidate.Y))
            {
                continue;
            }

            if (IsTileBlocked(world, candidate.X, candidate.Y))
            {
                continue;
            }

            int distance = DistanceSquared(start, candidate);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = candidate;
            }
        }

        return best;
    }

    public static ResourceNode? FindClosestResource(
        GameWorld world,
        GridPosition center)
    {
        return world.Resources
            .Where(r => !r.IsDepleted)
            .OrderBy(r => DistanceSquared(center, r.Position))
            .FirstOrDefault();
    }

    public static ResourceNode? FindClosestResource(
        GameWorld world,
        GridPosition center,
        ResourceType resourceType)
    {
        return world.Resources
            .Where(r =>
                !r.IsDepleted &&
                r.ResourceType == resourceType)
            .OrderBy(r => DistanceSquared(center, r.Position))
            .FirstOrDefault();
    }

    public static List<ResourceNode> FindDepletedResources(GameWorld world)
    {
        return world.Resources
            .Where(r => r.IsDepleted)
            .ToList();
    }

    public static Building? FindClosestDeposit(
    GameWorld world,
    int ownerId,
    GridPosition center,
    ResourceType resourceType)
    {
        return world.Entities
            .OfType<Building>()
            .Where(b =>
                b.OwnerId == ownerId &&
                b.IsCompleted &&
                b.Definition.AcceptedResources.Contains(resourceType))
            .OrderBy(b => DistanceSquared(center, b.Position))
            .FirstOrDefault();
    }

    public static int DistanceSquared(
        GridPosition a,
        GridPosition b)
    {
        int dx = a.X - b.X;
        int dy = a.Y - b.Y;

        return dx * dx + dy * dy;
    }

    public static int ChebyshevDistance(
        GridPosition a,
        GridPosition b)
    {
        return Math.Max(
            Math.Abs(a.X - b.X),
            Math.Abs(a.Y - b.Y));
    }

    public static bool HasReachedDestination(
    Unit unit,
    GridPosition destination)
    {
        return
            unit.Movement.CurrentStep == null &&
            unit.Movement.PathQueue.Count == 0 &&
            WorldQueries.IsAdjacent(
                unit.Position,
                destination);
    }
      
    public static Building? FindBuilding(
        GameWorld world,
        Player player,
        string building_str)
    {
        var building_found = world.Entities
            .OfType<Building>()
            .FirstOrDefault(building =>
                building.OwnerId == player.Id &&
                building.Definition.Id == building_str &&
                building.IsCompleted);

        return building_found;
    }

    public static Building? FindEnemyBuilding(
        GameWorld world,
        Player player,
        string buildingId)
    {
        return world.Entities
            .OfType<Building>()
            .FirstOrDefault(b =>
                b.OwnerId != player.Id
                && !b.IsDead
                && b.Definition.Id == buildingId
                && b.IsCompleted);
    }

    public static bool HasBuilding(
        GameWorld world,
        Player player,
        string building_str)
    {
        return FindBuilding(world, player, building_str) != null;
    }

    public static int CountBuildings(
        GameWorld world,
        Player player,
        string buildingId)
    {
        return world.Entities
            .OfType<Building>()
            .Count(b =>
                b.OwnerId == player.Id &&
                b.Definition.Id == buildingId &&
                b.IsCompleted);
    }

    public static List<Building> FindDeadBuildings(GameWorld world)
    {
        return world.Entities
            .OfType<Building>()
            .Where(b => b.IsDead)
            .ToList();
    }

    public static bool IsBuildingAt(GameWorld world, int x, int y)
    {
        var pos = new GridPosition(x, y);

        return world.Entities
            .OfType<Building>()
            .Any(b => b.IsBlocking && BuildingQueries.OccupiesTile(
                b.Definition,
                b.Position,
                pos));
    }

    public static Entity? GetEntityAt(GameWorld world, int x, int y)
    {
        return world.Entities.FirstOrDefault(
            e => e.Position.X == x
            && e.Position.Y == y);
    }

    public static bool IsInsideBounds(GameWorld world, int x, int y)
    {
        return x >= 0
            && y >= 0
            && x < world.Map.Width
            && y < world.Map.Height;
    }

    public static bool IsTileOccupied(GameWorld world, int x, int y)
    {
        return GetEntityAt(world, x, y) != null;
    }

    public static bool IsResourceAt(GameWorld world, int x, int y)
    {
        return world.Resources.Any(
            r => r.Position.X == x
            && r.Position.Y == y
            && !r.IsDepleted);
    }

    public static bool IsTileBlocked(GameWorld world, int x, int y)
    {
        if (!IsInsideBounds(world, x, y))
        {
            return true;
        }

        var tile = world.Map.GetTile(x, y);

        if (!TileRules.IsWalkable(tile))
        {
            return true;
        }

        if (IsResourceAt(world, x, y))
        {
            return true;
        }

        if (IsBuildingAt(world, x, y))
        {
            return true;
        }

        var entity = GetEntityAt(world, x, y);

        return entity?.IsBlocking ?? false;
    }

    public static (Entity Entity, int OwnerId)? FindNearestEnemyEntity(
        GameWorld world,
        Player player,
        GridPosition position)
    {
        var units = world.Entities
            .OfType<Unit>()
            .Where(u => u.OwnerId != player.Id && !u.IsDead)
            .Select(u => (Entity: (Entity)u, OwnerId: u.OwnerId));

        var buildings = world.Entities
            .OfType<Building>()
            .Where(b => b.OwnerId != player.Id && !b.IsDead)
            .Select(b => (Entity: (Entity)b, OwnerId: b.OwnerId));

        return units.Concat(buildings)
            .OrderBy(e => ChebyshevDistance(position, e.Entity.Position))
            .FirstOrDefault();
    }

    public static bool HasEnemies(
        GameWorld world,
        Player player)
    {
        var hasEnemyUnits = world.Entities
            .OfType<Unit>()
            .Any(u => u.OwnerId != player.Id && !u.IsDead);

        var hasEnemyBuildings = world.Entities
            .OfType<Building>()
            .Any(b => b.OwnerId != player.Id && !b.IsDead);

        return hasEnemyUnits || hasEnemyBuildings;
    }

    public static GridPosition? FindBuildPosition(
        GameWorld world,
        Player player,
        BuildingDefinition definition)
    {
        var townCenter = FindBuilding(world, player, EntityIds.TownCenter);

        if (townCenter == null)
        {
            return null;
        }

        return BuildingPlacementRules.FindFreePosition(
            world,
            definition,
            townCenter.Position);
    }

    public static void EnsureSpawnPoint(
        GameWorld world,
        Building building)
    {
        if (building.Production.SpawnPoint is not null)
        {
            return;
        }

        var center = new GridPosition(
            building.Position.X + building.Definition.Width / 2,
            building.Position.Y + building.Definition.Height / 2);

        building.Production.SpawnPoint =
            FindAdjacentWalkableTile(world, center)
            ?? new GridPosition(
                building.Position.X + building.Definition.Width,
                building.Position.Y);
    }
}
