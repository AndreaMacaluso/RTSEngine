using RTSEngine.Core.AI.Decisions;
using RTSEngine.Core.Players;
using RTSEngine.Core.Entities.Runtime;

namespace RTSEngine.Core.AI;

public static class BasicAI
{
    public static void Update(
        RuntimeContext context,
        Player player)
    {
        ProductionDecision.Execute(context, player);
        ConstructionDecision.Execute(context, player);
        GatherDecision.Execute(context.World, player);
    }
}
