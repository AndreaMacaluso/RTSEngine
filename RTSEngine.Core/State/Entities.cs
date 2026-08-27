using RTSEngine.Core.Entities;
using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Resources;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Players;
using RTSEngine.Core.Systems;

namespace RTSEngine.Core.State;

public class Entities
{
    private readonly Dictionary<int, Unit> _units = new();
    private readonly Dictionary<int, Building> _buildings = new();
    private readonly Dictionary<int, ResourceNode> _resources = new();
    private readonly List<Player> _players;
    private readonly SpatialIndex _spatial = new();
    private int _nextEntityId = 1;
    private bool _spatialDirty = true;

    public IReadOnlyDictionary<int, Unit> Units => _units;
    public IReadOnlyDictionary<int, Building> Buildings => _buildings;
    public IReadOnlyDictionary<int, ResourceNode> Resources => _resources;
    public SpatialIndex Spatial => EnsureSpatialIndex();

    public Entities(List<Player> players)
    {
        _players = players;
    }

    private SpatialIndex EnsureSpatialIndex()
    {
        if (_spatialDirty)
        {
            _spatial.Rebuild(
                _units.Values,
                _buildings.Values,
                _resources.Values);
            _spatialDirty = false;
        }
        return _spatial;
    }

    public int GenerateEntityId()
    {
        return _nextEntityId++;
    }

    public void Add(Unit unit, Player player)
    {
        ArgumentNullException.ThrowIfNull(unit);
        ArgumentNullException.ThrowIfNull(player);
        unit.Id = GenerateEntityId();
        _units[unit.Id] = unit;
        player.AddUnit(unit.Id);
        _spatialDirty = true;
    }

    public void Add(Building building, Player player)
    {
        ArgumentNullException.ThrowIfNull(building);
        ArgumentNullException.ThrowIfNull(player);
        building.Id = GenerateEntityId();
        _buildings[building.Id] = building;
        player.AddBuilding(building.Id);
        _spatialDirty = true;
    }

    public void Add(ResourceNode resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        resource.Id = GenerateEntityId();
        _resources[resource.Id] = resource;
        _spatialDirty = true;
    }

    public void Remove(Unit unit, Player player)
    {
        ArgumentNullException.ThrowIfNull(unit);
        ArgumentNullException.ThrowIfNull(player);
        _units.Remove(unit.Id);
        player.RemoveUnit(unit.Id);
        _spatialDirty = true;
    }

    public void Remove(Building building, Player player)
    {
        ArgumentNullException.ThrowIfNull(building);
        ArgumentNullException.ThrowIfNull(player);
        _buildings.Remove(building.Id);
        player.RemoveBuilding(building.Id);
        _spatialDirty = true;
    }

    public void Remove(ResourceNode resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        _resources.Remove(resource.Id);
        _spatialDirty = true;
    }

    public IEnumerable<Unit> GetUnits(Player player)
    {
        foreach (var id in player.UnitIds)
        {
            if (_units.TryGetValue(id, out var unit))
                yield return unit;
        }
    }

    public IEnumerable<Building> GetBuildings(Player player)
    {
        foreach (var id in player.BuildingIds)
        {
            if (_buildings.TryGetValue(id, out var building))
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
                if (_units.TryGetValue(id, out var unit) && !unit.IsDead)
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
                if (_buildings.TryGetValue(id, out var building) && !building.IsDead)
                    yield return building;
            }
        }
    }

    public Unit? GetUnitById(int id)
    {
        return _units.GetValueOrDefault(id);
    }

    public Building? GetBuildingById(int id)
    {
        return _buildings.GetValueOrDefault(id);
    }

    public ResourceNode? GetResourceById(int id)
    {
        return _resources.GetValueOrDefault(id);
    }

    public Entity? GetEntityById(int id)
    {
        if (_units.TryGetValue(id, out var unit))
            return unit;

        if (_buildings.TryGetValue(id, out var building))
            return building;

        if (_resources.TryGetValue(id, out var resource))
            return resource;

        return null;
    }

    public void RebuildSpatialIndex()
    {
        Spatial.Rebuild(
            _units.Values,
            _buildings.Values,
            _resources.Values);
        _spatialDirty = false;
    }
}
