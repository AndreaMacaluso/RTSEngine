using RTSEngine.Core.State;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Map.Runtime;

namespace RTSEngine.Core.Systems;

public static class MovementSystem
{
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
                continue;
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
            return;
        }

        unit.Position = target;
        unit.TargetPosition = null;
    }
}