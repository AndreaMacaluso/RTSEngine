using RTSEngine.Core.State;
using RTSEngine.Core.Systems;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.States;
using RTSEngine.Core.Actions;
using RTSEngine.Core.Helpers;
using RTSEngine.Core.Players;

namespace RTSEngine.Core.Simulation;

public class SimulationRunner
{
    private readonly RuntimeContext _context;

    public SimulationRunner(
        RuntimeContext context)
    {
        _context = context;
    }

    public void Tick()
    {
        if (_context.World.State != WorldState.Running)
        {
            return;
        }

        Step();
    }

    public void Step()
    {
        _context.World.Entities.RebuildSpatialIndex();

        CommandSystem.Update(_context);

        AISystem.Update(_context);

        MovementSystem.Update(_context);

        CombatSystem.Update(_context);

        GatherSystem.Update(_context);

        ResourceCleanupSystem.Update(_context.World);

        ConstructionSystem.Update(_context);

        ProductionSystem.Update(_context);

        ScoreSystem.Update(_context.World);

        _context.Victory.Check(_context.World.Players, _context.Settings.Victory);

        if (_context.Victory.WinnerPlayerId.HasValue)
        {
            _context.World.Finish();
        }

        RemoveDeadEntities(_context.World);

        _context.World.AdvanceTick();
    }

    private static void RemoveDeadEntities(GameWorld world)
    {
        var deadUnits = UnitQueries.FindDeadUnits(world);

        foreach (var unit in deadUnits)
        {
            ReleaseUnitPopulation(world, unit);
            var unitPlayer = world.GetPlayerById(unit.OwnerId) as Player;
            if (unitPlayer is not null)
            {
                world.Entities.Remove(unit, unitPlayer);
            }
        }

        var deadBuildings = WorldQueries.FindDeadBuildings(world);

        foreach (var building in deadBuildings)
        {
            ReleasePopulation(world, building);
            var buildingPlayer = world.GetPlayerById(building.OwnerId) as Player;
            if (buildingPlayer is not null)
            {
                world.Entities.Remove(building, buildingPlayer);
            }
        }
    }

    private static void ReleasePopulation(
        GameWorld world,
        Building building)
    {
        if (building.Definition.PopulationBonus <= 0)
        {
            return;
        }

        var owner = world.GetPlayerById(building.OwnerId);

        if (owner is not Player player)
        {
            return;
        }

        PopulationActions.DecreaseCap(
            player,
            building.Definition.PopulationBonus);
    }

    private static void ReleaseUnitPopulation(
        GameWorld world,
        Unit unit)
    {
        var owner = world.GetPlayerById(unit.OwnerId);

        if (owner is not Player player)
        {
            return;
        }

        PopulationActions.RemovePopulation(player, 1);
    }
}
