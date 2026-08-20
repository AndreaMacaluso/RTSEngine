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
            player.Population.Current + player.Population.Reserved + amount <= player.Population.Capacity &&
            player.Population.Current + player.Population.Reserved + amount <= MaxPopulation;
    }

    public static void AddPopulation(
        Player player,
        int amount)
    {
        player.Population.Current += amount;
    }

    public static bool TryReservePopulation(
        Player player,
        int amount)
    {
        if (!CanAddPopulation(player, amount))
        {
            return false;
        }

        player.Population.Reserved += amount;
        return true;
    }

    public static void CompleteReservedPopulation(
        Player player,
        int amount)
    {
        if (amount > player.Population.Reserved)
        {
            throw new InvalidOperationException(
                "Cannot complete more population than is reserved.");
        }

        player.Population.Reserved -= amount;
        player.Population.Current += amount;
    }

    public static void ReleaseReservedPopulation(
        Player player,
        int amount)
    {
        if (amount > player.Population.Reserved)
        {
            throw new InvalidOperationException(
                "Cannot release more population than is reserved.");
        }

        player.Population.Reserved -= amount;
    }

    public static void RemovePopulation(
        Player player,
        int amount)
    {
        player.Population.Current = Math.Max(0, player.Population.Current - amount);
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
