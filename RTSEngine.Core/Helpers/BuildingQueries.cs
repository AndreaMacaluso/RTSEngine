using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Map.Runtime;

namespace RTSEngine.Core.Helpers;

public static class BuildingQueries
{
    public static IEnumerable<GridPosition> GetOccupiedTiles(
        Building building)
    {
        return GetOccupiedTiles(
            building.Definition,
            building.Position);
    }

    public static IEnumerable<GridPosition> GetOccupiedTiles(
        BuildingDefinition definition,
        GridPosition topLeft)
    {
        for (int y = 0; y < definition.Height; y++)
        {
            for (int x = 0; x < definition.Width; x++)
            {
                yield return new GridPosition(
                    topLeft.X + x,
                    topLeft.Y + y);
            }
        }
    }

    public static IEnumerable<GridPosition> GetAdjacentTiles(
        Building building)
    {
        return GetAdjacentTiles(
            building.Definition,
            building.Position);
    }

    public static IEnumerable<GridPosition> GetAdjacentTiles(
        BuildingDefinition definition,
        GridPosition topLeft)
    {
        var occupied = GetOccupiedTiles(definition, topLeft).ToHashSet();

        for (int y = -1; y <= definition.Height; y++)
        {
            for (int x = -1; x <= definition.Width; x++)
            {
                var tile = new GridPosition(
                    topLeft.X + x,
                    topLeft.Y + y);

                if (occupied.Contains(tile))
                {
                    continue;
                }

                yield return tile;
            }
        }
    }

    public static GridPosition GetCenterTile(
        Building building)
    {
        return GetCenterTile(
            building.Definition,
            building.Position);
    }

    public static GridPosition GetCenterTile(
        BuildingDefinition definition,
        GridPosition topLeft)
    {
        return new GridPosition(
            topLeft.X + definition.Width / 2,
            topLeft.Y + definition.Height / 2);
    }

        public static bool OccupiesTile(
        Building building,
        GridPosition position)
    {
        return OccupiesTile(
            building.Definition,
            building.Position,
            position);
    }

    public static bool OccupiesTile(
        BuildingDefinition definition,
        GridPosition topLeft,
        GridPosition position)
    {
        return
            position.X >= topLeft.X &&
            position.X < topLeft.X + definition.Width &&
            position.Y >= topLeft.Y &&
            position.Y < topLeft.Y + definition.Height;
    }
}