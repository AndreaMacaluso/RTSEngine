using RTSEngine.Core.AI.Actions;
using RTSEngine.Core.Entities.Resources;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Helpers;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Players;
using RTSEngine.Core.State;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Entities.States;
using RTSEngine.Core.Systems;

namespace RTSEngine.Core.AI.Brains;

public class GatherBrain : AIBrain
{
    private static readonly ResourceType[] PriorityOrder =
    [
        ResourceType.Food,
        ResourceType.Wood,
        ResourceType.Gold,
        ResourceType.Stone
    ];

    private List<(Unit Villager, ResourceNode Resource)> _pendingAssignments = [];

    protected override string Think(RuntimeContext context, Player player)
    {
        var idleVillagers = UnitQueries
            .FindIdleVillagers(context.World, player)
            .ToList();

        if (idleVillagers.Count == 0) return BrainActions.None;

        AssignDepositsForResourceCarriers(context.World, player, idleVillagers);

        _pendingAssignments = FindAllAssignments(context, player, idleVillagers);

        return _pendingAssignments.Count > 0
            ? BrainActions.AssignGatherers
            : BrainActions.None;
    }

    protected override void ExecutePlan(RuntimeContext context, Player player, string action)
    {
        if (action == BrainActions.None) return;

        foreach (var (villager, resource) in _pendingAssignments)
        {
            GatherAIActions.AssignGatherTask(context.World, villager, resource);
        }

        _pendingAssignments.Clear();
    }

    private static List<(Unit Villager, ResourceNode Resource)> FindAllAssignments(
        RuntimeContext context,
        Player player,
        List<Unit> idleVillagers)
    {
        var assignments = new List<(Unit, ResourceNode)>();
        var assignedVillagers = new HashSet<Unit>();

        foreach (var resourceType in PriorityOrder)
        {
            if (player.Economy.Get(resourceType) >= GameConfig.GatherResourceTarget) continue;

            int currentCount = UnitQueries.CountGatherers(context.World, player, resourceType);

            foreach (var villager in idleVillagers)
            {
                if (currentCount >= GameConfig.GatherMinVillagersPerResource) break;
                if (assignedVillagers.Contains(villager)) continue;

                if (villager.Gather.CurrentLoad > 0 && villager.Gather.CarriedResource.HasValue)
                    continue;

                var resource = WorldQueries.FindClosestResource(context.World, villager.Position, resourceType);
                if (resource == null) break;

                assignments.Add((villager, resource));
                assignedVillagers.Add(villager);
                currentCount++;
            }
        }

        return assignments;
    }

    private static void AssignDepositsForResourceCarriers(
        GameWorld world,
        Player player,
        List<Unit> idleVillagers)
    {
        foreach (var villager in idleVillagers)
        {
            if (villager.Gather.CurrentLoad <= 0 || !villager.Gather.CarriedResource.HasValue)
                continue;

            var deposit = WorldQueries.FindClosestDeposit(
                world, player.Id, villager.Position, villager.Gather.CarriedResource.Value);

            if (deposit == null) continue;

            var target = WorldQueries.FindClosestAdjacentWalkableTile(
                world, villager.Position, deposit.Position);

            if (target is not GridPosition destination) continue;

            villager.CurrentTask = UnitTask.Gathering;
            villager.Gather.Phase = GatherPhase.MovingToDeposit;
            villager.Gather.DepositPosition = deposit.Position;
            CommandSystem.AssignMoveTarget(villager, destination, world);
        }
    }
}
