using RTSEngine.Core.State;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.States;
using RTSEngine.Core.Helpers;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Entities.Runtime;

namespace RTSEngine.Core.Systems;

public static class CombatSystem
{
    public static void Update(RuntimeContext context)
    {
        GameWorld world = context.World;

        foreach (var unit in world.Entities.Units.Values)
        {

            if (unit.CurrentTask != UnitTask.Attacking)
            {
                continue;
            }

            unit.Combat.TickCooldown();

            switch (unit.Combat.Phase)
            {
                case CombatPhase.MovingToTarget:
                    HandleMovingToTarget(context, unit);
                    break;

                case CombatPhase.Attacking:
                    HandleAttacking(world, unit);
                    break;
            }
        }
    }

    private static void HandleMovingToTarget(
        RuntimeContext context,
        Unit unit)
    {
        GameWorld world = context.World;

        if (unit.Combat.TargetEntityId is not int targetId)
        {
            StopAttacking(unit);
            return;
        }

        var target = world.Entities.GetEntityById(targetId);

        if (target == null || IsTargetDead(target))
        {
            StopAttacking(unit);
            return;
        }

        int distance = WorldQueries.ChebyshevDistance(
            unit.Position,
            target.Position);

        if (distance <= unit.Combat.AttackRange)
        {
            unit.Combat.Phase = CombatPhase.Attacking;
            unit.Movement.PathQueue.Clear();
            unit.Movement.CurrentStep = null;
            return;
        }

        if (unit.Movement.PathQueue.Count == 0
            && unit.Movement.CurrentStep == null)
        {
            var adjacentTile = WorldQueries
                .FindClosestAdjacentWalkableTile(
                    world,
                    unit.Position,
                    target.Position);

            if (adjacentTile is GridPosition tile)
            {
                CommandSystem.AssignMoveTarget(
                    unit,
                    tile,
                    context);
            }
            else
            {
                StopAttacking(unit);
            }
        }
    }

    private static void HandleAttacking(
        GameWorld world,
        Unit unit)
    {
        if (unit.Combat.TargetEntityId is not int targetId)
        {
            StopAttacking(unit);
            return;
        }

        var target = world.Entities.GetEntityById(targetId);

        if (target == null || IsTargetDead(target))
        {
            StopAttacking(unit);
            return;
        }

        int distance = WorldQueries.ChebyshevDistance(
            unit.Position,
            target.Position);

        if (distance > unit.Combat.AttackRange)
        {
            unit.Combat.Phase = CombatPhase.MovingToTarget;
            return;
        }

        if (unit.Combat.IsOnCooldown)
        {
            return;
        }

        switch (target)
        {
            case Unit u:
                u.Health.TakeDamage(unit.Combat.AttackDamage);
                break;
            case Building b:
                b.Health.TakeDamage(unit.Combat.AttackDamage);
                break;
        }
        unit.Combat.ResetCooldown();
    }

    public static void BeginAttack(
        GameWorld world,
        Unit unit,
        int targetEntityId)
    {
        unit.CurrentTask = UnitTask.Attacking;
        unit.Combat.TargetEntityId = targetEntityId;
        unit.Combat.Phase = CombatPhase.MovingToTarget;
        unit.Movement.PathQueue.Clear();
        unit.Movement.CurrentStep = null;
    }

    public static void StopAttacking(Unit unit)
    {
        unit.Combat.Clear();
        unit.CurrentTask = UnitTask.Idle;
    }

    private static bool IsTargetDead(Entities.Entity target)
    {
        return target.IsDead;
    }
}
