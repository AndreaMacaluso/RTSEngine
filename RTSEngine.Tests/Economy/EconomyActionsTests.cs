using System;
using RTSEngine.Core.Actions;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Players;

namespace RTSEngine.Tests.Actions;

public class EconomyActionsTests
{
    [Theory]
    [Trait("Category", "Economy")]
    [InlineData(100, 50, 50, 25, true)]
    [InlineData(20, 0, 50, 0, false)]
    public void CanAfford_ShouldReturnCorrectResult(
        int woodAmount, int goldAmount, int woodCost, int goldCost, bool expected)
    {
        var player = new Player(1, "", ConsoleColor.Gray, PlayerControllerType.Human);

        if (woodAmount > 0) player.Economy.Add(ResourceType.Wood, woodAmount);
        if (goldAmount > 0) player.Economy.Add(ResourceType.Gold, goldAmount);

        var costs = new List<ResourceCost>();
        if (woodCost > 0) costs.Add(new(ResourceType.Wood, woodCost));
        if (goldCost > 0) costs.Add(new(ResourceType.Gold, goldCost));

        Assert.Equal(expected, EconomyActions.CanAfford(player, costs));
    }

    [Theory]
    [Trait("Category", "Economy")]
    [InlineData(100, 40, true, 60)]
    [InlineData(20, 50, false, 20)]
    public void TryPay_ShouldReturnResultAndAdjustResources(
        int initialWood, int woodCost, bool expectedResult, int expectedRemaining)
    {
        var player = new Player(1, "", ConsoleColor.Gray, PlayerControllerType.Human);

        player.Economy.Add(ResourceType.Wood, initialWood);

        var costs = new List<ResourceCost>
        {
            new(ResourceType.Wood, woodCost),
        };

        var result = EconomyActions.TryPay(player, costs);

        Assert.Equal(expectedResult, result);
        Assert.Equal(expectedRemaining, player.Economy.Get(ResourceType.Wood));
    }

    [Fact]
    [Trait("Category", "Economy")]
    public void Refund_ShouldRestoreResources()
    {
        var player = new Player(1, "", ConsoleColor.Gray, PlayerControllerType.Human);

       var costs = new List<ResourceCost>
        {
            new(ResourceType.Wood, 40),
            new(ResourceType.Gold, 10)
        };

        EconomyActions.Refund(player, costs);

        Assert.Equal(40, player.Economy.Get(ResourceType.Wood));
        Assert.Equal(10, player.Economy.Get(ResourceType.Gold));
    }
}
