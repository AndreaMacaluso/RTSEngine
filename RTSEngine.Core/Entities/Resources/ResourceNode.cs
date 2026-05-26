namespace RTSEngine.Core.Entities.Resources;

using RTSEngine.Core.Map.Runtime;

abstract public class ResourceNode : Entity
{
    public int Amount { get; set; }

    public ResourceType ResourceType { get; set; }

    public bool IsDepleted => Amount <= 0;

    
    protected ResourceNode(
        GridPosition position,
        int amount,
        ResourceType resourceType)
    {
        Position = position;
        Amount = amount;
        ResourceType = resourceType;
    }

    public void Gather(int amount)
    {
        Amount -= amount;

        if (Amount < 0)
        {
            Amount = 0;
        }
    }
}
