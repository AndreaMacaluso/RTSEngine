using RTSEngine.Core.Helpers;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.State;

namespace RTSEngine.Core.Systems.Pathfinding;

public class AStarPathFinder : IPathFinder
{
    private const float Sqrt2 = 1.41421356f;

    private static readonly (int X, int Y, float Cost)[] Directions =
    [
        (-1, -1, Sqrt2),
        ( 0, -1, 1.0f),
        ( 1, -1, Sqrt2),

        (-1,  0, 1.0f),
        ( 1,  0, 1.0f),

        (-1,  1, Sqrt2),
        ( 0,  1, 1.0f),
        ( 1,  1, Sqrt2)
    ];

    private readonly IMovementFilter _filter;

    private readonly PriorityQueue<GridPosition, float> _open = new();
    private readonly HashSet<GridPosition> _closed = new();
    private readonly Dictionary<GridPosition, float> _gScore = new();
    private readonly Dictionary<GridPosition, GridPosition> _cameFrom = new();
    private readonly List<GridPosition> _pathBuffer = new();

    public AStarPathFinder(IMovementFilter filter)
    {
        _filter = filter;
    }

    public Queue<GridPosition> FindPath(
        GameWorld world,
        GridPosition start,
        GridPosition target)
    {
        _open.Clear();
        _closed.Clear();
        _gScore.Clear();
        _cameFrom.Clear();

        if (!_filter.CanPass(world, target))
        {
            var nearestWalkable = WorldQueries.FindAdjacentWalkableTile(world, target);
            if (nearestWalkable.HasValue)
            {
                return FindPath(world, start, nearestWalkable.Value);
            }
            return [];
        }

        _gScore[start] = 0.0f;
        _open.Enqueue(start, OctileDistance(start, target));

        while (_open.Count > 0)
        {
            var current = _open.Dequeue();

            if (current.Equals(target))
            {
                return ReconstructPath(start, target);
            }

            _closed.Add(current);

            var currentG = _gScore[current];

            foreach (var (dx, dy, cost) in Directions)
            {
                var neighbor = new GridPosition(
                    current.X + dx,
                    current.Y + dy);

                if (_closed.Contains(neighbor))
                {
                    continue;
                }

                if (!_filter.CanPass(world, neighbor))
                {
                    continue;
                }

                var tentativeG = currentG + cost;

                if (_gScore.TryGetValue(neighbor, out var existingG)
                    && tentativeG >= existingG)
                {
                    continue;
                }

                _cameFrom[neighbor] = current;
                _gScore[neighbor] = tentativeG;

                var f = tentativeG
                    + OctileDistance(neighbor, target);

                _open.Enqueue(neighbor, f);
            }
        }

        return [];
    }

    private Queue<GridPosition> ReconstructPath(
        GridPosition start,
        GridPosition target)
    {
        _pathBuffer.Clear();

        var current = target;

        while (!current.Equals(start))
        {
            _pathBuffer.Add(current);
            current = _cameFrom[current];
        }

        _pathBuffer.Reverse();

        return new Queue<GridPosition>(_pathBuffer);
    }

    private static float OctileDistance(
        GridPosition a,
        GridPosition b)
    {
        var dx = Math.Abs(a.X - b.X);
        var dy = Math.Abs(a.Y - b.Y);

        return Math.Max(dx, dy)
            + (Sqrt2 - 1.0f) * Math.Min(dx, dy);
    }
}
