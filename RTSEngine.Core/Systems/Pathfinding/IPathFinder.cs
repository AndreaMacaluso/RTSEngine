using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.State;

namespace RTSEngine.Core.Systems.Pathfinding;

public interface IPathFinder
{
    Queue<GridPosition> FindPath(
        GameWorld world,
        GridPosition start,
        GridPosition target);
}
