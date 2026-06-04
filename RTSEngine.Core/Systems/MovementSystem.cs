using RTSEngine.Core.State;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Map.Runtime;

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

             if (unit.TargetPosition is null)
            {
                if (unit.PathQueue.Count > 0)
                {
                    unit.TargetPosition =
                        unit.PathQueue.Dequeue();
                }
                else
                {
                    continue;
                }
            }

            unit.MovementProgress += unit.MovementSpeed;

            if (unit.MovementProgress < 1f)
            {
                continue;
            }

            unit.MovementProgress = 0f;

            
            TryMove(
                world,
                unit,
                unit.TargetPosition.Value);
            unit.TargetPosition = null;
            
        }
    }

    private static bool IsAdjacent(
        GridPosition current,
        GridPosition target)
    {
        var dx = Math.Abs(current.X - target.X);

        var dy = Math.Abs(current.Y - target.Y);

        if (dx == 0 && dy == 0)
        {
            return false;
        }

        return dx <= 1 && dy <= 1;
    }
    private static void TryMove(
    GameWorld world,
    Unit unit,
    GridPosition target)
    {
        if (!IsAdjacent(unit.Position, target))
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
        unit.TargetPosition = null;
    }

    private static void RecalculatePath(
    GameWorld world,
    Unit unit)
    {
       
        if (unit.TargetPosition is null)
        {
            return;
        }

        unit.PathQueue =
            PathSystem.GeneratePath(
                world,
                unit.Position,
                unit.TargetPosition.Value);

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