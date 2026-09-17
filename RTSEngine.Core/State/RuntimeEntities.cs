using RTSEngine.Core.Entities;
using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Resources;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Helpers;
using RTSEngine.Core.Players;
using RTSEngine.Core.Systems;

namespace RTSEngine.Core.State;

/// <summary>
/// Source of truth for all runtime entities (units, buildings, resources).
///
/// Data flow:
///   RuntimeEntities (source) → SpatialIndex (derived spatial lookup)
///
/// RuntimeEntities owns the canonical lists sorted by entity Id.
/// SpatialIndex is rebuilt from these lists each tick (or on demand via EnsureSpatialIndex).
/// Systems iterate via Units/Buildings/Resources (deterministic order).
/// Spatial queries go through Spatial (e.g. Spatial.GetBuildingAt(x, y)).
///
/// Coherence rules:
///   - All entity Add/Remove MUST go through this class.
///   - After Add/Remove, call MarkSpatialDirty() (done automatically in Add/Remove).
///   - MovementSystem must call MarkSpatialDirty() after moving entities.
///   - SimulationRunner calls EnsureSpatialIndex(forceRebuild: true) at tick start.
/// </summary>
public class RuntimeEntities
{
    // Canonical entity lists, sorted by Id for deterministic iteration.
    private readonly List<Unit> _units = new();
    private readonly List<Building> _buildings = new();
    private readonly List<ResourceNode> _resources = new();

    // External references — shared with GameWorld, not owned by this class.
    private readonly List<Player> _players;

    // Derived spatial index — rebuilt from _units/_buildings/_resources.
    private readonly SpatialIndex _spatial = new();

    private readonly SequentialIdGenerator _idGenerator;
    private bool _spatialDirty = true;

    // Read-only access for deterministic iteration (sorted by Id).
    public IReadOnlyList<Unit> Units => _units;
    public IReadOnlyList<Building> Buildings => _buildings;
    public IReadOnlyList<ResourceNode> Resources => _resources;

    // Derived spatial index — lazy rebuild on access.
    public SpatialIndex Spatial => EnsureSpatialIndex();

    public RuntimeEntities(List<Player> players)
    {
        _players = players;
        _idGenerator = new SequentialIdGenerator();
    }

    /// <summary>
    /// Returns the spatial index, rebuilding it if dirty.
    /// Call with forceRebuild: true at tick start to guarantee freshness.
    /// </summary>
    public SpatialIndex EnsureSpatialIndex(bool forceRebuild = false)
    {
        if (forceRebuild || _spatialDirty)
        {
            _spatial.Rebuild(_units, _buildings, _resources);
            _spatialDirty = false;
        }
        return _spatial;
    }

    public int GenerateEntityId()
    {
        return _idGenerator.Next();
    }

    public void MarkSpatialDirty()
    {
        _spatialDirty = true;
    }

    public void Add(Unit unit, Player player)
    {
        unit.Id = GenerateEntityId();
        InsertSorted(_units, unit);
        player.AddUnit(unit.Id);
        _spatialDirty = true;
    }

    public void Add(Building building, Player player)
    {
        building.Id = GenerateEntityId();
        InsertSorted(_buildings, building);
        player.AddBuilding(building.Id);
        _spatialDirty = true;
    }

    public void Add(ResourceNode resource)
    {
        resource.Id = GenerateEntityId();
        InsertSorted(_resources, resource);
        _spatialDirty = true;
    }

    public void Remove(Unit unit, Player player)
    {
        int index = BinarySearchById(_units, unit.Id);
        if (index >= 0) _units.RemoveAt(index);
        player.RemoveUnit(unit.Id);
        _spatialDirty = true;
    }

    public void Remove(Building building, Player player)
    {
        int index = BinarySearchById(_buildings, building.Id);
        if (index >= 0) _buildings.RemoveAt(index);
        player.RemoveBuilding(building.Id);
        _spatialDirty = true;
    }

    public void Remove(ResourceNode resource)
    {
        int index = BinarySearchById(_resources, resource.Id);
        if (index >= 0) _resources.RemoveAt(index);
        _spatialDirty = true;
    }

    public IEnumerable<Unit> GetUnits(Player player)
    {
        foreach (var id in player.UnitIds)
        {
            var unit = FindById(_units, id);
            if (unit != null)
                yield return unit;
        }
    }

    public IEnumerable<Building> GetBuildings(Player player)
    {
        foreach (var id in player.BuildingIds)
        {
            var building = FindById(_buildings, id);
            if (building != null)
                yield return building;
        }
    }

    public IEnumerable<Unit> GetEnemyUnits(Player player)
    {
        foreach (var p in _players)
        {
            if (p.Id == player.Id)
                continue;

            foreach (var id in p.UnitIds)
            {
                var unit = FindById(_units, id);
                if (unit != null && !unit.IsDead)
                    yield return unit;
            }
        }
    }

    public IEnumerable<Building> GetEnemyBuildings(Player player)
    {
        foreach (var p in _players)
        {
            if (p.Id == player.Id)
                continue;

            foreach (var id in p.BuildingIds)
            {
                var building = FindById(_buildings, id);
                if (building != null && !building.IsDead)
                    yield return building;
            }
        }
    }

    public Unit? GetUnitById(int id) => FindById(_units, id);

    public Building? GetBuildingById(int id) => FindById(_buildings, id);

    public ResourceNode? GetResourceById(int id) => FindById(_resources, id);

    public Entity? GetEntityById(int id)
    {
        var unit = FindById(_units, id);
        if (unit != null) return unit;

        var building = FindById(_buildings, id);
        if (building != null) return building;

        return FindById(_resources, id);
    }

    private static void InsertSorted<T>(List<T> list, T item) where T : Entity
    {
        int index = BinarySearchById(list, item.Id);
        if (index < 0) index = ~index;
        list.Insert(index, item);
    }

    private static T? FindById<T>(List<T> list, int id) where T : Entity
    {
        int index = BinarySearchById(list, id);
        return index >= 0 ? list[index] : null;
    }

    private static int BinarySearchById<T>(List<T> list, int id) where T : Entity
    {
        int lo = 0, hi = list.Count - 1;
        while (lo <= hi)
        {
            int mid = lo + ((hi - lo) >> 1);
            int cmp = list[mid].Id.CompareTo(id);
            if (cmp == 0) return mid;
            if (cmp < 0) lo = mid + 1;
            else hi = mid - 1;
        }
        return ~lo;
    }
}
