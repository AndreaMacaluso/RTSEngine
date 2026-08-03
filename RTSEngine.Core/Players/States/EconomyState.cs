using RTSEngine.Core.Map.Runtime;
namespace RTSEngine.Core.Players.States;

public sealed class EconomyState
{
    public int Wood {get; private set;}

    public int Food {get; private set;}

    public int Gold {get; private set;}

    public int Stone {get; private set;}


    public void Add(
        ResourceType type,
        int amount)
    {
        switch(type)
        {
            case ResourceType.Wood:
                Wood += amount;
                break;

            case ResourceType.Food:
                Food += amount;
                break;

            case ResourceType.Gold:
                Gold += amount;
                break;

            case ResourceType.Stone:
                Stone += amount;
                break;
        }
    }


    public bool Has(
        ResourceType type,
        int amount)
    {
        return type switch
        {
            ResourceType.Wood => Wood >= amount,
            ResourceType.Food => Food >= amount,
            ResourceType.Gold => Gold >= amount,
            ResourceType.Stone => Stone >= amount,
            _ => false
        };
    }


    public bool Spend(
        ResourceType type,
        int amount)
    {
        if(!Has(type, amount))
            return false;


        Add(type,-amount);

        return true;
    }
}