using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.State;

namespace RTSEngine.Core.Systems.Pathfinding;

public interface IMovementFilter
{
    bool CanPass(GameWorld world, GridPosition position);
}
