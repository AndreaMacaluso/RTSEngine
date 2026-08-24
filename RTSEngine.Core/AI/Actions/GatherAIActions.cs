using RTSEngine.Core.Commands;
using RTSEngine.Core.Entities.Resources;
using RTSEngine.Core.Entities.Units;

namespace RTSEngine.Core.AI.Actions;

public static class GatherAIActions
{
    public static void AssignGatherTask(
        ICommandQueue commandQueue,
        Unit villager,
        ResourceNode resource)
    {
        commandQueue.Enqueue(new GatherCommand
        {
            UnitIds = [villager.Id],
            ResourceId = resource.Id
        });
    }
}