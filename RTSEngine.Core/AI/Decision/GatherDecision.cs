using RTSEngine.Core.AI.Actions;
using RTSEngine.Core.Helpers;
using RTSEngine.Core.Players;
using RTSEngine.Core.State;
using RTSEngine.Core.Map.Runtime;

namespace RTSEngine.Core.AI.Decisions;

public static class GatherDecision
{
    public static void Execute(
        GameWorld world,
        Player player)
    {

        foreach (var villager in UnitQueries.FindIdleVillagers(world, player))
        {
            var resource = WorldQueries.FindClosestResource(
                world,
                villager.Position,
                ResourceType.Wood);

            if (resource == null)
            {
                continue;
            }

            GatherAIActions.AssignGatherTask(
                world,
                villager,
                resource);
        }
    }
}