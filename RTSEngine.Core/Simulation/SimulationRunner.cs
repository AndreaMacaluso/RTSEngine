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
        if (_world.State != WorldState.Running)
        {
            return;
        }
        
        Step();
    }

    public void Step()
    {
        CommandSystem.Update(_world);
        MovementSystem.Update(_world);
        GatherSystem.Update(_world);
        ConstructionSystem.Update(_world);

        _world.AdvanceTick();
    }
}