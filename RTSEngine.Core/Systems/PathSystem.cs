using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.State;

namespace RTSEngine.Core.Systems;

public static class PathSystem
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

    public static Queue<GridPosition> GeneratePath(
        GameWorld world,
        GridPosition start,
        GridPosition target)
    {
        var frontier = new Queue<GridPosition>();

        var visited = new HashSet<GridPosition>();

        var cameFrom =
            new Dictionary<GridPosition, GridPosition>();

        frontier.Enqueue(start);
        visited.Add(start);

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();

            if (current.Equals(target))
            {
                return ReconstructPath(
                    start,
                    target,
                    cameFrom);
            }

            foreach (var direction in Directions)
            {
                var neighbor = new GridPosition(
                    current.X + direction.X,
                    current.Y + direction.Y);

                if (!world.IsInsideBounds(
                        neighbor.X,
                        neighbor.Y))
                {
                    continue;
                }

                if (world.IsTileBlocked(
                        neighbor.X,
                        neighbor.Y))
                {
                    continue;
                }

                if (visited.Contains(neighbor))
                {
                    continue;
                }

                visited.Add(neighbor);

                cameFrom[neighbor] = current;

                frontier.Enqueue(neighbor);
            }
        }

        return [];
    }

    private static Queue<GridPosition> ReconstructPath(
        GridPosition start,
        GridPosition target,
        Dictionary<GridPosition, GridPosition> cameFrom)
    {
        var path = new Stack<GridPosition>();

        var current = target;

        while (!current.Equals(start))
        {
            path.Push(current);

            current = cameFrom[current];
        }

        return new Queue<GridPosition>(path);
    }
}