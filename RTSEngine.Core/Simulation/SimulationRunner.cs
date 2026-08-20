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
        CommandSystem.Update(_context);

        AISystem.Update(_context);

        MovementSystem.Update(_context.World);

        CombatSystem.Update(_context.World);

        GatherSystem.Update(_context.World);

        ResourceCleanupSystem.Update(_context.World);

        ConstructionSystem.Update(_context.World);

        ProductionSystem.Update(_context);

        RemoveDeadEntities(_context.World);

        _context.World.AdvanceTick();
    }

    private static void RemoveDeadEntities(GameWorld world)
    {
        var deadUnits = UnitQueries.FindDeadUnits(world);

        foreach (var unit in deadUnits)
        {
            ReleaseUnitPopulation(world, unit);
            world.RemoveEntity(unit);
        }

        var deadBuildings = WorldQueries.FindDeadBuildings(world);

        foreach (var building in deadBuildings)
        {
            ReleaseBuilders(world, building);
            ReleasePopulation(world, building);
            world.RemoveEntity(building);
        }
    }

    private static void ReleaseBuilders(
        GameWorld world,
        Building building)
    {
        var builders = UnitQueries.FindBuildersForBuilding(world, building);

        foreach (var builder in builders)
        {
            ConstructionActions.StopBuilding(builder);
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
