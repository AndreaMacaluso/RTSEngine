using RTSEngine.Core.Map.Loading;
using RTSEngine.Core.State;
using RTSEngine.Core.Simulation;
using RTSEngine.Core.Entities;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.DebugClient.Renders;

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

        var loader = new JsonMapLoader();

        var mapData = loader.Load(mapPath);

        var world = WorldBuilder.Build(mapData);

        //spawn a villager for testing
        // world.Entities.Add(new Villager
        // {
        //     Id = 1,
        //     Position = new GridPosition(5, 5)
        // });

        var simulation = new SimulationRunner(world);
        
        while (true)
        {
            Console.Clear();

            simulation.Tick();

            Console.WriteLine($"Tick: {world.CurrentTick}");
            
            ConsoleRenderer.Render(
                world,
                RenderMode.Minimal);;

            Thread.Sleep(200);
        }

    }
}
