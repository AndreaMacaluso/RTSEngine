
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
            if (!world.IsInsideBounds(tile.X, tile.Y))
            {
                return false;
            }

            var runtimeTile =
                world.Map.GetTile(tile.X, tile.Y);

            if (!TileRules.IsBuildable(runtimeTile))
            {
                return false;
            }

            if (world.IsTileOccupied(tile.X, tile.Y))
            {
                return false;
            }
        }

        return true;
    }
}