using RTSEngine.Core.State;
using RTSEngine.Core.Systems;

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
        CommandSystem.Update(_world);
        MovementSystem.Update(_world);
        _world.AdvanceTick();
    }
}