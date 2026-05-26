namespace RTSEngine.Core.Entities.Resources;

using RTSEngine.Core.Map.Runtime;

public sealed class StoneMine : ResourceNode
{
    public StoneMine(GridPosition position)
        : base(position, 300, ResourceType.Stone)
    {
    }
}