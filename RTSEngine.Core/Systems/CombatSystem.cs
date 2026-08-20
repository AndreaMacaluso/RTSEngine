using RTSEngine.Core.State;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Entities.States;
using RTSEngine.Core.Helpers;
using RTSEngine.Core.Map.Runtime;

namespace RTSEngine.Core.Systems;

public static class CombatSystem
{
    public static void Update(GameWorld world)
    {
        foreach (var entity in world.Entities)
        {
            if (entity is not Unit unit)
            {
                continue;
            }

            if (unit.CurrentTask != UnitTask.Attacking)
            {
                continue;
            }

            unit.Combat.TickCooldown();

            switch (unit.Combat.Phase)
            {
                case CombatPhase.MovingToTarget:
                    HandleMovingToTarget(world, unit);
                    break;

                case CombatPhase.Attacking:
                    HandleAttacking(world, unit);
                    break;
            }
        }
    }

    private static void HandleMovingToTarget(
        GameWorld world,
        Unit unit)
    {
        if (unit.Combat.TargetEntityId is not int targetId)
        {
            StopAttacking(unit);
            return;
        }

        var target = world.GetEntityById(targetId);

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
                    world);
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

        var target = world.GetEntityById(targetId);

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

        target.TakeDamage(unit.Combat.AttackDamage);
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
