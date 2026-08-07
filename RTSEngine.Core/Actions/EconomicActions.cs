using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Players;


namespace RTSEngine.Core.Actions;

public static class EconomyActions
{
    public static bool CanAfford(
        Player player,
        IEnumerable<ResourceCost> costs)
    {
        foreach(var cost in costs)
        {
            if(!player.Economy.Has(
                cost.Type,
                cost.Amount))
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
        if(!CanAfford(player,costs))
        {
            return false;
        }


        foreach(var cost in costs)
        {
            player.Economy.Spend(
                cost.Type,
                cost.Amount);
        }


        return true;
    }


    public static void Refund(
        Player player,
        IEnumerable<ResourceCost> costs)
    {
        foreach(var cost in costs)
        {
            player.Economy.Add(
                cost.Type,
                cost.Amount);
        }
    }


    public static bool CanPayUnit(
        Player player,
        UnitDefinition definition)
    {
        return CanAfford(
            player,
            definition.Costs);
    }
}