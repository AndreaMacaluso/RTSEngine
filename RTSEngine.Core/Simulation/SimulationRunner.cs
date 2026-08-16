using RTSEngine.Core.State;
using RTSEngine.Core.Systems;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.Units;
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
        var deadUnits = world.Entities
            .OfType<Unit>()
            .Where(u => u.IsDead)
            .ToList();

        foreach (var unit in deadUnits)
        {
            world.RemoveEntity(unit);
        }
    }
}