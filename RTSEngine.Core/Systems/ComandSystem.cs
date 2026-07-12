using RTSEngine.Core.Commands;
using RTSEngine.Core.State;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Entities.States;
using RTSEngine.Core.Helpers;

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
            case GatherCommand gatherCommand:
                HandleGather(world, gatherCommand);
                break;
            case BuildCommand buildCommand:
                HandleBuild(world, buildCommand);
                break;
        }
    }
    private static void HandleBuild(
    GameWorld world,
    BuildCommand command)
    {
        foreach (var unitId in command.UnitIds)
        {
            var unit = world.GetUnitById(unitId);
            if (unit == null)
                continue;

            var building = world.GetBuildingById(command.BuildingId);

            if (building == null)
            {
                unit.CurrentTask = UnitTask.Idle;
                continue;
            }
           
            var target = WorldQueries.FindClosestAdjacentWalkableTile(
                    world,
                    unit.Position,
                    building.Position);
           
            if (target == null)
            {
                unit.CurrentTask = UnitTask.Idle;
                unit.Build.BuildingId = null;
                continue;
            }

            unit.CurrentTask = UnitTask.Building;
            unit.Build.BuildingId = building.Id;
            unit.Build.Phase = BuildPhase.MovingToConstruction;
            AssignMoveTarget(unit, target.Value, world);
        }
    }
    private static void HandleGather(
    GameWorld world,
    GatherCommand command)
    {
        foreach (var unitId in command.UnitIds)
        {
            var unit = world.GetUnitById(unitId);
            if (unit == null)
                continue;

            var resource = world.GetResourceById(command.ResourceId);

            if (resource == null)
            {
                unit.CurrentTask = UnitTask.Idle;
                unit.Gather.TargetResourceId = null;
                continue;
            }

            var target = WorldQueries.FindClosestAdjacentWalkableTile(
                    world,
                    unit.Position,
                    resource.Position);

            if (target == null)
            {
                unit.CurrentTask = UnitTask.Idle;
                unit.Gather.TargetResourceId = null;
                continue;
            }

            unit.CurrentTask = UnitTask.Gathering;
            unit.Gather.TargetResourceId = command.ResourceId;
            unit.Gather.Phase = GatherPhase.MovingToResource;
            unit.Gather.CarriedResource = resource.ResourceType;
            AssignMoveTarget(unit, target.Value, world);
        }
    }
    private static void ProcessMoveCommand(
    GameWorld world,
    MoveCommand command)
    {
        foreach (var unitId in command.UnitIds)
        {
            var unit = world.GetUnitById(unitId);

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