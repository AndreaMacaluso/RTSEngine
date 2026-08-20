using RTSEngine.Core.Commands;
using RTSEngine.Core.State;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Entities.States;
using RTSEngine.Core.Helpers;
using RTSEngine.Core.Entities.Runtime;

namespace RTSEngine.Core.Systems;

public static class CommandSystem
{
    public static void Update(RuntimeContext context)
    {
        while (context.World.PendingCommands.Count > 0)
        {
            var command = context.World.DequeueCommand();

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
                ProcessMoveCommand(context.World, moveCommand);
                break;
            case GatherCommand gatherCommand:
                HandleGather(context.World, gatherCommand);
                break;
            case BuildCommand buildCommand:
                HandleBuild(context.World, buildCommand);
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
            unit.Build.BuildPosition = building.Position;
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

            if (unit.CurrentTask == UnitTask.Idle)
            {
                unit.CurrentTask = UnitTask.Moving;
            }

            AssignMoveTarget(unit, command.Target,world);
        }
    }

    public static void AssignMoveTarget(
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

        unit.Movement.Destination = target;
        unit.Movement.CurrentStep = null;

    }

    private static void HandleProduction(
    RuntimeContext context,
    QueueProductionCommand command)
    {

        GameWorld world = context.World;

        var building = world.GetBuildingById(command.BuildingId);

        if (building == null)
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
            var unit = world.GetUnitById(unitId);

            if (unit == null || unit.IsDead)
            {
                continue;
            }

            var target = world.GetEntityById(command.TargetEntityId);

            if (target == null)
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