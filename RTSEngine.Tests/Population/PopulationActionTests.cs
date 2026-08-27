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

        Assert.Equal(3, player.Population.Current );
    }

    [Theory]
    [InlineData(5, 5, 10)]
    [InlineData(74, 10, 84)]
    public void IncreaseCap_ShouldIncreasePopulationCap(int initialCap, int increase, int expected)
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player = world.GetPlayerById(1)!;

        player.Population.Capacity = initialCap;

        PopulationActions.IncreaseCap(player, increase);

        Assert.Equal(expected, player.Population.Capacity);
    }

    [Theory]
    [InlineData(10, 4, 6)]
    [InlineData(2, 10, 0)]
    public void DecreaseCap_ShouldDecreasePopulationCap(int initialCap, int decrease, int expected)
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player = world.GetPlayerById(1)!;

        player.Population.Capacity = initialCap;

        PopulationActions.DecreaseCap(player, decrease);

        Assert.Equal(expected, player.Population.Capacity);
    }

    [Theory]
    [InlineData(4, 5, 1, true)]
    [InlineData(5, 5, 1, false)]
    public void CanAddPopulation_ShouldReturnCorrectResult(
        int currentPop, int capacity, int addCount, bool expected)
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player = world.GetPlayerById(1)!;

        player.Population.Current  = currentPop;
        player.Population.Capacity = capacity;

        Assert.Equal(expected,
            PopulationActions.CanAddPopulation(player, addCount));
    }

    [Theory]
    [InlineData(5, 2, 3)]
    [InlineData(2, 5, 0)]
    public void RemovePopulation_ShouldDecreasePopulation(int initialPop, int removeCount, int expected)
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player = world.GetPlayerById(1)!;

        player.Population.Current = initialPop;

        PopulationActions.RemovePopulation(player, removeCount);

        Assert.Equal(expected, player.Population.Current);
    }
}
