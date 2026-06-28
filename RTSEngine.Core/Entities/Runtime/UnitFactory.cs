using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Map.Runtime;
namespace RTSEngine.Core.Entities.Runtime;

public static class UnitFactory
{
    public static Unit Create(
        UnitDefinition definition,
        int ownerId,
        GridPosition position)
    {
        return new Unit(
            ownerId,
            position,
            definition);
    }
}