using RTSEngine.Core.Simulation;
using RTSEngine.Core.State;
using RTSEngine.DebugClient.Renders;

namespace RTSEngine.DebugClient.Runtime;

public static class SimulationHost
{
    private const int FrameDelayMs = 300;

    public static void Run(
        GameWorld world,
        SimulationRunner simulation)
    {
        while (true)
        {
            if (!HandleInput(world, simulation))
            {
                return;
            }

            Console.SetCursorPosition(0, 0);

            simulation.Tick();

            RenderFrame(world);

            Thread.Sleep(FrameDelayMs);
        }
    }

    private static bool HandleInput(
        GameWorld world,
        SimulationRunner simulation)
    {
        if (!Console.KeyAvailable)
        {
            return true;
        }

        var key = Console.ReadKey(true);

        switch (key.Key)
        {
            case ConsoleKey.Spacebar:
                TogglePause(world);
                return true;

            case ConsoleKey.N:
                if (world.State == WorldState.Paused)
                {
                    simulation.Step();
                }
                return true;

            case ConsoleKey.Escape:
                return false;

            default:
                return true;
        }
    }

    private static void TogglePause(GameWorld world)
    {
        if (world.State == WorldState.Paused)
        {
            world.Resume();
        }
        else
        {
            world.Pause();
        }
    }

    private static void RenderFrame(GameWorld world)
    {
        Console.WriteLine($"Tick: {world.CurrentTick}");
        Console.WriteLine(
            world.State == WorldState.Paused
                ? "PAUSED"
                : "RUNNING");

        ConsoleRenderer.Render(
            world,
            RenderMode.Minimal);
    }
}