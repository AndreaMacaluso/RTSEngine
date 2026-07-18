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

             if (unit.Movement.TargetPosition is null)
            {
                if (unit.Movement.PathQueue.Count > 0)
                {
                    unit.Movement.TargetPosition =
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
                unit.Movement.TargetPosition.Value);
            unit.Movement.TargetPosition = null;
            
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
            RecalculatePath(
                world,
                unit);

            return;
        }

        unit.Position = target;
        unit.Movement.TargetPosition = null;
    }

    private static void RecalculatePath(
    GameWorld world,
    Unit unit)
    {
       
        if (unit.Movement.TargetPosition is null)
        {
            return;
        }

        unit.Movement.PathQueue =
            PathSystem.GeneratePath(
                world,
                unit.Position,
                unit.Movement.TargetPosition.Value);

        // if( unit.PathQueue == [])
        // {
        //     unit.BlockedTicks++;
        // }
        // else
        // {
        //     unit.BlockedTicks = 0;
        // }
        
    }
}