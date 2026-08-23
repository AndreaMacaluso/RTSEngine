using RTSEngine.Core.Entities.Resources;
using RTSEngine.Core.Map.Definitions;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Map.Loading;
using RTSEngine.Core.State;
using RTSEngine.Core.Systems;
using RTSEngine.Tests.TestHelpers;

namespace RTSEngine.Tests.Resources;

public sealed class ResourceTests
{
    [Fact]
    [Trait("Category", "Resource")]
    public void Tree_ShouldContainWood()
    {
        var tree = new Tree(new GridPosition(2, 3));

        Assert.Equal(ResourceType.Wood, tree.ResourceType);
        Assert.True(tree.Amount > 0);
    }

    [Fact]
    [Trait("Category", "Resource")]
    public void GoldMine_ShouldContainGold()
    {
        var goldMine = new GoldMine(new GridPosition(4, 1));

        Assert.Equal(ResourceType.Gold, goldMine.ResourceType);
        Assert.True(goldMine.Amount > 0);
    }

    [Fact]
    [Trait("Category", "Resource")]
    public void StoneMine_ShouldContainStone()
    {
        var stoneMine = new StoneMine(new GridPosition(4, 1));

        Assert.Equal(ResourceType.Stone, stoneMine.ResourceType);
        Assert.True(stoneMine.Amount > 0);
    }

    [Fact]
    [Trait("Category", "Resource")]
    public void BerryBush_ShouldContainFood()
    {
        var berryBush = new BerryBush(new GridPosition(4, 1));

        Assert.Equal(ResourceType.Food, berryBush.ResourceType);
        Assert.True(berryBush.Amount > 0);
    }

    [Fact]
    [Trait("Category", "Resource")]
    public void ResourceFactory_ShouldCreateTree()
    {
        var definition = new ResourceDefinition
        {
            Type = "tree",
            X = 1,
            Y = 2
        };

        var resource = ResourceFactory.Create(definition);

        Assert.IsType<Tree>(resource);
    }

    [Fact]
    [Trait("Category", "Resource")]
    public void ResourceFactory_ShouldCreateGoldMine()
    {
        var definition = new ResourceDefinition
        {
            Type = "gold_mine",
            X = 8,
            Y = 5
        };

        var resource = ResourceFactory.Create(definition);

        Assert.IsType<GoldMine>(resource);
    }

    [Fact]
    [Trait("Category", "Resource")]
    public void ResourceFactory_ShouldThrow_WhenTypeIsInvalid()
    {
        var definition = new ResourceDefinition
        {
            Type = "invalid_resource",
            X = 0,
            Y = 0
        };

        Assert.Throws<Exception>(() =>
        {
            ResourceFactory.Create(definition);
        });
    }

    [Fact]
    [Trait("Category", "Resource")]
    [Trait("Category", "Resource.Runtime")]
    public void Gather_ShouldDecreaseAmount()
    {
        var tree = new Tree(new GridPosition(1, 1));
        int initialAmount = tree.Amount;

        tree.Gather(10);

        Assert.Equal(initialAmount - 10, tree.Amount);
    }

    [Fact]
    [Trait("Category", "Resource")]
    [Trait("Category", "Resource.Runtime")]
    public void Gather_ShouldNotGoBelowZero()
    {
        var tree = new Tree(new GridPosition(1, 1));

        tree.Gather(tree.Amount + 50);

        Assert.Equal(0, tree.Amount);
    }

    [Fact]
    [Trait("Category", "Resource")]
    [Trait("Category", "Resource.Runtime")]
    public void IsDepleted_ShouldBeTrue_WhenAmountIsZero()
    {
        var tree = new Tree(new GridPosition(1, 1));

        tree.Gather(tree.Amount);

        Assert.True(tree.IsDepleted);
    }

    [Fact]
    [Trait("Category", "Resource")]
    [Trait("Category", "Resource.Runtime")]
    public void IsDepleted_ShouldBeFalse_WhenAmountIsPositive()
    {
        var tree = new Tree(new GridPosition(1, 1));

        Assert.False(tree.IsDepleted);
    }

    [Fact]
    [Trait("Category", "Resource")]
    [Trait("Category", "Resource.Runtime")]
    public void IsBlocking_ShouldBeFalse_WhenDepleted()
    {
        var tree = new Tree(new GridPosition(1, 1));

        tree.Gather(tree.Amount);

        Assert.False(tree.IsBlocking);
    }

    [Fact]
    [Trait("Category", "Resource")]
    [Trait("Category", "Resource.Runtime")]
    public void IsBlocking_ShouldBeTrue_WhenNotDepleted()
    {
        var tree = new Tree(new GridPosition(1, 1));

        Assert.True(tree.IsBlocking);
    }

    [Fact]
    [Trait("Category", "Resource")]
    [Trait("Category", "Resource.Runtime")]
    public void ResourceCleanup_ShouldRemoveDepletedResources()
    {
        var world = TestWorldFactory.CreateWorld();

        var tree = new Tree(new GridPosition(1, 1));
        world.AddResource(tree);

        var goldMine = new GoldMine(new GridPosition(2, 1));
        world.AddResource(goldMine);

        Assert.Equal(2, world.Resources.Count);

        tree.Gather(tree.Amount);

        ResourceCleanupSystem.Update(world);

        Assert.Single(world.Resources);
        Assert.Contains(world.Resources, r => r.Id == goldMine.Id);
    }

    [Fact]
    [Trait("Category", "Resource")]
    [Trait("Category", "Resource.Runtime")]
    public void ResourceCleanup_ShouldNotRemoveActiveResources()
    {
        var world = TestWorldFactory.CreateWorld();

        var tree = new Tree(new GridPosition(1, 1));
        world.AddResource(tree);

        ResourceCleanupSystem.Update(world);

        Assert.Single(world.Resources);
    }

    [Fact]
    [Trait("Category", "Resource")]
    [Trait("Category", "Resource.Runtime")]
    public void ResourceCleanup_ShouldHandleMultipleDepletedResources()
    {
        var world = TestWorldFactory.CreateWorld();

        var tree1 = new Tree(new GridPosition(1, 1));
        var tree2 = new Tree(new GridPosition(2, 1));
        var tree3 = new Tree(new GridPosition(3, 1));
        world.AddResource(tree1);
        world.AddResource(tree2);
        world.AddResource(tree3);

        Assert.Equal(3, world.Resources.Count);

        tree1.Gather(tree1.Amount);
        tree3.Gather(tree3.Amount);

        ResourceCleanupSystem.Update(world);

        Assert.Single(world.Resources);
        Assert.Contains(world.Resources, r => r.Id == tree2.Id);
    }

    [Fact]
    [Trait("Category", "Resource")]
    [Trait("Category", "Resource.Runtime")]
    public void Tree_ShouldStartWith200Amount()
    {
        var tree = new Tree(new GridPosition(1, 1));

        Assert.Equal(200, tree.Amount);
    }
}