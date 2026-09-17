using RTSEngine.Core.Entities;
using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Resources;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Helpers;
using RTSEngine.Core.Map.Runtime;

namespace RTSEngine.Core.Systems;

/// <summary>
/// Derived spatial lookup index for entities.
///
/// This is NOT a source of truth — it is rebuilt each tick from RuntimeEntities.
/// Use this for spatial queries (e.g. "what is at tile (x, y)?").
/// For iteration, use RuntimeEntities.Units/Buildings/Resources instead.
///
/// Coherence:
///   - Rebuilt via RuntimeEntities.EnsureSpatialIndex().
///   - Mark dirty via RuntimeEntities.MarkSpatialDirty() after position changes.
/// </summary>
public class SpatialIndex
{
    private readonly Dictionary<GridPosition, List<Unit>> _units = new();
    private readonly Dictionary<GridPosition, Building> _buildings = new();
    private readonly Dictionary<GridPosition, ResourceNode> _resources = new();
    private readonly HashSet<GridPosition> _blockedTiles = new();

    public void Rebuild(
        ICollection<Unit> units,
        ICollection<Building> buildings,
        ICollection<ResourceNode> resources)
    {
        _units.Clear();
        _buildings.Clear();
        _resources.Clear();
        _blockedTiles.Clear();

        foreach (var unit in units)
        {
            if (unit.IsDead) continue;
            if (!_units.TryGetValue(unit.Position, out var list))
            {
                list = [];
                _units[unit.Position] = list;
            }
            list.Add(unit);
        }

        foreach (var building in buildings)
        {
            if (building.IsDead) continue;

            if (building.IsBlocking)
            {
                foreach (var tile in BuildingQueries.GetOccupiedTiles(building))
                {
                    _blockedTiles.Add(tile);
                    _buildings[tile] = building;
                }
            }
            else
            {
                _buildings[building.Position] = building;
            }
        }

        foreach (var resource in resources)
        {
            if (!resource.IsDepleted)
                _resources[resource.Position] = resource;
        }
    }

    public IReadOnlyList<Unit> GetUnitsAt(int x, int y)
    {
        var pos = new GridPosition(x, y);
        return _units.TryGetValue(pos, out var list) ? list : [];
    }

    public Building? GetBuildingAt(int x, int y)
    {
        return _buildings.GetValueOrDefault(new GridPosition(x, y));
    }

    public ResourceNode? GetResourceAt(int x, int y)
    {
        return _resources.GetValueOrDefault(new GridPosition(x, y));
    }

    public bool HasEntityAt(int x, int y)
    {
        var pos = new GridPosition(x, y);
        return _units.ContainsKey(pos) || _buildings.ContainsKey(pos) || _resources.ContainsKey(pos);
    }

    public bool IsBuildingAt(int x, int y)
    {
        return _blockedTiles.Contains(new GridPosition(x, y));
    }
}
