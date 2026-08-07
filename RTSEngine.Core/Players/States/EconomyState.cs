using RTSEngine.Core.Map.Runtime;
namespace RTSEngine.Core.Players.States;

public sealed class EconomyState
{
    private readonly Dictionary<ResourceType,int> _resources = new();


    public int Get(ResourceType type)
    {
        return _resources.GetValueOrDefault(type);
    }


    public void Add(
        ResourceType type,
        int amount)
    {
        if(!_resources.ContainsKey(type))
        {
            _resources[type] = 0;
        }

        _resources[type] += amount;
    }


    public bool Has(
        ResourceType type,
        int amount)
    {
        return Get(type) >= amount;
    }


    public void Spend(
        ResourceType type,
        int amount)
    {
        if(!Has(type, amount))
        {
            return;
        }

        _resources[type] -= amount;
    }
}
