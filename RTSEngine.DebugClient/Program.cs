using RTSEngine.Core.Map.Loading;
using RTSEngine.Core.State;
using RTSEngine.Core.Simulation;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.DebugClient.Renders;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.Loader;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Commands;
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
            new GridPosition(5,12));

        world.AddEntity(villager);

        var command= new MoveCommand
        {
            UnitIds = [villager.Id],
            Target = new GridPosition(20, 25)
        };
        world.AddCommand(command);

        var villager_2 = UnitFactory.Create(
            villagerDefinition,
            2,
            new GridPosition(35, 30));

        world.AddEntity(villager_2);

        var command_2= new MoveCommand
        {
            UnitIds = [villager_2.Id],
            Target = new GridPosition(10, 15)
        };
        world.AddCommand(command_2);


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
                            world.Resume();
                        }
                        else
                        {
                             world.Pause();
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
