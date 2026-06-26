using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Entities.Units;

namespace RTSEngine.Core.Entities.Runtime;

public sealed class RuntimeUnit : Unit
{
    public RuntimeUnit(
        UnitDefinition definition,
        int ownerId,
        GridPosition position)
        : base(
            ownerId,
            position,
            definition)
    {
    }
}