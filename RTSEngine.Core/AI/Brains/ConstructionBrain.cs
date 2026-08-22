using RTSEngine.Core.AI.Actions;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Helpers;
using RTSEngine.Core.Players;
using RTSEngine.Core.State;

namespace RTSEngine.Core.AI.Brains;

public class ConstructionBrain : AIBrain
{
    protected override string Think(RuntimeContext context, Player player)
    {
        if (player.Population.Current >= GameConfig.TargetPopulation
            && !WorldQueries.HasBuilding(context.World, player, EntityIds.Barracks))
        {
            return BrainActions.BuildBarracks;
        }

        if (NeedMorePopulation(player))
        {
            return BrainActions.BuildHouse;
        }

        return BrainActions.None;
    }

    protected override void ExecutePlan(RuntimeContext context, Player player, string action)
    {
        if (action == BrainActions.None) return;

        var definitionKey = BrainActions.GetDefinition(action);
        if (definitionKey != null)
        {
            ConstructionAIActions.RequestConstruction(context, player, definitionKey);
        }
    }

    private static bool NeedMorePopulation(Player player)
    {
        return player.Population.Current + GameConfig.PopulationBuffer >= player.Population.Capacity;
    }
}
