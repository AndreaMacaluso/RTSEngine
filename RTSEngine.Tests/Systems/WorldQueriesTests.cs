using RTSEngine.Core.Entities.Resources;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Helpers;
using RTSEngine.Core.Players;
using RTSEngine.Core.State;
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
    public void FindClosestDeposit_ShouldReturnNearestBuilding()
    {
        var world = TestWorldFactory.CreateWorld();

        var definition = TestDefinitionFactory.CreateTownCenter();

        var near = BuildingFactory.Create(
            definition,
            1,
            new GridPosition(5, 5));
        near.IsCompleted = true;

        var far = BuildingFactory.Create(
            definition,
            1,
            new GridPosition(20, 20));
        far.IsCompleted = true;

        world.AddEntity(near);
        world.AddEntity(far);

        var result = WorldQueries.FindClosestDeposit(
            world,
            1,
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

    [Fact]
    [Trait("Category", "WorldQueries")]
    public void FindBuilding_ShouldRespect_CompletionAndOwnership()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player1 = world.GetPlayerById(1)!;
        var player2 = world.GetPlayerById(2)!;

        var tc1 = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(), player1.Id, new GridPosition(5, 5));
        tc1.IsCompleted = true;
        world.AddEntity(tc1);

        var tc2 = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(), player1.Id, new GridPosition(8, 5));
        tc2.IsCompleted = false;
        world.AddEntity(tc2);

        var tc3 = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(), player2.Id, new GridPosition(5, 8));
        tc3.IsCompleted = true;
        world.AddEntity(tc3);

        Assert.Equal(tc1.Id, WorldQueries.FindBuilding(world, player1, "town_center")!.Id);
        Assert.Null(WorldQueries.FindBuilding(world, player1, "house"));
        Assert.Equal(tc3.Id, WorldQueries.FindBuilding(world, player2, "town_center")!.Id);
    }

    [Fact]
    [Trait("Category", "WorldQueries")]
    public void HasBuilding_ShouldMatch_FindBuilding()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player = world.GetPlayerById(1)!;

        Assert.False(WorldQueries.HasBuilding(world, player, "town_center"));

        var tc = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(), player.Id, new GridPosition(5, 5));
        tc.IsCompleted = true;
        world.AddEntity(tc);

        Assert.True(WorldQueries.HasBuilding(world, player, "town_center"));

        tc.IsCompleted = false;
        Assert.False(WorldQueries.HasBuilding(world, player, "town_center"));
    }

    [Fact]
    [Trait("Category", "WorldQueries")]
    public void CountBuildings_ShouldCountCompletedAndOwnerFiltered()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player1 = world.GetPlayerById(1)!;
        var player2 = world.GetPlayerById(2)!;

        var houseDef = TestDefinitionFactory.CreateHouseWithCost();

        var h1 = BuildingFactory.Create(houseDef, player1.Id, new GridPosition(1, 1));
        h1.IsCompleted = true;
        world.AddEntity(h1);

        var h2 = BuildingFactory.Create(houseDef, player1.Id, new GridPosition(3, 1));
        h2.IsCompleted = false;
        world.AddEntity(h2);

        var h3 = BuildingFactory.Create(houseDef, player1.Id, new GridPosition(5, 1));
        h3.IsCompleted = true;
        world.AddEntity(h3);

        var h4 = BuildingFactory.Create(houseDef, player2.Id, new GridPosition(7, 1));
        h4.IsCompleted = true;
        world.AddEntity(h4);

        Assert.Equal(2, WorldQueries.CountBuildings(world, player1, "house"));
        Assert.Equal(1, WorldQueries.CountBuildings(world, player2, "house"));
    }
}
