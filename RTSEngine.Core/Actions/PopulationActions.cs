using RTSEngine.Core.Players;

namespace RTSEngine.Core.Actions;

public static class PopulationActions
{
    public const int MaxPopulation = 75;

    public static bool CanAddPopulation(
        Player player,
        int amount)
    {
        return
            player.Population.Current + amount <= player.Population.Capacity &&
            player.Population.Current + amount <= MaxPopulation;
    }

    public static void AddPopulation(
        Player player,
        int amount)
    {
        player.Population.Current += amount;
    }

    public static void RemovePopulation(
        Player player,
        int amount)
    {
        player.Population.Current -= amount;
    }

    public static void IncreaseCap(
        Player player,
        int amount)
    {
        player.Population.Capacity =
            Math.Min(
                player.Population.Capacity + amount,
                MaxPopulation);
    }

    public static void DecreaseCap(
        Player player,
        int amount)
    {
        player.Population.Capacity =
            Math.Max(
                0,
                player.Population.Capacity - amount);
    }
}