using RTSEngine.Core.AI.Actions;
using RTSEngine.Core.Helpers;
using RTSEngine.Core.Players;
using RTSEngine.Core.State;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Entities.States;
using RTSEngine.Core.Systems;

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

        AssignDepositsForResourceCarriers(world, player, idleVillagers);

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

                if (villager.Gather.CurrentLoad > 0
                    && villager.Gather.CarriedResource.HasValue)
                {
                    idleIndex++;
                    continue;
                }

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

    private static void AssignDepositsForResourceCarriers(
        GameWorld world,
        Player player,
        List<Unit> idleVillagers)
    {
        foreach (var villager in idleVillagers)
        {
            if (villager.Gather.CurrentLoad <= 0
                || !villager.Gather.CarriedResource.HasValue)
            {
                continue;
            }

            var deposit = WorldQueries.FindClosestDeposit(
                world,
                player.Id,
                villager.Position,
                villager.Gather.CarriedResource.Value);

            if (deposit == null)
            {
                continue;
            }

            var target = WorldQueries.FindClosestAdjacentWalkableTile(
                world,
                villager.Position,
                deposit.Position);

            if (target is not GridPosition destination)
            {
                continue;
            }

            villager.CurrentTask = UnitTask.Gathering;
            villager.Gather.Phase = GatherPhase.MovingToDeposit;
            villager.Gather.DepositPosition = deposit.Position;
            CommandSystem.AssignMoveTarget(villager, destination, world);
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
