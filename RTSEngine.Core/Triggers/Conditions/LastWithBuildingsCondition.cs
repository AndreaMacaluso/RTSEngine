using RTSEngine.Core.Entities.Runtime;

namespace RTSEngine.Core.Triggers;

public sealed class LastWithBuildingsCondition : ITriggerCondition
{
    public bool Evaluate(RuntimeContext context)
    {
        var world = context.World;
        int playersWithBuildings = 0;

        foreach (var player in world.Players)
        {
            if (player.BuildingIds.Count > 0)
            {
                playersWithBuildings++;
            }
        }

        return playersWithBuildings == 1;
    }
}
