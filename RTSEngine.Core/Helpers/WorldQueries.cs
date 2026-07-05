using RTSEngine.Core.Entities.Resources;
using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.State;
using RTSEngine.Core.Systems;

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

            if (!world.IsInsideBounds(candidate.X, candidate.Y))
            {
                continue;
            }

            if (world.IsTileBlocked(candidate.X, candidate.Y))
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

            if (!world.IsInsideBounds(candidate.X, candidate.Y))
            {
                continue;
            }

            if (world.IsTileBlocked(candidate.X, candidate.Y))
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

    public static IEnumerable<ResourceNode> FindResourcesNear(
        GameWorld world,
        GridPosition center,
        int radius)
    {
        int radiusSquared = radius * radius;

        return world.Resources.Where(r =>
            !r.IsDepleted &&
            DistanceSquared(center, r.Position) <= radiusSquared);
    }

    public static Building? FindClosestDeposit(
        GameWorld world,
        GridPosition center,
        ResourceType resourceType)
    {
        return world.Entities
            .OfType<Building>()
            .Where(b =>
                b.Definition.AcceptedResources.Contains(resourceType))
            .OrderBy(b =>
                Math.Abs(b.Position.X - center.X)
                + Math.Abs(b.Position.Y - center.Y))
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
}