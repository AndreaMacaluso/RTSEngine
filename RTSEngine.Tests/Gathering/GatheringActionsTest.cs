using RTSEngine.Core.Actions;
using RTSEngine.Core.Entities.Resources;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.States;
using RTSEngine.Core.State;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Tests.TestHelpers;
using RTSEngine.Core.Players;

namespace RTSEngine.Tests.Actions;

public class GatherActionsTests
{
    [Fact]
    [Trait("Category", "GatheringAction")]
    [Trait("Category", "Gathering")]
    public void BeginMoveToResource_ShouldQueueMoveCommand()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();

        var definition = TestDefinitionFactory.CreateVillager();

        var unit = UnitFactory.Create(
            definition,
            1,
            new GridPosition(5, 5));

        world.AddEntity(unit);

        var tree = new Tree(new GridPosition(6, 5));

        world.AddResource(tree);

        unit.Gather.TargetResourceId = tree.Id;

        GatherActions.BeginMoveToResource(world, unit);

        Assert.Single(world.PendingCommands);
    }

    [Fact]
    [Trait("Category", "GatheringAction")]
    [Trait("Category", "Gathering")]
    public void GatherOneTick_ShouldIncreaseCurrentLoad()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();

        var definition = TestDefinitionFactory.CreateVillager();

        var unit = UnitFactory.Create(
            definition,
            1,
            new GridPosition(5, 5));

        world.AddEntity(unit);

        var tree = new Tree(new GridPosition(6, 5));

        world.AddResource(tree);

        unit.Gather.TargetResourceId = tree.Id;

        GatherActions.GatherOneTick(world, unit);

        Assert.Equal(1, unit.Gather.CurrentLoad);
    }

    [Fact]
    [Trait("Category", "GatheringAction")]
    [Trait("Category", "Gathering")]
    public void GatherOneTick_ShouldReduceResourceAmount()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();

        var definition = TestDefinitionFactory.CreateVillager();

        var unit = UnitFactory.Create(
            definition,
            1,
            new GridPosition(5, 5));

        world.AddEntity(unit);

        var tree = new Tree(new GridPosition(6, 5));

        world.AddResource(tree);

        unit.Gather.TargetResourceId = tree.Id;

        GatherActions.GatherOneTick(world, unit);

        Assert.Equal(199, tree.Amount);
    }

    [Fact]
    [Trait("Category", "GatheringAction")]
    [Trait("Category", "Gathering")]
    public void GatherOneTick_ShouldReturnTrueWhenInventoryBecomesFull()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();

        var definition = TestDefinitionFactory.CreateVillager();

        var unit = UnitFactory.Create(
            definition,
            1,
            new GridPosition(5, 5));

        world.AddEntity(unit);

        var tree = new Tree(new GridPosition(6, 5));

        world.AddResource(tree);

        unit.Gather.TargetResourceId = tree.Id;

        unit.Gather.CurrentLoad =
            unit.Gather.Capacity - 1;

        bool full =
            GatherActions.GatherOneTick(world, unit);

        Assert.True(full);
    }

    [Fact]
    [Trait("Category", "GatheringAction")]
    [Trait("Category", "Gathering")]
    public void DepositInventory_ShouldTransferResourcesToPlayer()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();

        var definition = TestDefinitionFactory.CreateVillager();
        Player? player = world.GetPlayerById(1);
        Assert.NotNull(player);
        var unit = UnitFactory.Create(
            definition,
            1,
            new GridPosition(5, 5));

        world.AddEntity(unit);

        unit.Gather.CarriedResource = ResourceType.Wood;
        unit.Gather.CurrentLoad = 15;

        GatherActions.DepositInventory(world, unit);

        Assert.Equal(15, player.Wood);
    }

    [Fact]
    [Trait("Category", "GatheringAction")]
    [Trait("Category", "Gathering")]
    public void DepositInventory_ShouldClearInventory()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();

        var definition = TestDefinitionFactory.CreateVillager();

        var unit = UnitFactory.Create(
            definition,
            1,
            new GridPosition(5, 5));

        world.AddEntity(unit);

        unit.Gather.CarriedResource = ResourceType.Wood;
        unit.Gather.CurrentLoad = 15;

        GatherActions.DepositInventory(world, unit);

        Assert.Equal(0, unit.Gather.CurrentLoad);
    }

    [Fact]
    [Trait("Category", "GatheringAction")]
    [Trait("Category", "Gathering")]
    public void StopGathering_ShouldResetGatherState()
    {
        var definition = TestDefinitionFactory.CreateVillager();

        var unit = UnitFactory.Create(
            definition,
            1,
            new GridPosition(5, 5));

        unit.CurrentTask = UnitTask.Gathering;

        unit.Gather.CurrentLoad = 10;
        unit.Gather.TargetResourceId = 5;
        unit.Gather.CarriedResource = ResourceType.Wood;
        unit.Gather.Phase = GatherPhase.Gathering;

        GatherActions.StopGathering(unit);

        Assert.Equal(UnitTask.Idle, unit.CurrentTask);
        Assert.Equal(0, unit.Gather.CurrentLoad);
        Assert.Null(unit.Gather.TargetResourceId);
        Assert.Null(unit.Gather.CarriedResource);
        Assert.Equal(GatherPhase.None, unit.Gather.Phase);
    }
}