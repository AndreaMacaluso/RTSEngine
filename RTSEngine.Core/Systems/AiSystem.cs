using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.AI;
using RTSEngine.Core.Players;

namespace RTSEngine.Core.Systems;


public static class AISystem
{
    public static void Update(RuntimeContext context)
    {
        foreach (var player in context.World.Players)
        {
            if (player.Controller != PlayerControllerType.AI)
            {
                continue;
            }
            if (context.World.CurrentTick - player.AI.LastDecisionTick < 10)
            {
                  continue;
            }
            player.AI.LastDecisionTick = context.World.CurrentTick;
            BasicAI.Update(context, player);
        }
    }
}