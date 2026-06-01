using RTSEngine.Core.State;
using RTSEngine.Core.Systems;

namespace RTSEngine.Core.Simulation;

public class SimulationRunner
{
    private readonly GameWorld _world;

    public bool IsPaused { get; private set; }

    public SimulationRunner(GameWorld world)
    {
        _world = world;
    }

    public void TogglePause()
    {
        IsPaused = !IsPaused;
    }
    public void Tick()
    {
        if (IsPaused)
        {
            return;
        }
        
        Step();
    }

    public void Step()
    {
        CommandSystem.Update(_world);
        MovementSystem.Update(_world);

        _world.AdvanceTick();
    }
}