using RTSEngine.Core.State;
using RTSEngine.Core.Systems;
using RTSEngine.Core.Entities.Runtime;

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

        ProjectileSystem.Update(_context);

        GatherSystem.Update(_context);

        ConstructionSystem.Update(_context);

        ProductionSystem.Update(_context);

        ScoreSystem.Update(_context.World);

        _context.Victory.Check(_context.World.Players, _context.Settings.Victory);

        if (_context.Victory.WinnerPlayerId.HasValue)
        {
            _context.World.Finish();
        }

        CleanupSystem.Update(_context.World);

        _context.World.AdvanceTick();
    }
}
