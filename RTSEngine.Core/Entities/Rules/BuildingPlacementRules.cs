
using RTSEngine.Core.Helpers;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.State;
using RTSEngine.Core.Map.Rules;

namespace RTSEngine.Core.Entities.Rules;

public static class BuildingPlacementRules
{
    public static bool CanPlace(
        GameWorld world,
        BuildingDefinition definition,
        GridPosition topLeft)
    {
        foreach (GridPosition tile in
            BuildingQueries.GetOccupiedTiles(
                definition,
                topLeft))
        {
            if (!WorldQueries.IsInsideBounds(world, tile.X, tile.Y))
            {
                return false;
            }

            var runtimeTile =
                world.Map.GetTile(tile.X, tile.Y);

            if (!TileRules.IsBuildable(runtimeTile))
            {
                return false;
            }

            if (WorldQueries.IsTileOccupied(world, tile.X, tile.Y))
            {
                return false;
            }
        }

        return true;
    }

    public static GridPosition? FindFreePosition(
        GameWorld world,
        BuildingDefinition definition,
        GridPosition center)
    {
        for (int radius = 0; radius < 20; radius++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    var position = new GridPosition(
                        center.X + x,
                        center.Y + y);

                    if (CanPlace(
                        world,
                        definition,
                        position))
                    {
                        return position;
                    }
                }
            }
        }

        return null;
    }

}