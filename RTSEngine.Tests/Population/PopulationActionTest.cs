using RTSEngine.Core.Actions;
using RTSEngine.Tests.TestHelpers;

namespace RTSEngine.Tests.Population;

public class PopulationActionsTests
{
    [Fact]
    public void AddPopulation_ShouldIncreasePopulation()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player = world.GetPlayerById(1)!;

        PopulationActions.AddPopulation(player, 3);

        Assert.Equal(3, player.Population);
    }

    [Fact]
    public void RemovePopulation_ShouldDecreasePopulation()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player = world.GetPlayerById(1)!;

        player.Population = 5;

        PopulationActions.RemovePopulation(player, 2);

        Assert.Equal(3, player.Population);
    }

    [Fact]
    public void IncreaseCap_ShouldIncreasePopulationCap()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player = world.GetPlayerById(1)!;

        player.PopulationCap = 5;

        PopulationActions.IncreaseCap(player, 5);

        Assert.Equal(10, player.PopulationCap);
    }

    [Fact]
    public void IncreaseCap_ShouldNotExceedMaxPopulation()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player = world.GetPlayerById(1)!;

        player.PopulationCap = 74;

        PopulationActions.IncreaseCap(player, 10);

        Assert.Equal(
            PopulationActions.MaxPopulation,
            player.PopulationCap);
    }

    [Fact]
    public void DecreaseCap_ShouldDecreasePopulationCap()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player = world.GetPlayerById(1)!;

        player.PopulationCap = 10;

        PopulationActions.DecreaseCap(player, 4);

        Assert.Equal(6, player.PopulationCap);
    }

    [Fact]
    public void DecreaseCap_ShouldNotGoBelowZero()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player = world.GetPlayerById(1)!;

        player.PopulationCap = 2;

        PopulationActions.DecreaseCap(player, 10);

        Assert.Equal(0, player.PopulationCap);
    }

    [Fact]
    public void CanAddPopulation_ShouldReturnTrue_WhenEnoughCapacity()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player = world.GetPlayerById(1)!;

        player.Population = 4;
        player.PopulationCap = 5;

        Assert.True(
            PopulationActions.CanAddPopulation(player, 1));
    }

    [Fact]
    public void CanAddPopulation_ShouldReturnFalse_WhenCapReached()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player = world.GetPlayerById(1)!;

        player.Population = 5;
        player.PopulationCap = 5;

        Assert.False(
            PopulationActions.CanAddPopulation(player, 1));
    }

    [Fact]
    public void CanAddPopulation_ShouldReturnFalse_WhenHardLimitReached()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player = world.GetPlayerById(1)!;

        player.Population = PopulationActions.MaxPopulation;
        player.PopulationCap = PopulationActions.MaxPopulation;

        Assert.False(
            PopulationActions.CanAddPopulation(player, 1));
    }
}