namespace RTSEngine.DebugClient;

using System;
using RTSEngine.Core.Map;
using RTSEngine.Core.State;
using RTSEngine.Core.Simulation;

class Program
{
    static void Main()
    {
        Console.WriteLine("a RTS Debug Client");
        var map = new TileMap(10, 10);

        var world = new GameWorld(map);

        var simulation = new SimulationRunner(world);

        while (true)
        {
            Console.Clear();

            simulation.Tick();

            Console.WriteLine($"Tick: {world.CurrentTick}");

            Renders.ConsoleRenderer.Render(world);

            Thread.Sleep(100);
        }
    }
}
