using System;
using RTSEngine.Core.Actions;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Players;

namespace RTSEngine.Tests.Actions;

public class EconomyActionsTests
{
    [Fact]
    [Trait("Category", "Economy")]
    public void CanAfford_ShouldReturnTrue_WhenPlayerHasResources()
    {
        var player = new Player(1, "", ConsoleColor.Gray, PlayerControllerType.Human);

        player.Economy.Add(ResourceType.Wood, 100);
        player.Economy.Add(ResourceType.Gold, 50);

        var costs = new List<ResourceCost>
        {
            new(ResourceType.Wood, 50),
            new(ResourceType.Gold, 25)
        };
        Assert.True(EconomyActions.CanAfford(player, costs));
    }

    [Fact]
    [Trait("Category", "Economy")]
    public void CanAfford_ShouldReturnFalse_WhenPlayerLacksResources()
    {
        var player = new Player(1, "", ConsoleColor.Gray, PlayerControllerType.Human);

        player.Economy.Add(ResourceType.Wood, 20);

        var costs = new List<ResourceCost>
        {
            new(ResourceType.Wood, 50),
        };

        Assert.False(EconomyActions.CanAfford(player, costs));
    }

    [Fact]
    [Trait("Category", "Economy")]
    public void TryPay_ShouldRemoveResources()
    {
        var player = new Player(1, "", ConsoleColor.Gray, PlayerControllerType.Human);

        player.Economy.Add(ResourceType.Wood, 100);

        var costs = new List<ResourceCost>
        {
            new(ResourceType.Wood, 40),
        };

        var result = EconomyActions.TryPay(player, costs);

        Assert.True(result);
        Assert.Equal(60, player.Economy.Get(ResourceType.Wood));
    }

    [Fact]
    [Trait("Category", "Economy")]
    public void TryPay_ShouldNotRemoveResources_WhenCannotAfford()
    {
        var player = new Player(1, "", ConsoleColor.Gray, PlayerControllerType.Human);

        player.Economy.Add(ResourceType.Wood, 20);

        var costs = new List<ResourceCost>
        {
            new(ResourceType.Wood, 50),
        };

        var result = EconomyActions.TryPay(player, costs);

        Assert.False(result);
        Assert.Equal(20, player.Economy.Get(ResourceType.Wood));
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
