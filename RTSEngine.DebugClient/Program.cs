using RTSEngine.Core.Simulation;
using RTSEngine.DebugClient.Bootstrap;
using RTSEngine.DebugClient.Runtime;
using RTSEngine.DebugClient.Scenarios;
using RTSEngine.Core.Diagnostics;
namespace RTSEngine.DebugClient;

class Program
{
    static void Main()
    {
        Console.WriteLine("RTS Debug Client");

        var baseDirectory = AppContext.BaseDirectory;

        var logsDirectory = Path.Combine(baseDirectory, "Logs");

        Directory.CreateDirectory(logsDirectory);

        var logPath = Path.Combine(
            logsDirectory,
            $"session_{DateTime.Now:yyyyMMdd_HHmmss}.log");

        var logger = new Logger();
        logger.AddSink(new FileLogSink(logPath));
        DebugSession.Initialize(logger);

        DebugSession.Log.Info("Simulation Starting");
      

        //DebugSession.Dump(...);
        var context = SimulationBootstrap.Create();
        DebugSession.Log.Info("context Created");
        DebugSession.Log.Info("CreeateGatheringScenario");
        // ScenarioBuilder.CreateMovementScenario(context);
        ScenarioBuilder.CreeateGatheringScenario(context);
        DebugSession.Log.Info("Create Simulation Runners");
        var simulation = new SimulationRunner(context.World);

        SimulationHost.Run(
            context.World,
            simulation);
    }
}