using RTSEngine.Core.Simulation;
using RTSEngine.Core.State;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Commands;
using RTSEngine.Core.Systems.Pathfinding;

namespace RTSEngine.Tests.TestHelpers;

public static class SimulationTestHelper
{
    public static void RunTicks(
        GameWorld world,
        int ticks,
        RuntimeContext? context = null)
    {
        context ??= new RuntimeContext
        {
            World = world,
            UnitRepository = new UnitDefinitionRepository([]),
            BuildingRepository = new BuildingDefinitionRepository([]),
            CommandQueue = new CommandQueue(),
            PathFinder = new AStarPathFinder(new GroundMovementFilter())
        };
        var simulation = new SimulationRunner(context);

        for (int i = 0; i < ticks; i++)
        {
            simulation.Step();
        }
    }
}