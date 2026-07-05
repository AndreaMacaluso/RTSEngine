using RTSEngine.Core.Entities.Resources;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Helpers;
using RTSEngine.Tests.TestHelpers;

namespace RTSEngine.Tests.Systems;

public class WorldQueriesTests
{
    [Fact]
    public void IsAdjacent_ShouldReturnTrue_ForAdjacentTiles()
    {
        var a = new GridPosition(5, 5);
        var b = new GridPosition(6, 5);

        Assert.True(WorldQueries.IsAdjacent(a, b));
    }

    [Fact]
    public void IsAdjacent_ShouldReturnFalse_ForSameTile()
    {
        var position = new GridPosition(5, 5);

        Assert.False(WorldQueries.IsAdjacent(position, position));
    }

    [Fact]
    public void IsAdjacent_ShouldReturnFalse_ForFarTiles()
    {
        var a = new GridPosition(5, 5);
        var b = new GridPosition(8, 8);

        Assert.False(WorldQueries.IsAdjacent(a, b));
    }

    [Fact]
    public void FindAdjacentWalkableTile_ShouldReturnAdjacentTile()
    {
        var world = TestWorldFactory.CreateWorld();

        var resource = new Tree(new GridPosition(10, 10));

        world.AddResource(resource);

        var result =
            WorldQueries.FindAdjacentWalkableTile(
                world,
                resource.Position);

        Assert.NotNull(result);

        Assert.True(
            WorldQueries.IsAdjacent(
                result!.Value,
                resource.Position));
    }

    [Fact]
    public void FindClosestResource_ShouldReturnNearestResource()
    {
        var world = TestWorldFactory.CreateWorld();

        var tree = new Tree(new GridPosition(5, 5));
        var gold = new GoldMine(new GridPosition(20, 20));

        world.AddResource(tree);
        world.AddResource(gold);

        var result =
            WorldQueries.FindClosestResource(
                world,
                new GridPosition(3, 3));

        Assert.Equal(tree.Id, result!.Id);
    }

    [Fact]
    public void FindClosestResourceByType_ShouldReturnCorrectType()
    {
        var world = TestWorldFactory.CreateWorld();

        var tree = new Tree(new GridPosition(5, 5));
        var gold = new GoldMine(new GridPosition(6, 6));

        world.AddResource(tree);
        world.AddResource(gold);
        
        var result =
            WorldQueries.FindClosestResource(
                world,
                new GridPosition(3, 3),
                ResourceType.Gold);
        Assert.NotNull(result);
        Assert.Equal(gold.Id, result!.Id);
    }

    [Fact]
    public void FindResourcesNear_ShouldReturnOnlyNearbyResources()
    {
        var world = TestWorldFactory.CreateWorld();

        var near = new Tree(new GridPosition(5, 5));
        var far = new Tree(new GridPosition(30, 30));

        world.AddResource(near);
        world.AddResource(far);

        var resources =
            WorldQueries.FindResourcesNear(
                world,
                new GridPosition(4, 4),
                5);

        Assert.Single(resources);
    }

    [Fact]
    public void FindClosestDeposit_ShouldReturnNearestBuilding()
    {
        var world = TestWorldFactory.CreateWorld();

        var definition = TestDefinitionFactory.CreateTownCenter();

        var near = BuildingFactory.Create(
            definition,
            1,
            new GridPosition(5, 5));

        var far = BuildingFactory.Create(
            definition,
            1,
            new GridPosition(20, 20));

        world.AddEntity(near);
        world.AddEntity(far);

        var result = WorldQueries.FindClosestDeposit(
            world,
            new GridPosition(8, 8),
            ResourceType.Wood);

        Assert.Equal(near.Id, result!.Id);
    }

    [Fact]
    public void DistanceSquared_ShouldReturnCorrectDistance()
    {
        var a = new GridPosition(0, 0);
        var b = new GridPosition(3, 4);

        Assert.Equal(25,
            WorldQueries.DistanceSquared(a, b));
    }
}
