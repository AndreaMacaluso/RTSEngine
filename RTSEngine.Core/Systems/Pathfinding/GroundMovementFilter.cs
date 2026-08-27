using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Helpers;
using RTSEngine.Core.State;

namespace RTSEngine.Core.Systems.Pathfinding;

public class GroundMovementFilter : IMovementFilter
{
    public bool CanPass(GameWorld world, GridPosition position)
    {
        return !WorldQueries.IsTileBlocked(world, position.X, position.Y);
    }
}
