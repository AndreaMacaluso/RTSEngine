using RTSEngine.Core.Simulation;
using RTSEngine.Core.State;
using RTSEngine.DebugClient.Renders;
using RTSEngine.Core.Diagnostics;
using RTSEngine.Core.Settings;
using RTSEngine.Core.Entities.Runtime;

namespace RTSEngine.DebugClient.Runtime;

public static class SimulationHost
{
    public static void Run(
        GameWorld world,
        SimulationRunner simulation,
        RuntimeContext context)
    {
        while (true)
        {
            if (!HandleInput(world, simulation))
            {
                return;
            }

            simulation.Tick();

            RenderFrame(world, context);
            Thread.Sleep(300 / (int)context.Settings.Speed);
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
                DebugSession.Log.Info("input.Spacebar.Pause");
                TogglePause(world);
                return true;

            case ConsoleKey.N:
                DebugSession.Log.Info("input.N.Advance");
                if (world.State == WorldState.Paused)
                {
                    simulation.Step();
                }
                return true;

            case ConsoleKey.Escape:
                DebugSession.Log.Info("input.Escape.Stop");
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

    private static void RenderFrame(GameWorld world, RuntimeContext context)
    {
        Console.SetCursorPosition(0, 0);
        Console.WriteLine($"Tick: {world.CurrentTick}    ");
        Console.WriteLine(
            world.State == WorldState.Paused
                ? "PAUSED "
                : world.State == WorldState.Finished
                    ? $"GAME OVER - Player {context.Victory.WinnerPlayerId} WINS!"
                    : "RUNNING");

        ConsoleRenderer.Render(
            world,
            RenderMode.Minimal);
    }
}