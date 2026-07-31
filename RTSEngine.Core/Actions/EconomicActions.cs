using RTSEngine.Core.Players;
using  RTSEngine.Core.Map.Runtime;

namespace RTSEngine.Core.Actions;

public static class EconomyActions
{
    public static bool CanAfford(
        Player player,
        IEnumerable<ResourceCost> costs)
    {
        foreach (var cost in costs)
        {
            if (!player.HasResource(cost.Type, cost.Amount))
            {
                return false;
            }
        }

        return true;
    }

    public static bool TryPay(
        Player player,
        IEnumerable<ResourceCost> costs)
    {
        if (!CanAfford(player, costs))
        {
            return false;
        }

        foreach (var cost in costs)
        {
            player.SpendResource(cost.Type, cost.Amount);
        }

        return true;
    }

    public static void Refund(
        Player player,
        IEnumerable<ResourceCost> costs)
    {
        foreach (var cost in costs)
        {
            player.AddResource(cost.Type, cost.Amount);
        }
    }
}