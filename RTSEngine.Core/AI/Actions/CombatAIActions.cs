using RTSEngine.Core.Commands;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.State;

namespace RTSEngine.Core.AI.Actions;

public static class CombatAIActions
{
    public static void AttackTarget(
        GameWorld world,
        Unit unit,
        int targetEntityId)
    {
        world.AddCommand(new AttackCommand
        {
            UnitIds = [unit.Id],
            TargetEntityId = targetEntityId
        });
    }

    public static void MoveToTarget(
        GameWorld world,
        Unit unit,
        GridPosition target)
    {
        world.AddCommand(new MoveCommand
        {
            UnitIds = [unit.Id],
            Target = target
        });
    }
}
