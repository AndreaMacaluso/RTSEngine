using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Map.Rules;
using RTSEngine.Core.Helpers;
using RTSEngine.Core.State;

namespace RTSEngine.Core.Systems.Pathfinding;

public class GroundMovementFilter : IMovementFilter
{
    public bool CanPass(GameWorld world, GridPosition position)
    {
        if (!WorldQueries.IsInsideBounds(world, position.X, position.Y))
        {
            return false;
        }

        var tile = world.Map.GetTile(position.X, position.Y);

        if (!TileRules.IsWalkable(tile))
        {
            return false;
        }

        if (WorldQueries.IsResourceAt(world, position.X, position.Y))
        {
            return false;
        }

        if (WorldQueries.IsBuildingAt(world, position.X, position.Y))
        {
            return false;
        }

        var unit = world.Entities.Spatial.GetUnitAt(
            position.X, position.Y);

        if (unit?.IsBlocking ?? false)
        {
            return false;
        }

        return true;
    }
}
