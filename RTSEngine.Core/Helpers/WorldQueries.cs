using RTSEngine.Core.Entities;
using RTSEngine.Core.Entities.Resources;
using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Entities.Rules;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Map.Rules;
using RTSEngine.Core.State;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Players;

namespace RTSEngine.Core.Helpers;

public static class WorldQueries
{
    public static readonly GridPosition[] Directions =
    [
        new(-1, -1),
        new( 0, -1),
        new( 1, -1),

        new(-1,  0),
        new( 1,  0),

        new(-1,  1),
        new( 0,  1),
        new( 1,  1)
    ];

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
        GridPosition[] clockwiseFromBottomRight =
        [
            new( 1,  1),
            new( 1,  0),
            new( 1, -1),
            new( 0, -1),
            new(-1, -1),
            new(-1,  0),
            new(-1,  1),
            new( 0,  1)
        ];

        foreach (var direction in clockwiseFromBottomRight)
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

        foreach (var direction in Directions)
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
        return FindClosestResource(world, center, null);
    }

    public static ResourceNode? FindClosestResource(
        GameWorld world,
        GridPosition center,
        ResourceType? resourceType)
    {
        ResourceNode? closest = null;
        int bestDist = int.MaxValue;

        foreach (var r in world.Entities.Resources.Values)
        {
            if (r.IsDepleted) continue;
            if (resourceType.HasValue && r.ResourceType != resourceType.Value) continue;

            int dist = DistanceSquared(center, r.Position);
            if (dist < bestDist)
            {
                bestDist = dist;
                closest = r;
            }
        }
        return closest;
    }

    public static List<ResourceNode> FindDepletedResources(GameWorld world)
    {
        return world.Entities.Resources.Values
            .Where(r => r.IsDepleted)
            .ToList();
    }

    public static Building? FindClosestDeposit(
    GameWorld world,
    Player player,
    GridPosition center,
    ResourceType resourceType)
    {
        Building? closest = null;
        int bestDist = int.MaxValue;

        foreach (var building in world.Entities.GetBuildings(player))
        {
            if (!building.IsCompleted) continue;
            if (!building.Definition.AcceptedResources.Contains(resourceType)) continue;

            int dist = DistanceSquared(center, building.Position);
            if (dist < bestDist)
            {
                bestDist = dist;
                closest = building;
            }
        }
        return closest;
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
        string buildingId)
    {
        return world.Entities.GetBuildings(player)
            .FirstOrDefault(building =>
                building.Definition.Id == buildingId &&
                building.IsCompleted);
    }

    public static Building? FindEnemyBuilding(
        GameWorld world,
        Player player,
        string buildingId)
    {
        return world.Entities.GetEnemyBuildings(player)
            .FirstOrDefault(b =>
                b.Definition.Id == buildingId
                && b.IsCompleted);
    }

    public static bool HasBuilding(
        GameWorld world,
        Player player,
        string buildingId)
    {
        return FindBuilding(world, player, buildingId) != null;
    }

    public static int CountBuildings(
        GameWorld world,
        Player player,
        string buildingId)
    {
        return world.Entities.GetBuildings(player)
            .Count(b =>
                b.Definition.Id == buildingId &&
                b.IsCompleted);
    }

    public static List<Building> FindDeadBuildings(GameWorld world)
    {
        return world.Entities.Buildings.Values
            .Where(b => b.IsDead && b.IsCompleted)
            .ToList();
    }

    public static bool IsBuildingAt(GameWorld world, int x, int y)
    {
        return world.Entities.Spatial.IsBuildingAt(x, y);
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
        return world.Entities.Spatial.HasEntityAt(x, y);
    }

    public static bool IsResourceAt(GameWorld world, int x, int y)
    {
        return world.Entities.Spatial.GetResourceAt(x, y) is not null;
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

        var units = world.Entities.Spatial.GetUnitsAt(x, y);
        foreach (var unit in units)
        {
            if (unit.IsBlocking)
                return true;
        }

        return false;
    }

    public static (Entity Entity, int OwnerId)? FindNearestEnemyEntity(
        GameWorld world,
        Player player,
        GridPosition position)
    {
        Entity? bestEntity = null;
        int bestOwnerId = 0;
        int bestDist = int.MaxValue;

        foreach (var unit in world.Entities.GetEnemyUnits(player))
        {
            int dist = ChebyshevDistance(position, unit.Position);
            if (dist < bestDist)
            {
                bestDist = dist;
                bestEntity = unit;
                bestOwnerId = unit.OwnerId;
            }
        }

        foreach (var building in world.Entities.GetEnemyBuildings(player))
        {
            int dist = ChebyshevDistance(position, building.Position);
            if (dist < bestDist)
            {
                bestDist = dist;
                bestEntity = building;
                bestOwnerId = building.OwnerId;
            }
        }

        return bestEntity is not null
            ? (bestEntity, bestOwnerId)
            : null;
    }

    public static bool HasEnemies(
        GameWorld world,
        Player player)
    {
        return world.Entities.GetEnemyUnits(player).Any()
            || world.Entities.GetEnemyBuildings(player).Any();
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
        var center = new GridPosition(
            building.Position.X + building.Definition.Width / 2,
            building.Position.Y + building.Definition.Height / 2);

        if (building.Production.RallyPoint is GridPosition rallyPoint)
        {
            building.Production.SpawnPoint =
                FindClosestAdjacentTile(world, center, rallyPoint)
                ?? FindAdjacentWalkableTile(world, center)
                ?? new GridPosition(
                    building.Position.X + building.Definition.Width,
                    building.Position.Y);
        }
        else if (building.Production.SpawnPoint is null)
        {
            building.Production.SpawnPoint =
                FindAdjacentWalkableTile(world, center)
                ?? new GridPosition(
                    building.Position.X + building.Definition.Width,
                    building.Position.Y);
        }
    }

    public static GridPosition? FindClosestAdjacentTile(
        GameWorld world,
        GridPosition center,
        GridPosition target)
    {
        GridPosition? best = null;
        int bestDistance = int.MaxValue;

        foreach (var direction in Directions)
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

            int distance = DistanceSquared(candidate, target);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = candidate;
            }
        }

        return best;
    }
}
