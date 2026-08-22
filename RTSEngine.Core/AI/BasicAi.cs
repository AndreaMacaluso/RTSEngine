using RTSEngine.Core.AI.Brains;
using RTSEngine.Core.Players;
using RTSEngine.Core.Entities.Runtime;

namespace RTSEngine.Core.AI;

public static class BasicAI
{   
    // This AI implementation is only for simulation purposes. It's throwaway code when Lua implementation comes in.
    public static void Update(RuntimeContext context, Player player)
    {
        new GatherBrain().Execute(context, player);
        new ConstructionBrain().Execute(context, player);
        new ProductionBrain().Execute(context, player);
        new CombatBrain().Execute(context, player);
    }
}
