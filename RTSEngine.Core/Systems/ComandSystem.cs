using RTSEngine.Core.Commands;
using RTSEngine.Core.State;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Entities.States;

namespace RTSEngine.Core.Systems;

public static class CommandSystem
{
    public static void Update(GameWorld world)
    {
        while (world.PendingCommands.Count > 0)
        {
            var command = world.PendingCommands.Dequeue();

            ProcessCommand(world, command);
        }
    }

    private static void ProcessCommand(
        GameWorld world,
        ICommand command)
    {
        switch (command)
        {
            case MoveCommand moveCommand:
                ProcessMoveCommand(world, moveCommand);
                break;
        }
    }

    private static void ProcessMoveCommand(
    GameWorld world,
    MoveCommand command)
    {
        foreach (var unitId in command.UnitIds)
        {
            var unit = world.Entities
                .OfType<Unit>()
                .FirstOrDefault(u => u.Id == unitId);

            if (unit == null)
            {
                continue;
            }

            AssignMoveTarget(unit, command.Target,world);
        }
    }

    private static void AssignMoveTarget(
        Unit unit,
        GridPosition target,
        GameWorld world)
    {
        unit.Movement.PathQueue.Clear();

        var path = PathSystem.GeneratePath(
            world,
            unit.Position,
            target);

        foreach (var step in path)
            {
                unit.Movement.PathQueue.Enqueue(step);
            }

        unit.Movement.TargetPosition = target;

    }
}