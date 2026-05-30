using RTSEngine.Core.Commands;
using RTSEngine.Core.State;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Map.Runtime;

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

            AssignMoveTarget(unit, command.Target);
        }
    }

    private static void AssignMoveTarget(
        Unit unit,
        GridPosition target)
    {
        unit.PathQueue.Clear();

        var path = PathSystem.GeneratePath(
            unit.Position,
            target);

        foreach (var step in path)
            {
                unit.PathQueue.Enqueue(step);
            }

        unit.TargetPosition = target;

    }
}