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
            player.Population + amount <= player.PopulationCap &&
            player.Population + amount <= MaxPopulation;
    }

    public static void AddPopulation(
        Player player,
        int amount)
    {
        player.Population += amount;
    }

    public static void RemovePopulation(
        Player player,
        int amount)
    {
        player.Population -= amount;
    }

    public static void IncreaseCap(
        Player player,
        int amount)
    {
        player.PopulationCap =
            Math.Min(
                player.PopulationCap + amount,
                MaxPopulation);
    }

    public static void DecreaseCap(
        Player player,
        int amount)
    {
        player.PopulationCap =
            Math.Max(
                0,
                player.PopulationCap - amount);
    }
}