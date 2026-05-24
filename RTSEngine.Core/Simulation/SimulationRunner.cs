using RTSEngine.Core.State;

namespace RTSEngine.Core.Simulation;

public class SimulationRunner
{
    private readonly GameWorld _world;

    public SimulationRunner(GameWorld world)
    {
        _world = world;
    }

    public void Tick()
    {
        _world.AdvanceTick();
    }
}