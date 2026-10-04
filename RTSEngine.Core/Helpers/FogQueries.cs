using RTSEngine.Core.Entities;
using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Resources;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Map.Visibility;
using RTSEngine.Core.State;

namespace RTSEngine.Core.Helpers;

/// <summary>
/// The gate: the single entry point for every visibility question gameplay asks.
///
/// Player orders are not gated here - they go through on any tile, black
/// included. The rules these queries apply: resources on Explored or better,
/// enemy units on Visible only, enemy buildings on Visible plus the remembered.
/// </summary>
public static class FogQueries
{
    public static bool IsVisible(GameWorld world, VisionScope scope, GridPosition position)
        => TileAt(world, scope, position) == TileVisibility.Visible;

    public static bool IsSeen(GameWorld world, VisionScope scope, GridPosition position)
        => TileAt(world, scope, position) != TileVisibility.Hidden;

    // Scope comes from the viewer, so a caller holding an entity cannot build
    // one for the wrong player.
    public static bool IsVisible(GameWorld world, Unit viewer, GridPosition position)
        => IsVisible(world, world.Fog.ScopeFor(viewer.OwnerId), position);

    public static bool IsVisible(GameWorld world, Building viewer, GridPosition position)
        => IsVisible(world, world.Fog.ScopeFor(viewer.OwnerId), position);

    /// An enemy that left the view is not chased.
    public static IEnumerable<Unit> GetVisibleEnemies(GameWorld world, VisionScope scope)
    {
        foreach (var unit in world.Entities.Units)
        {
            if (unit.IsDead || scope.Includes(unit.OwnerId))
                continue;

            if (!IsVisible(world, scope, unit.Position))
                continue;

            yield return unit;
        }
    }

    /// Visible now, or seen at least once. Destroyed ones are filtered here
    /// and forgotten by VisibilitySystem.
    public static IEnumerable<Building> GetKnownEnemyBuildings(GameWorld world, VisionScope scope)
    {
        var viewer = world.GetPlayerById(scope.ViewerId);
        var remembered = viewer?.RememberedEnemyBuildingIds;

        foreach (var building in world.Entities.Buildings)
        {
            if (building.IsDead || scope.Includes(building.OwnerId))
                continue;

            if (IsVisible(world, scope, building.Position)
                || (remembered?.Contains(building.Id) ?? false))
            {
                yield return building;
            }
        }
    }

    public static Building? FindEnemyBuilding(
        GameWorld world,
        VisionScope scope,
        string buildingId)
    {
        return GetKnownEnemyBuildings(world, scope)
            .FirstOrDefault(b =>
                b.Definition.Id == buildingId
                && b.IsCompleted);
    }

    public static (Entity Entity, int OwnerId)? FindNearestEnemyEntity(
        GameWorld world,
        VisionScope scope,
        GridPosition position)
    {
        Entity? bestEntity = null;
        int bestOwnerId = 0;
        int bestDist = int.MaxValue;

        foreach (var unit in GetVisibleEnemies(world, scope))
        {
            int dist = WorldQueries.ChebyshevDistance(position, unit.Position);
            if (dist < bestDist)
            {
                bestDist = dist;
                bestEntity = unit;
                bestOwnerId = unit.OwnerId;
            }
        }

        foreach (var building in GetKnownEnemyBuildings(world, scope))
        {
            int dist = WorldQueries.ChebyshevDistance(position, building.Position);
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
        VisionScope scope)
    {
        foreach (var _ in GetVisibleEnemies(world, scope))
            return true;

        foreach (var _ in GetKnownEnemyBuildings(world, scope))
            return true;

        return false;
    }

    /// Counts on Explored, never on Hidden. Ties break on the lowest Id,
    /// so two clients pick the same resource.
    public static ResourceNode? FindClosestVisibleResource(
        GameWorld world,
        VisionScope scope,
        GridPosition center,
        ResourceType? resourceType = null)
    {
        ResourceNode? closest = null;
        int bestDist = int.MaxValue;

        foreach (var resource in world.Entities.Resources)
        {
            if (resource.IsDepleted)
                continue;

            if (resourceType.HasValue && resource.ResourceType != resourceType.Value)
                continue;

            if (!IsSeen(world, scope, resource.Position))
                continue;

            int dist = WorldQueries.DistanceSquared(center, resource.Position);
            if (dist < bestDist
                || (dist == bestDist && (closest is null || resource.Id < closest.Id)))
            {
                bestDist = dist;
                closest = resource;
            }
        }

        return closest;
    }

    private static TileVisibility TileAt(GameWorld world, VisionScope scope, GridPosition position)
    {
        if (!world.Fog.IsEnabled)
            return TileVisibility.Visible;

        return world.Fog.Get(scope.ViewerId, position);
    }
}
