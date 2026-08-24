using RTSEngine.Core.Commands;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Map.Runtime;

namespace RTSEngine.Core.AI.Actions;

public static class CombatAIActions
{
    public static void AttackTarget(
        ICommandQueue commandQueue,
        Unit unit,
        int targetEntityId)
    {
        commandQueue.Enqueue(new AttackCommand
        {
            UnitIds = [unit.Id],
            TargetEntityId = targetEntityId
        });
    }

    public static void MoveToTarget(
        ICommandQueue commandQueue,
        Unit unit,
        GridPosition target)
    {
        commandQueue.Enqueue(new MoveCommand
        {
            UnitIds = [unit.Id],
            Target = target
        });
    }
}
