using RTSEngine.Core.Map.Loading;
using RTSEngine.Core.State;
using RTSEngine.Core.Simulation;

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

        var builder = new TileMapBuilder();

        var tileMap = builder.Build(mapData);

        var world = new GameWorld(tileMap);

        var simulation = new SimulationRunner(world);

        while (true)
        {
            Console.Clear();

            simulation.Tick();

            Console.WriteLine($"Tick: {world.CurrentTick}");

            Renders.ConsoleRenderer.Render(world);

            Thread.Sleep(200);
        }
    }
}
