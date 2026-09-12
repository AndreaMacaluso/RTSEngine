using RTSEngine.Core.State;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Entities.States;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Helpers;
using RTSEngine.Core.Entities.Runtime;

namespace RTSEngine.Core.Systems;

public static class MovementSystem
{
    private const int RepathThreshold = 1;
    private const int DeadlockThreshold = 10;

    public static void Update(RuntimeContext context)
    {
        GameWorld world = context.World;
        
        foreach (var unit in world.Entities.Units.Values)
        {
            if (unit.CurrentTask == EntityState.Decaying
                || unit.CurrentTask == EntityState.Dead)
            {
                continue;
            }

            if (unit.Movement.NeedsRepath
                && (unit.CurrentTask == EntityState.Moving
                    || unit.CurrentTask == EntityState.Attacking))
            {
                unit.Movement.NeedsRepath = false;

                if (unit.Movement.Destination is GridPosition destination)
                {
                    CommandSystem.AssignMoveTarget(
                        unit,
                        destination,
                        context);
                }

                continue;
            }

            if (unit.Movement.CurrentStep is null)
            {
                if (unit.Movement.PathQueue.Count == 0)
                {
                    if (unit.CurrentTask == EntityState.Moving)
                    {
                        unit.CurrentTask = EntityState.Idle;
                    }
                    else if (unit.CurrentTask == EntityState.Attacking
                        && !unit.Combat.TargetEntityId.HasValue
                        && unit.Combat.Phase == CombatPhase.AttackMoving)
                    {
                        unit.CurrentTask = EntityState.Idle;
                        unit.Combat.Phase = CombatPhase.Idle;
                    }

                    continue;
                }

                unit.Movement.CurrentStep =
                    unit.Movement.PathQueue.Dequeue();
            }

            unit.Movement.Progress += unit.Movement.Speed;

            if (unit.Movement.Progress < 1f)
            {
                continue;
            }

            while (unit.Movement.Progress >= 1f)
            {
                if (unit.Movement.CurrentStep is not GridPosition currentStep)
                {
                    break;
                }

                bool moved = TryMove(
                    world,
                    unit,
                    currentStep);

                if (!moved)
                {
                    break;
                }

                unit.Movement.Progress -= 1f;
                unit.Movement.CurrentStep = null;

                if (unit.Movement.PathQueue.Count == 0)
                {
                    break;
                }

                unit.Movement.CurrentStep =
                    unit.Movement.PathQueue.Dequeue();
            }
            
        }
    }
    private static bool TryMove(
    GameWorld world,
    Unit unit,
    GridPosition target)
    {
        if (!WorldQueries.IsAdjacent(unit.Position,target))
        {
            return false;
        }

        if (WorldQueries.IsTileBlocked(world, target.X, target.Y))
        {

            unit.Movement.BlockedTicks++;

            if (unit.Movement.BlockedTicks >= DeadlockThreshold)
            {
                unit.Movement.BlockedTicks = 0;
                unit.Movement.NeedsRepath = false;
                unit.Movement.PathQueue.Clear();
                unit.Movement.CurrentStep = null;
                unit.CurrentTask = EntityState.Idle;
                return false;
            }

            if (unit.Movement.BlockedTicks >= RepathThreshold)
            {
                unit.Movement.BlockedTicks = 0;
                unit.Movement.NeedsRepath = true;
                unit.Movement.CurrentStep = null;
            }

            return false;
        }

        unit.Position = target;
        unit.Movement.CurrentStep = null;
        unit.Movement.BlockedTicks = 0;
        unit.Movement.NeedsRepath = false;
        return true;
    }

    public static void BeginMove(
    GameWorld world,
    Unit unit,
    GridPosition destination)
    {
        unit.Movement.Destination = destination;
        unit.Movement.CurrentStep = null;
        unit.Movement.BlockedTicks = 0;
        unit.Movement.NeedsRepath = false;
    }
}