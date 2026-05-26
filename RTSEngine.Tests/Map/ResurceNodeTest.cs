using RTSEngine.Core.Entities.Resources;
using RTSEngine.Core.Map.Runtime;

namespace RTSEngine.Tests.Entities;

public class ResourceNodeTest
{
    [Fact]
    public void Tree_ShouldBeBlocking()
    {
        // Arrange
        var tree = new Tree(new GridPosition(0, 0));

        // Assert
        Assert.True(tree.IsBlocking);
    }

    [Fact]
    public void GoldMine_ShouldBeBlocking()
    {
        // Arrange
        var goldMine = new GoldMine(new GridPosition(0, 0));

        // Assert
        Assert.True(goldMine.IsBlocking);
    }

    [Fact]
    public void BerryBush_ShouldBeBlocking()
    {
        // Arrange
        var berryBush = new BerryBush(new GridPosition(0, 0));

        // Assert
        Assert.True(berryBush.IsBlocking);
    }

    [Fact]
    public void StoneMine_ShouldBeBlocking()
    {
        // Arrange
        var stoneMine = new StoneMine(new GridPosition(0, 0));

        // Assert
        Assert.True(stoneMine.IsBlocking);
    }
}