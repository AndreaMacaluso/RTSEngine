using RTSEngine.Core.State;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Helpers;

namespace RTSEngine.Core.Systems;

public static class MovementSystem
{
    private const int RepathThreshold = 5;
    public static void Update(GameWorld world)
    {
        
        foreach (var entity in world.Entities)
        {
            if (entity is not Unit unit)
            {
                continue;
            } 

            if (unit.Movement.CurrentStep is null)
            {
                if (unit.Movement.PathQueue.Count > 0)
                {
                        unit.Movement.PathQueue.Dequeue();
                }
                else
                {
                    continue;
                }
            }

            unit.Movement.Progress += unit.Movement.Speed;

            if (unit.Movement.Progress < 1f)
            {
                continue;
            }

            unit.Movement.Progress = 0f;

            
            TryMove(
                world,
                unit,
                unit.Movement.CurrentStep.Value);
            unit.Movement.CurrentStep = null;
            
        }
    }
    private static void TryMove(
    GameWorld world,
    Unit unit,
    GridPosition target)
    {
        if (!WorldQueries.IsAdjacent(unit.Position,target))
        {
            return;
        }

        if (world.IsTileBlocked(target.X, target.Y))
        {

            unit.Movement.BlockedTicks++;

            if (unit.Movement.BlockedTicks >= RepathThreshold)
            {

                unit.Movement.BlockedTicks = 0;
                unit.Movement.NeedsRepath = true;
            }

            return;
        }

        unit.Position = target;
        unit.Movement.TargetPosition = null;
        unit.Movement.CurrentStep = null;
        unit.Movement.BlockedTicks = 0;
        unit.Movement.NeedsRepath = false;
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

        CommandSystem.AssignMoveTarget(
            unit,
            destination,
            world);
    }
}