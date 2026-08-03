using RTSEngine.Core.Actions;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Tests.TestHelpers;

namespace RTSEngine.Tests.Buildings;

public class BuildingPlacementActionsTests
{
    [Fact]
    public void PlaceFoundation_ShouldSpawnBuilding()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();

        var player = world.GetPlayerById(1);
        Assert.NotNull(player);
        player.Economy.Add(ResourceType.Wood, 500);

        var definition = TestDefinitionFactory.CreateHouseWithCost();

        var building = BuildingPlacementActions.PlaceFoundation(
            world,
            player,
            definition,
            new GridPosition(7, 7));

        Assert.NotNull(building);
        Assert.Contains(building, world.Entities);
    }

    [Fact]
    public void PlaceFoundation_ShouldPayResources()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();

        var player = world.GetPlayerById(1);
        Assert.NotNull(player);
        player.Economy.Add(ResourceType.Wood, 500);

        var definition = TestDefinitionFactory.CreateHouseWithCost();

        var woodBefore = player.Economy.Wood;

        var result = BuildingPlacementActions.PlaceFoundation(
            world,
            player,
            definition,
            new GridPosition(7, 7));
        Assert.NotNull(result);
        Assert.Equal(
            woodBefore - 100,
            player.Economy.Wood);
    }

    [Fact]
    public void PlaceFoundation_ShouldFail_WhenPlayerCannotAfford()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player = world.GetPlayerById(1);
        Assert.NotNull(player);
        var definition = TestDefinitionFactory.CreateHouseWithCost();

        var building = BuildingPlacementActions.PlaceFoundation(
            world,
            player,
            definition,
            new GridPosition(7, 7));

        Assert.Null(building);
    }

    [Fact]
    public void PlaceFoundation_ShouldNotSpawnBuilding_WhenCannotAfford()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player = world.GetPlayerById(1);
        Assert.NotNull(player);
        var definition = TestDefinitionFactory.CreateHouseWithCost();

        var result = BuildingPlacementActions.PlaceFoundation(
            world,
            player,
            definition,
            new GridPosition(7, 7));

        Assert.Null(result);
    }

    [Fact]
    public void PlaceFoundation_ShouldFail_WhenPlacementIsInvalid()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();

        var player = world.GetPlayerById(1);
        Assert.NotNull(player);
        player.Economy.Add(ResourceType.Wood, 500);

        var definition = TestDefinitionFactory.CreateHouseWithCost();

        var existing = BuildingFactory.Create(
            definition,
            2,
            new GridPosition(7, 7));

        world.AddEntity(existing);

        var building = BuildingPlacementActions.PlaceFoundation(
            world,
            player,
            definition,
            new GridPosition(7, 7));

        Assert.Null(building);
    }
}