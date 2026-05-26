namespace RTSEngine.Core.Entities.Resources;

using RTSEngine.Core.Map.Runtime;

public sealed class GoldMine : ResourceNode
{
    public override bool IsBlocking => true;
    public GoldMine(GridPosition position)
        : base(position, 450, ResourceType.Gold)
    {
    }
}