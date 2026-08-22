using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Players;

namespace RTSEngine.Core.AI;

public abstract class AIBrain
{
    public void Execute(RuntimeContext context, Player player)
    {
        var action = Think(context, player);
        ExecutePlan(context, player, action);
    }

    protected abstract string Think(RuntimeContext context, Player player);
    protected abstract void ExecutePlan(RuntimeContext context, Player player, string action);
}
