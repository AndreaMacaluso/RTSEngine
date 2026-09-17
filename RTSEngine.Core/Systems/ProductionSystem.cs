using RTSEngine.Core.Actions;
using RTSEngine.Core.Entities.Runtime;

namespace RTSEngine.Core.Systems;

public static class ProductionSystem
{
    public static void Update(
        RuntimeContext context)
    {
        foreach(var building in context.World.Entities.Buildings)
        {
            ProductionActions.ProduceOneTick(
                context,
                building);
        }
    }
}