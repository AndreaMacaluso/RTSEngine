using RTSEngine.Core.Commands;
using RTSEngine.Core.Entities.Resources;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.State;

namespace RTSEngine.Core.AI.Actions;

public static class GatherAIActions
{
    public static void AssignGatherTask(
        GameWorld world,
        Unit villager,
        ResourceNode resource)
    {
        world.AddCommand(new GatherCommand
        {
            UnitIds = [villager.Id],
            ResourceId = resource.Id
        });
    }
}