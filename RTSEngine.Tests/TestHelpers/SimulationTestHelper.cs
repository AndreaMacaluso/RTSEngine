using RTSEngine.Core.Simulation;
using RTSEngine.Core.State;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.Definitions;

namespace RTSEngine.Tests.TestHelpers;

public static class SimulationTestHelper
{
    public static void RunTicks(
        GameWorld world,
        int ticks)
    {
        RuntimeContext context = new RuntimeContext
        {
            World = world,
            UnitRepository = new UnitDefinitionRepository([]),
            BuildingRepository = new BuildingDefinitionRepository([])
        };
        var simulation = new SimulationRunner(context);

        for (int i = 0; i < ticks; i++)
        {
            simulation.Step();
        }
    }
}