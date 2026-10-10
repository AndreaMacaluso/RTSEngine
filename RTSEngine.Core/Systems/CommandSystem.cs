using RTSEngine.Core.Commands;
using RTSEngine.Core.State;
using RTSEngine.Core.Entities;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Events;
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

            if (!ValidateCommand(command, context, out var reason))
            {
                context.Events.Publish(new GameEvent
                {
                    Tick = context.World.CurrentTick,
                    Type = EventType.CommandRejected,
                    Payload = $"{reason}:{command.GetType().Name}"
                });
                continue;
            }

            ProcessCommand(context, command);
        }
    }

    private static bool ValidateCommand(
        ICommand command,
        RuntimeContext context,
        out CommandRejectReason reason)
    {
        int maxUnits = context.Engine.MaxUnitsPerCommand;

        switch (command)
        {
            case MoveCommand move:
                if (!ValidateUnitCount(move.UnitIds, maxUnits, out reason))
                    return false;
                return ValidateOwnership(move.UnitIds, move.PlayerId, context, out reason);

            case AttackCommand attack:
                if (!ValidateUnitCount(attack.UnitIds, maxUnits, out reason))
                    return false;
                if (!ValidateOwnership(attack.UnitIds, attack.PlayerId, context, out reason))
                    return false;
                if (attack.Mode == AttackMode.Entity && !attack.TargetEntityId.HasValue)
                {
                    reason = CommandRejectReason.MissingTarget;
                    return false;
                }
                if ((attack.Mode == AttackMode.AttackMove || attack.Mode == AttackMode.Ground)
                    && !attack.TargetPosition.HasValue)
                {
                    reason = CommandRejectReason.MissingTarget;
                    return false;
                }
                reason = CommandRejectReason.None;
                return true;

            case GatherCommand gather:
                if (!ValidateUnitCount(gather.UnitIds, maxUnits, out reason))
                    return false;
                if (!ValidateOwnership(gather.UnitIds, gather.PlayerId, context, out reason))
                    return false;
                if (gather.ResourceId <= 0)
                {
                    reason = CommandRejectReason.MissingTarget;
                    return false;
                }
                reason = CommandRejectReason.None;
                return true;

            case BuildCommand build:
                if (!ValidateUnitCount(build.UnitIds, maxUnits, out reason))
                    return false;
                if (!ValidateOwnership(build.UnitIds, build.PlayerId, context, out reason))
                    return false;
                if (build.BuildingId <= 0)
                {
                    reason = CommandRejectReason.MissingTarget;
                    return false;
                }
                reason = CommandRejectReason.None;
                return true;

            case StopCommand stop:
                if (!ValidateUnitCount(stop.UnitIds, maxUnits, out reason))
                    return false;
                return ValidateOwnership(stop.UnitIds, stop.PlayerId, context, out reason);

            case StanceCommand stance:
                if (!ValidateUnitCount(stance.UnitIds, maxUnits, out reason))
                    return false;
                return ValidateOwnership(stance.UnitIds, stance.PlayerId, context, out reason);

            case ProductionCommand production:
                if (production.BuildingId <= 0)
                {
                    reason = CommandRejectReason.MissingTarget;
                    return false;
                }
                var building = context.World.Entities.GetBuildingById(production.BuildingId);
                if (building != null && building.OwnerId != production.PlayerId)
                {
                    reason = CommandRejectReason.NotOwner;
                    return false;
                }
                if (production.Action == ProductionActionType.QueueUnit
                    && string.IsNullOrEmpty(production.ProductId))
                {
                    reason = CommandRejectReason.MissingTarget;
                    return false;
                }
                if (production.Action == ProductionActionType.SetSpawnPoint
                    && !production.Target.HasValue)
                {
                    reason = CommandRejectReason.MissingTarget;
                    return false;
                }
                if (production.Action == ProductionActionType.SetRallyPoint
                    && !production.Target.HasValue)
                {
                    reason = CommandRejectReason.MissingTarget;
                    return false;
                }
                reason = CommandRejectReason.None;
                return true;

            default:
                reason = CommandRejectReason.UnknownCommand;
                return false;
        }
    }

    private static bool ValidateOwnership(
        List<int> unitIds,
        int playerId,
        RuntimeContext context,
        out CommandRejectReason reason)
    {
        foreach (var unitId in unitIds)
        {
            var unit = context.World.Entities.GetUnitById(unitId);

            if (unit != null && unit.OwnerId != playerId)
            {
                reason = CommandRejectReason.NotOwner;
                return false;
            }
        }

        reason = CommandRejectReason.None;
        return true;
    }

    private static bool ValidateUnitCount(
        List<int> unitIds,
        int maxUnits,
        out CommandRejectReason reason)
    {
        if (unitIds.Count == 0)
        {
            reason = CommandRejectReason.Empty;
            return false;
        }

        if (unitIds.Count > maxUnits)
        {
            reason = CommandRejectReason.TooManyUnits;
            return false;
        }

        reason = CommandRejectReason.None;
        return true;
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
            case ProductionCommand productionCommand:
                HandleProduction(context, productionCommand);
                break;
            case AttackCommand attackCommand:
                HandleAttack(context, attackCommand);
                break;
            case StopCommand stopCommand:
                HandleStop(context.World, stopCommand);
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
                unit.CurrentTask = EntityState.Idle;
                continue;
            }
           
            var target = WorldQueries.FindClosestAdjacentWalkableTile(
                    world,
                    unit.Position,
                    building.Position);
           
            if (target == null)
            {
                unit.CurrentTask = EntityState.Idle;
                unit.Build.BuildingId = null;
                continue;
            }

            unit.CurrentTask = EntityState.Building;
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
                unit.CurrentTask = EntityState.Idle;
                unit.Gather.TargetResourceId = null;
                continue;
            }

            var target = WorldQueries.FindClosestAdjacentWalkableTile(
                    world,
                    unit.Position,
                    resource.Position);

            if (target == null)
            {
                unit.CurrentTask = EntityState.Idle;
                unit.Gather.TargetResourceId = null;
                continue;
            }

            unit.CurrentTask = EntityState.Gathering;
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

        if (command.Formation == FormationType.None
            || command.UnitIds.Count == 1)
        {
            foreach (var unitId in command.UnitIds)
            {
                var unit = world.Entities.GetUnitById(unitId);

                if (unit == null)
                {
                    continue;
                }

                unit.CurrentTask = EntityState.Moving;

                AssignMoveTarget(unit, command.Target, context);
            }
        }
        else
        {
            var positions = FormationCalculator.Calculate(
                command.Formation,
                command.UnitIds.Count,
                command.Target);

            for (int i = 0; i < command.UnitIds.Count; i++)
            {
                var unit = world.Entities.GetUnitById(command.UnitIds[i]);

                if (unit == null)
                {
                    continue;
                }

                unit.CurrentTask = EntityState.Moving;

                AssignMoveTarget(unit, positions[i], context);
            }
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
        ProductionCommand command)
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

        switch (command.Action)
        {
            case ProductionActionType.QueueUnit:
                HandleQueueUnit(context, building, command);
                break;

            case ProductionActionType.SetSpawnPoint:
                HandleSetSpawnPoint(building, command);
                break;

            case ProductionActionType.SetRallyPoint:
                HandleSetRallyPoint(building, command);
                break;
        }
    }

    private static void HandleQueueUnit(
        RuntimeContext context,
        Building building,
        ProductionCommand command)
    {
        GameWorld world = context.World;

        var player = world.GetPlayerById(command.PlayerId);

        if (player == null)
        {
            return;
        }

        var productionDefinition = context.UnitRepository.Get(command.ProductId!);

        if (!building.Definition.Produces.Contains(productionDefinition.Id))
        {
            return;
        }

        if (building.Definition.Produces.Count > 0)
        {
            WorldQueries.EnsureSpawnPoint(world, building);
        }

        building.Production.Add(
            new ProductionTask(productionDefinition.Id, productionDefinition.ProductionTimeTicks)
        );
    }

    private static void HandleSetSpawnPoint(
        Building building,
        ProductionCommand command)
    {
        if (command.Target is not GridPosition target)
        {
            return;
        }

        building.Production.SpawnPoint = target;
    }

    private static void HandleSetRallyPoint(
        Building building,
        ProductionCommand command)
    {
        if (command.Target is not GridPosition target)
        {
            return;
        }

        building.Production.RallyPoint = target;
    }

    private static void HandleAttack(
        RuntimeContext context,
        AttackCommand command)
    {
        GameWorld world = context.World;

        if (command.Formation != FormationType.None
            && command.UnitIds.Count > 1
            && command.Mode == AttackMode.AttackMove
            && command.TargetPosition.HasValue)
        {
            var positions = FormationCalculator.Calculate(
                command.Formation,
                command.UnitIds.Count,
                command.TargetPosition.Value);

            for (int i = 0; i < command.UnitIds.Count; i++)
            {
                var unit = world.Entities.GetUnitById(command.UnitIds[i]);

                if (unit == null || unit.IsDead)
                {
                    continue;
                }

                unit.CurrentTask = EntityState.Attacking;
                unit.Combat.Clear();
                unit.Combat.Phase = CombatPhase.AttackMoving;
                AssignMoveTarget(unit, positions[i], context);
            }
        }
        else
        {
            foreach (var unitId in command.UnitIds)
            {
                var unit = world.Entities.GetUnitById(unitId);

                if (unit == null || unit.IsDead)
                {
                    continue;
                }

                switch (command.Mode)
                {
                    case AttackMode.Entity:
                        HandleAttackEntity(world, unit, command.TargetEntityId!.Value);
                        break;

                    case AttackMode.Guard:
                        unit.CurrentTask = EntityState.Attacking;
                        unit.Combat.Clear();
                        unit.Combat.Phase = CombatPhase.Guarding;
                        unit.Movement.PathQueue.Clear();
                        unit.Movement.CurrentStep = null;
                        break;

                    case AttackMode.AttackMove:
                        unit.CurrentTask = EntityState.Attacking;
                        unit.Combat.Clear();
                        unit.Combat.Phase = CombatPhase.AttackMoving;
                        AssignMoveTarget(unit, command.TargetPosition!.Value, context);
                        break;

                    case AttackMode.Ground:
                        if (unit.Definition.Category != EntityCategory.Siege)
                        {
                            continue;
                        }
                        unit.CurrentTask = EntityState.Attacking;
                        unit.Combat.Clear();
                        unit.Combat.TargetGroundPosition = command.TargetPosition!.Value;
                        unit.Combat.Phase = CombatPhase.MovingToTarget;
                        break;
                }
            }
        }
    }

    private static void HandleAttackEntity(
        GameWorld world,
        Unit unit,
        int targetEntityId)
    {
        var target = world.Entities.GetEntityById(targetEntityId);

        if (target == null)
        {
            return;
        }

        if (target is not IHittable)
        {
            return;
        }

        CombatSystem.BeginAttack(world, unit, targetEntityId);
    }

    private static void HandleStop(GameWorld world, StopCommand command)
    {
        foreach (var unitId in command.UnitIds)
        {
            var unit = world.Entities.GetUnitById(unitId);

            if (unit == null || unit.IsDead)
            {
                continue;
            }

            unit.CurrentTask = EntityState.Idle;
            unit.Combat.Clear();
            unit.Movement.PathQueue.Clear();
            unit.Movement.CurrentStep = null;
        }
    }

}
