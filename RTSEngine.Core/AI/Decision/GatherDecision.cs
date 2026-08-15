using RTSEngine.Core.AI.Actions;
using RTSEngine.Core.Helpers;
using RTSEngine.Core.Players;
using RTSEngine.Core.State;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Entities.Units;

namespace RTSEngine.Core.AI.Decisions;

public static class GatherDecision
{
    private const int ResourceTarget = 500;
    private const int MinVillagersPerResource = 2;

    private static readonly ResourceType[] PriorityOrder =
    [
        ResourceType.Food,
        ResourceType.Wood,
        ResourceType.Gold,
        ResourceType.Stone
    ];

    public static void Execute(
        GameWorld world,
        Player player)
    {
        var idleVillagers = UnitQueries
            .FindIdleVillagers(world, player)
            .ToList();

        int idleIndex = 0;

        foreach (var resourceType in PriorityOrder)
        {
            if (player.Economy.Get(resourceType) >= ResourceTarget)
            {
                continue;
            }

            int currentCount = CountGatherers(world, player, resourceType);

            while (currentCount < MinVillagersPerResource
                   && idleIndex < idleVillagers.Count)
            {
                var villager = idleVillagers[idleIndex];

                var resource = WorldQueries.FindClosestResource(
                    world,
                    villager.Position,
                    resourceType);

                if (resource == null)
                {
                    break;
                }

                GatherAIActions.AssignGatherTask(
                    world,
                    villager,
                    resource);

                currentCount++;
                idleIndex++;
            }
        }
    }

    private static int CountGatherers(
        GameWorld world,
        Player player,
        ResourceType resourceType)
    {
        return world.Entities
            .OfType<Unit>()
            .Count(unit =>
                unit.OwnerId == player.Id &&
                unit.CurrentTask == UnitTask.Gathering &&
                unit.Gather.CarriedResource == resourceType);
    }
}
