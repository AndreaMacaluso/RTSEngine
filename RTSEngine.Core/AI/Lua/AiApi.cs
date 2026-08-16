using MoonSharp.Interpreter;
using RTSEngine.Core.Actions;
using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.Rules;
using RTSEngine.Core.Helpers;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Players;
using RTSEngine.Core.State;

namespace RTSEngine.Core.AI.Lua;

[MoonSharpUserData]
public class AiApi
{
    private readonly RuntimeContext _context;
    private readonly Player _player;

    public AiApi(RuntimeContext context, Player player)
    {
        _context = context;
        _player = player;
    }

    #region Queries - Resources

    public int GetWood() => _player.Economy.Get(ResourceType.Wood);
    public int GetFood() => _player.Economy.Get(ResourceType.Food);
    public int GetGold() => _player.Economy.Get(ResourceType.Gold);
    public int GetStone() => _player.Economy.Get(ResourceType.Stone);

    #endregion

    #region Queries - Population

    public int GetPopulation() => _player.Population.Current;
    public int GetPopulationCap() => _player.Population.Capacity;

    #endregion

    #region Queries - Units

    public int GetVillagerCount()
    {
        return _context.World.Entities
            .OfType<Unit>()
            .Count(u => u.OwnerId == _player.Id
                && !u.IsDead
                && u.Definition.Id == "villager");
    }

    public int GetMilitiaCount()
    {
        return _context.World.Entities
            .OfType<Unit>()
            .Count(u => u.OwnerId == _player.Id
                && !u.IsDead
                && u.Definition.Id == "militia");
    }

    public int GetUnitCount(string unitType)
    {
        return _context.World.Entities
            .OfType<Unit>()
            .Count(u => u.OwnerId == _player.Id
                && !u.IsDead
                && u.Definition.Id == unitType);
    }

    public int GetIdleUnitCount()
    {
        return _context.World.Entities
            .OfType<Unit>()
            .Count(u => u.OwnerId == _player.Id
                && !u.IsDead
                && u.CurrentTask == UnitTask.Idle);
    }

    public int GetIdleVillagerCount()
    {
        return _context.World.Entities
            .OfType<Unit>()
            .Count(u => u.OwnerId == _player.Id
                && !u.IsDead
                && u.Definition.Id == "villager"
                && u.CurrentTask == UnitTask.Idle);
    }

    public List<int> GetIdleVillagers()
    {
        return _context.World.Entities
            .OfType<Unit>()
            .Where(u => u.OwnerId == _player.Id
                && !u.IsDead
                && u.Definition.Id == "villager"
                && u.CurrentTask == UnitTask.Idle)
            .Select(u => u.Id)
            .ToList();
    }

    public List<int> GetIdleMilitary()
    {
        return _context.World.Entities
            .OfType<Unit>()
            .Where(u => u.OwnerId == _player.Id
                && !u.IsDead
                && u.Definition.CanAttack
                && u.CurrentTask == UnitTask.Idle)
            .Select(u => u.Id)
            .ToList();
    }

    public int GetMilitaryCount()
    {
        return _context.World.Entities
            .OfType<Unit>()
            .Count(u => u.OwnerId == _player.Id
                && !u.IsDead
                && u.Definition.CanAttack);
    }

    #endregion

    #region Queries - Buildings

    public bool HasBuilding(string buildingType)
    {
        return _context.World.Entities
            .OfType<Building>()
            .Any(b => b.OwnerId == _player.Id
                && b.Definition.Id == buildingType
                && b.IsCompleted
                && !b.IsDead);
    }

    public int GetBuildingCount(string buildingType)
    {
        return _context.World.Entities
            .OfType<Building>()
            .Count(b => b.OwnerId == _player.Id
                && b.Definition.Id == buildingType
                && b.IsCompleted
                && !b.IsDead);
    }

    public int GetTownCenter()
    {
        var tc = _context.World.Entities
            .OfType<Building>()
            .FirstOrDefault(b => b.OwnerId == _player.Id
                && b.Definition.Id == "town_center"
                && b.IsCompleted
                && !b.IsDead);
        return tc?.Id ?? -1;
    }

    public int GetBarracks()
    {
        var barracks = _context.World.Entities
            .OfType<Building>()
            .FirstOrDefault(b => b.OwnerId == _player.Id
                && b.Definition.Id == "barracks"
                && b.IsCompleted
                && !b.IsDead);
        return barracks?.Id ?? -1;
    }

    public bool IsBuildingQueued(string buildingType)
    {
        return _context.World.Entities
            .OfType<Building>()
            .Any(b => b.OwnerId == _player.Id
                && b.Definition.Id == buildingType
                && !b.IsCompleted);
    }

    #endregion

    #region Queries - Enemies

    public int GetEnemyUnitCount()
    {
        return _context.World.Entities
            .OfType<Unit>()
            .Count(u => u.OwnerId != _player.Id && !u.IsDead);
    }

    public int GetNearestEnemyId()
    {
        var idleMilitary = _context.World.Entities
            .OfType<Unit>()
            .FirstOrDefault(u => u.OwnerId == _player.Id
                && !u.IsDead
                && u.Definition.CanAttack
                && u.CurrentTask == UnitTask.Idle);

        if (idleMilitary == null) return -1;

        var enemy = _context.World.Entities
            .OfType<Unit>()
            .Where(u => u.OwnerId != _player.Id && !u.IsDead)
            .OrderBy(u => WorldQueries.ChebyshevDistance(idleMilitary.Position, u.Position))
            .FirstOrDefault();

        return enemy?.Id ?? -1;
    }

    public int GetEnemyTownCenterId()
    {
        var tc = _context.World.Entities
            .OfType<Building>()
            .FirstOrDefault(b => b.OwnerId != _player.Id
                && b.Definition.Id == "town_center"
                && b.IsCompleted
                && !b.IsDead);
        return tc?.Id ?? -1;
    }

    public DynValue GetEnemyTownCenter()
    {
        var tc = _context.World.Entities
            .OfType<Building>()
            .FirstOrDefault(b => b.OwnerId != _player.Id
                && b.Definition.Id == "town_center"
                && b.IsCompleted
                && !b.IsDead);

        if (tc == null) return DynValue.Nil;

        return DynValue.NewTable(new Table(null) { ["x"] = tc.Position.X, ["y"] = tc.Position.Y });
    }

    #endregion

    #region Queries - Time

    public int GetTick() => _context.World.CurrentTick;

    #endregion

    #region Queries - Unit Info

    public int GetUnitX(int unitId)
    {
        var unit = _context.World.GetUnitById(unitId);
        return unit?.Position.X ?? -1;
    }

    public int GetUnitY(int unitId)
    {
        var unit = _context.World.GetUnitById(unitId);
        return unit?.Position.Y ?? -1;
    }

    public string GetUnitTask(int unitId)
    {
        var unit = _context.World.GetUnitById(unitId);
        return unit?.CurrentTask.ToString() ?? "Unknown";
    }

    #endregion

    #region Actions - Production

    public bool TrainVillager(int buildingId)
    {
        var building = _context.World.GetBuildingById(buildingId);
        if (building == null || building.OwnerId != _player.Id) return false;

        return ProductionActions.TryTrainUnit(_context, building, "villager");
    }

    public bool TrainMilitia(int buildingId)
    {
        var building = _context.World.GetBuildingById(buildingId);
        if (building == null || building.OwnerId != _player.Id) return false;

        return ProductionActions.TryTrainUnit(_context, building, "militia");
    }

    public bool TrainUnit(int buildingId, string unitType)
    {
        var building = _context.World.GetBuildingById(buildingId);
        if (building == null || building.OwnerId != _player.Id) return false;

        return ProductionActions.TryTrainUnit(_context, building, unitType);
    }

    #endregion

    #region Actions - Movement

    public bool Move(int unitId, int x, int y)
    {
        var unit = _context.World.GetUnitById(unitId);
        if (unit == null || unit.OwnerId != _player.Id) return false;

        _context.World.AddCommand(new Commands.MoveCommand
        {
            UnitIds = [unitId],
            Target = new GridPosition(x, y)
        });
        return true;
    }

    public bool Stop(int unitId)
    {
        var unit = _context.World.GetUnitById(unitId);
        if (unit == null || unit.OwnerId != _player.Id) return false;

        unit.CurrentTask = UnitTask.Idle;
        unit.Movement.PathQueue.Clear();
        unit.Movement.CurrentStep = null;
        return true;
    }

    #endregion

    #region Actions - Gathering

    public bool Gather(int unitId, string resourceType)
    {
        var unit = _context.World.GetUnitById(unitId);
        if (unit == null || unit.OwnerId != _player.Id) return false;

        var resourceTypeEnum = resourceType.ToLower() switch
        {
            "wood" => Map.Runtime.ResourceType.Wood,
            "food" => Map.Runtime.ResourceType.Food,
            "gold" => Map.Runtime.ResourceType.Gold,
            "stone" => Map.Runtime.ResourceType.Stone,
            _ => Map.Runtime.ResourceType.None
        };

        if (resourceTypeEnum == Map.Runtime.ResourceType.None) return false;

        var resource = _context.World.Resources
            .FirstOrDefault(r => r.ResourceType == resourceTypeEnum && !r.IsDepleted);

        if (resource == null) return false;

        _context.World.AddCommand(new Commands.GatherCommand
        {
            UnitIds = [unitId],
            ResourceId = resource.Id
        });
        return true;
    }

    public bool GatherFrom(int unitId, int resourceId)
    {
        var unit = _context.World.GetUnitById(unitId);
        if (unit == null || unit.OwnerId != _player.Id) return false;

        _context.World.AddCommand(new Commands.GatherCommand
        {
            UnitIds = [unitId],
            ResourceId = resourceId
        });
        return true;
    }

    #endregion

    #region Actions - Combat

    public bool Attack(int unitId, int targetId)
    {
        var unit = _context.World.GetUnitById(unitId);
        if (unit == null || unit.OwnerId != _player.Id) return false;

        _context.World.AddCommand(new Commands.AttackCommand
        {
            UnitIds = [unitId],
            TargetEntityId = targetId
        });
        return true;
    }

    public bool AttackMove(int unitId, int x, int y)
    {
        var unit = _context.World.GetUnitById(unitId);
        if (unit == null || unit.OwnerId != _player.Id) return false;

        _context.World.AddCommand(new Commands.MoveCommand
        {
            UnitIds = [unitId],
            Target = new GridPosition(x, y)
        });
        return true;
    }

    public bool AttackNearestEnemy(int unitId)
    {
        var unit = _context.World.GetUnitById(unitId);
        if (unit == null || unit.OwnerId != _player.Id) return false;

        var enemy = _context.World.Entities
            .OfType<Unit>()
            .Where(u => u.OwnerId != _player.Id && !u.IsDead)
            .OrderBy(u => WorldQueries.ChebyshevDistance(unit.Position, u.Position))
            .FirstOrDefault();

        if (enemy == null) return false;

        _context.World.AddCommand(new Commands.AttackCommand
        {
            UnitIds = [unitId],
            TargetEntityId = enemy.Id
        });
        return true;
    }

    #endregion

    #region Actions - Building

    public bool Build(int unitId, string buildingType)
    {
        var unit = _context.World.GetUnitById(unitId);
        if (unit == null || unit.OwnerId != _player.Id) return false;

        var definition = _context.BuildingRepository.Get(buildingType);

        var position = BuildingPlacementRules.FindFreePosition(
            _context.World, definition, unit.Position);

        if (position == null) return false;

        var building = BuildingFactory.Create(definition, _player.Id, position.Value);
        _context.World.AddEntity(building);

        _context.World.AddCommand(new Commands.BuildCommand
        {
            UnitIds = [unitId],
            BuildingId = building.Id
        });
        return true;
    }

    public bool BuildAt(int unitId, string buildingType, int x, int y)
    {
        var unit = _context.World.GetUnitById(unitId);
        if (unit == null || unit.OwnerId != _player.Id) return false;

        var definition = _context.BuildingRepository.Get(buildingType);
        var position = new GridPosition(x, y);

        if (!BuildingPlacementRules.CanPlace(_context.World, definition, position))
            return false;

        var building = BuildingFactory.Create(definition, _player.Id, position);
        _context.World.AddEntity(building);

        _context.World.AddCommand(new Commands.BuildCommand
        {
            UnitIds = [unitId],
            BuildingId = building.Id
        });
        return true;
    }

    #endregion
}
