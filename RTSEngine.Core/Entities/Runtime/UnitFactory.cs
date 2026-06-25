using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Entities.Units;

namespace RTSEngine.Core.Entities.Runtime;

public static class UnitFactory
{
    public static Unit Create(
        UnitDefinition definition,
        int ownerId,
        GridPosition position)
    {
        return new RuntimeUnit(
            definition,
            ownerId,
            position);
    }
}