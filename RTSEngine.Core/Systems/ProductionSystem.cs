using RTSEngine.Core.Actions;
using RTSEngine.Core.Entities.Runtime;

namespace RTSEngine.Core.Systems;

public static class ProductionSystem
{
    public static void Update(
        RuntimeContext context)
    {
        var buildings = context.World.GetBuildings().ToList();

        foreach(var building in buildings)
        {
            ProductionActions.ProduceOneTick(
                context,
                building);
        }
    }
}