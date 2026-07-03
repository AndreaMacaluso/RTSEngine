using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Entities.Definitions;

namespace RTSEngine.Core.Entities.Runtime;
public static class BuildingFactory
{
    public static Building Create(
        BuildingDefinition definition,
        int ownerId,
        GridPosition position)
    {
        return new Building(
            ownerId,
            position,
            definition);
    }
}