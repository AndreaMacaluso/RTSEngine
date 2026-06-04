using RTSEngine.Core.Map.Runtime;
namespace RTSEngine.Core.Players;
public class Player
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public ConsoleColor Color { get; set; }

    public int Wood { get; private set; }
    public int Food { get; private set; }
    public int Gold { get; private set; }
    public int Stone { get; private set; }

    public void AddResource(
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

    public bool SpendResource(
    ResourceType type,
    int amount)
    {
        if (!HasResource(type, amount))
        {
            return false;
        }

        switch(type)
        {
            case ResourceType.Wood:
                Wood -= amount;
                break;

            case ResourceType.Food:
                Food -= amount;
                break;

            case ResourceType.Gold:
                Gold -= amount;
                break;

            case ResourceType.Stone:
                Stone -= amount;
                break;
        }

        return true;
    }

    public bool HasResource(
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
}