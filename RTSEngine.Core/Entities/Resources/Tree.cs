namespace RTSEngine.Core.Entities.Resources;

using RTSEngine.Core.Map.Runtime;
public sealed class Tree : ResourceNode
{    public Tree(GridPosition position)
        : base(position, 200, ResourceType.Wood)
    {
    }
}
