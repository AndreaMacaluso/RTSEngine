namespace RTSEngine.Core.Entities.Resources;

using RTSEngine.Core.Map.Runtime;

public sealed class BerryBush : ResourceNode
{
    public BerryBush(GridPosition position)
        : base(position, 250, ResourceType.Food)
    {
    }
}