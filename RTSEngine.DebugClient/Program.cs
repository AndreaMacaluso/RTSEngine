using RTSEngine.Core.Simulation;
using RTSEngine.DebugClient.Bootstrap;
using RTSEngine.DebugClient.Runtime;
using RTSEngine.DebugClient.Scenarios;

namespace RTSEngine.DebugClient;

class Program
{
    static void Main()
    {
        Console.WriteLine("RTS Debug Client");

        var context = SimulationBootstrap.Create();

        // ScenarioBuilder.CreateMovementScenario(context);
        ScenarioBuilder.CreeateGatheringScenario(context);

        var simulation = new SimulationRunner(context.World);

        SimulationHost.Run(
            context.World,
            simulation);
    }
}