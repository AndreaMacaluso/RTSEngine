using RTSEngine.Core.Map.Loading;
using RTSEngine.Core.State;
using RTSEngine.Core.Simulation;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.DebugClient.Renders;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.Loader;
using  RTSEngine.Core.Entities.Definitions;
namespace RTSEngine.DebugClient;
class Program
{
    static void Main()
    {
        Console.WriteLine("a RTS Debug Client");
       
        var mapPath = Path.Combine(
            AppContext.BaseDirectory,
            "Data",
            "Maps",
            "map_00.json");

        var unitsPath = Path.Combine(
            AppContext.BaseDirectory,
            "Data",
            "Units",
            "units.json");
    
        //map
        var loader = new JsonMapLoader();

        var mapData = loader.Load(mapPath);

        var world = WorldBuilder.Build(mapData);
        //unit
        var unitLoader = new UnitDefinitionLoader();
        var unitDefinitions = unitLoader.Load(unitsPath);

        var unitRepository = new UnitDefinitionRepository(unitDefinitions);

        var villagerDefinition = unitRepository.Get("villager");


        var villager = UnitFactory.Create(
            villagerDefinition,
            1,
            new GridPosition(5, 5));

        villager.PathQueue.Enqueue(new GridPosition(6, 5));
        villager.PathQueue.Enqueue(new GridPosition(7, 5));
        villager.PathQueue.Enqueue(new GridPosition(8, 5));
        villager.PathQueue.Enqueue(new GridPosition(8, 6));
        villager.PathQueue.Enqueue(new GridPosition(8, 7));

        world.AddEntity(villager);

        var simulation = new SimulationRunner(world);

        while (true)
        {
            Console.SetCursorPosition(0, 0);

            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);

                switch (key.Key)
                {
                    case ConsoleKey.Spacebar:
                    
                        if (world.State == WorldState.Paused)
                        {
                            world.Pause();
                        }
                        else
                        {
                            world.Resume();
                        }
                        break;
                    case ConsoleKey.N:
                        if (world.State == WorldState.Paused)
                        {
                            simulation.Step();
                        }
                        break;

                    case ConsoleKey.Escape:
                        return;
                }
            }

            simulation.Tick();

            Console.WriteLine($"Tick: {world.CurrentTick}");
           
            Console.WriteLine(
                (world.State == WorldState.Paused)
                    ? "PAUSED"
                    : "RUNNING");

            ConsoleRenderer.Render(
                world,
                RenderMode.Minimal);

            Thread.Sleep(300);
        }

    }
}
