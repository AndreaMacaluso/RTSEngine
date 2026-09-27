using RTSEngine.Core.Actions;
using RTSEngine.Core.AI.Actions;
using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Helpers;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Players;
using RTSEngine.Core.State;

namespace RTSEngine.Core.AI.Brains;

public class ProductionBrain : AIBrain
{
    protected override string Think(RuntimeContext context, Player player)
    {
        var tc = WorldQueries.FindBuilding(context.World, player, EntityIds.TownCenter);
        if (tc == null) return BrainActions.None;

        if (player.Population.Current >= GameConfig.TargetPopulation)
        {
            return ThinkMilitia(context, player);
        }

        return ThinkVillager(context, player, tc);
    }

    private string ThinkMilitia(RuntimeContext context, Player player)
    {
        var barracks = WorldQueries.FindBuilding(context.World, player, EntityIds.Barracks);
        if (barracks == null) return BrainActions.None;

        int militiaCount = UnitQueries.CountUnits(context.World, player, EntityIds.Militia);
        if (militiaCount >= GameConfig.TargetMilitiaCount) return BrainActions.None;
        if (barracks.Production.IsProducing) return BrainActions.None;

        var militiaDef = context.UnitRepository.Get(EntityIds.Militia);
        if (!EconomyActions.CanAfford(player, militiaDef.Costs)) return BrainActions.None;

        return BrainActions.TrainMilitia;
    }

    private string ThinkVillager(RuntimeContext context, Player player, Building townCenter)
    {
        if (player.Population.Current >= player.Population.Capacity) return BrainActions.None;
        if (townCenter.Production.IsProducing) return BrainActions.None;

        var villagerDef = context.UnitRepository.Get(EntityIds.Villager);
        if (!EconomyActions.CanAfford(player, villagerDef.Costs)) return BrainActions.None;

        return BrainActions.TrainVillager;
    }

    protected override void ExecutePlan(RuntimeContext context, Player player, string action)
    {
        switch (action)
        {
            case BrainActions.TrainVillager:
                var tc = WorldQueries.FindBuilding(context.World, player, EntityIds.TownCenter);
                if (tc != null) ProductionAIActions.TrainVillager(context, tc);
                break;

            case BrainActions.TrainMilitia:
                var barracks = WorldQueries.FindBuilding(context.World, player, EntityIds.Barracks);
                if (barracks != null) ProductionAIActions.TrainMilitia(context, barracks);
                break;
        }
    }
}
