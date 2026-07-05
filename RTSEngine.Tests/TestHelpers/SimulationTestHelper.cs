using RTSEngine.Core.Simulation;
using RTSEngine.Core.State;

namespace RTSEngine.Tests.TestHelpers;

public static class SimulationTestHelper
{
    public static void RunTicks(
        GameWorld world,
        int ticks)
    {
        var simulation = new SimulationRunner(world);

        for (int i = 0; i < ticks; i++)
        {
            simulation.Step();
        }
    }
}