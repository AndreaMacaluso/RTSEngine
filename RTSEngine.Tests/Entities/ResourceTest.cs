using RTSEngine.Core.Entities.Resources;
using RTSEngine.Core.Map.Definitions;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Map.Loading;

namespace RTSEngine.Tests.Resources;

public sealed class ResourceTests
{
    [Fact]
    public void Tree_ShouldContainWood()
    {
        // Arrange
        var tree = new Tree(
            new GridPosition(2, 3));

        // Assert
        Assert.Equal(
            ResourceType.Wood,
            tree.ResourceType);

        Assert.True(tree.Amount > 0);
    }

    [Fact]
    public void GoldMine_ShouldContainGold()
    {
        // Arrange
        var goldMine = new GoldMine(
            new GridPosition(4, 1));

        // Assert
        Assert.Equal(
            ResourceType.Gold,
            goldMine.ResourceType);

        Assert.True(goldMine.Amount > 0);
    }

    [Fact]
    public void ResourceFactory_ShouldCreateTree()
    {
        // Arrange
        var definition = new ResourceDefinition
        {
            Type = "tree",

            X = 1,
            Y = 2
        };

        // Act
        var resource =
            ResourceFactory.Create(definition);

        // Assert
        Assert.IsType<Tree>(resource);
    }

    [Fact]
    public void ResourceFactory_ShouldCreateGoldMine()
    {
        // Arrange
        var definition = new ResourceDefinition
        {
            Type = "gold_mine",

            X = 8,
            Y = 5
        };

        // Act
        var resource =
            ResourceFactory.Create(definition);

        // Assert
        Assert.IsType<GoldMine>(resource);
    }

    [Fact]
    public void ResourceFactory_ShouldThrow_WhenTypeIsInvalid()
    {
        // Arrange
        var definition = new ResourceDefinition
        {
            Type = "invalid_resource",

            X = 0,
            Y = 0
        };

        // Act & Assert
        Assert.Throws<Exception>(() =>
        {
            ResourceFactory.Create(definition);
        });
    }
}