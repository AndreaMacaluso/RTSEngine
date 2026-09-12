using RTSEngine.Core.Simulation;
using RTSEngine.Core.State;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Commands;
using RTSEngine.Core.Settings;
using RTSEngine.Core.Systems.Pathfinding;

namespace RTSEngine.Tests.TestHelpers;

public static class SimulationTestHelper
{
    public static RuntimeContext CreateContext(
        GameWorld world,
        GameSettings? settings = null,
        EngineSettings? engine = null)
    {
        return new RuntimeContext
        {
            World = world,
            UnitRepository = new UnitDefinitionRepository([]),
            BuildingRepository = new BuildingDefinitionRepository([]),
            CommandQueue = new CommandQueue(),
            PathFinder = new AStarPathFinder(new GroundMovementFilter()),
            Settings = settings ?? new GameSettings(),
            Engine = engine ?? new EngineSettings()
        };
    }

    public static void RunTicks(
        GameWorld world,
        int ticks,
        RuntimeContext? context = null)
    {
        context ??= CreateContext(world);
        var simulation = new SimulationRunner(context);

        for (int i = 0; i < ticks; i++)
        {
            simulation.Step();
        }
    }
}