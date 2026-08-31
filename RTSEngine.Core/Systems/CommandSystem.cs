using RTSEngine.Core.Commands;
using RTSEngine.Core.State;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Entities.States;
using RTSEngine.Core.Helpers;
using RTSEngine.Core.Entities.Runtime;

namespace RTSEngine.Core.Systems;

public static class CommandSystem
{
    public static void Update(RuntimeContext context)
    {
        while (context.CommandQueue.Count > 0)
        {
            var command = context.CommandQueue.Dequeue();

            if (command is null) break;

            ProcessCommand(context, command);
        }
    }

    private static void ProcessCommand(
        RuntimeContext context,
        ICommand command)
    {
        switch (command)
        {
            case MoveCommand moveCommand:
                ProcessMoveCommand(context, moveCommand);
                break;
            case GatherCommand gatherCommand:
                HandleGather(context, gatherCommand);
                break;
            case BuildCommand buildCommand:
                HandleBuild(context, buildCommand);
                break;
            case QueueProductionCommand productionCommand:
                HandleProduction(context, productionCommand);
                break;
            case AttackCommand attackCommand:
                HandleAttack(context.World, attackCommand);
                break;
        }
    }
    private static void HandleBuild(
    RuntimeContext context,
    BuildCommand command)
    {
        GameWorld world = context.World;

        foreach (var unitId in command.UnitIds)
        {
            var unit = world.Entities.GetUnitById(unitId);
            if (unit == null)
                continue;

            var building = world.Entities.GetBuildingById(command.BuildingId);

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
            AssignMoveTarget(unit, target.Value, context);
        }
    }
    private static void HandleGather(
    RuntimeContext context,
    GatherCommand command)
    {
        GameWorld world = context.World;

        foreach (var unitId in command.UnitIds)
        {
            var unit = world.Entities.GetUnitById(unitId);
            if (unit == null)
                continue;

            var resource = world.Entities.GetResourceById(command.ResourceId);

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
            AssignMoveTarget(unit, target.Value, context);
        }
    }
    private static void ProcessMoveCommand(
    RuntimeContext context,
    MoveCommand command)
    {
        GameWorld world = context.World;

        foreach (var unitId in command.UnitIds)
        {
            var unit = world.Entities.GetUnitById(unitId);

            if (unit == null)
            {
                continue;
            }

            unit.CurrentTask = UnitTask.Moving;

            AssignMoveTarget(unit, command.Target, context);
        }
    }

    public static void AssignMoveTarget(
        Unit unit,
        GridPosition target,
        RuntimeContext context)
    {
        unit.Movement.PathQueue.Clear();

        if (unit.Position == target)
        {
            unit.Movement.Destination = target;
            unit.Movement.CurrentStep = null;
            return;
        }

        var path = context.PathFinder.FindPath(
            context.World,
            unit.Position,
            target);

        foreach (var step in path)
        {
            unit.Movement.PathQueue.Enqueue(step);
        }

        unit.Movement.Destination = target;
        unit.Movement.CurrentStep = null;

    }

    private static void HandleProduction(
    RuntimeContext context,
    QueueProductionCommand command)
    {

        GameWorld world = context.World;

        var building = world.Entities.GetBuildingById(command.BuildingId);

        if (building == null)
        {
            return;
        }

        if (building.OwnerId != command.PlayerId)
        {
            return;
        }

        var player =world.GetPlayerById(command.PlayerId);

        if (player == null)
        {
            return;
        }

        var productionDefinition = context.UnitRepository.Get(command.ProductId);

        if (!building.Definition.Produces.Contains(productionDefinition.Id))
        {
            return;
        }

        if (building.Definition.Produces.Count > 0)
        {
            WorldQueries.EnsureSpawnPoint(world, building);
        }

        building.Production.Add(
            new ProductionTask(productionDefinition.Id,productionDefinition.ProductionTimeTicks)
        );
    }

    private static void HandleAttack(
    GameWorld world,
    AttackCommand command)
    {
        foreach (var unitId in command.UnitIds)
        {
            var unit = world.Entities.GetUnitById(unitId);

            if (unit == null || unit.IsDead)
            {
                continue;
            }

            var target = world.Entities.GetEntityById(command.TargetEntityId);

            if (target == null)
            {
                continue;
            }

            if (target is not Unit and not Building)
            {
                continue;
            }

            CombatSystem.BeginAttack(
                world,
                unit,
                command.TargetEntityId);
        }
    }
}