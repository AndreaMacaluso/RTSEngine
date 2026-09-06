using RTSEngine.Core.Actions;
using RTSEngine.Core.Commands;
using RTSEngine.Core.Entities.Resources;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.States;
using RTSEngine.Core.State;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Settings;
using RTSEngine.Core.Systems.Pathfinding;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Tests.TestHelpers;
using RTSEngine.Core.Players;

namespace RTSEngine.Tests.Actions;

public class GatherActionsTests
{
    [Fact]
    [Trait("Category", "GatheringAction")]
    [Trait("Category", "Gathering")]
    public void BeginMoveToResource_ShouldFillPathQueue()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player = world.GetPlayerById(1)!;

        var definition = TestDefinitionFactory.CreateVillager();

        var unit = UnitFactory.Create(
            definition,
            1,
            new GridPosition(5, 5));

        world.Entities.Add(unit, player);

        var tree = new Tree(new GridPosition(6, 5));

        world.Entities.Add(tree);

        unit.Gather.TargetResourceId = tree.Id;

        var context = new RuntimeContext
        {
            World = world,
            CommandQueue = new CommandQueue(),
            PathFinder = new AStarPathFinder(new GroundMovementFilter()),
            UnitRepository = new UnitDefinitionRepository([]),
            BuildingRepository = new BuildingDefinitionRepository([]),
            Settings = new GameSettings()
        };
        var result = GatherActions.BeginMoveToResource(context, unit);

        Assert.True(result);
        Assert.NotNull(unit.Movement.Destination);
    }

    [Fact]
    [Trait("Category", "GatheringAction")]
    [Trait("Category", "Gathering")]
    public void GatherOneTick_ShouldIncreaseCurrentLoad()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player = world.GetPlayerById(1)!;

        var definition = TestDefinitionFactory.CreateVillager();

        var unit = UnitFactory.Create(
            definition,
            1,
            new GridPosition(5, 5));

        world.Entities.Add(unit, player);

        var tree = new Tree(new GridPosition(6, 5));

        world.Entities.Add(tree);

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
        var player = world.GetPlayerById(1)!;

        var definition = TestDefinitionFactory.CreateVillager();

        var unit = UnitFactory.Create(
            definition,
            1,
            new GridPosition(5, 5));

        world.Entities.Add(unit, player);

        var tree = new Tree(new GridPosition(6, 5));

        world.Entities.Add(tree);

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
        var player = world.GetPlayerById(1)!;

        var definition = TestDefinitionFactory.CreateVillager();

        var unit = UnitFactory.Create(
            definition,
            1,
            new GridPosition(5, 5));

        world.Entities.Add(unit, player);

        var tree = new Tree(new GridPosition(6, 5));

        world.Entities.Add(tree);

        unit.Gather.TargetResourceId = tree.Id;

        unit.Gather.CurrentLoad = unit.Gather.Capacity - 1;

        GatherResult result = GatherActions.GatherOneTick(world, unit);

        Assert.Equal(GatherResult.InventoryFull, result);
    }

    [Fact]
    [Trait("Category", "GatheringAction")]
    [Trait("Category", "Gathering")]
    public void DepositInventory_ShouldTransferResourcesToPlayer()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player = world.GetPlayerById(1)!;

        var definition = TestDefinitionFactory.CreateVillager();

        var unit = UnitFactory.Create(
            definition,
            1,
            new GridPosition(5, 5));

        world.Entities.Add(unit, player);

        unit.Gather.CarriedResource = ResourceType.Wood;
        unit.Gather.CurrentLoad = 15;

        GatherActions.DepositInventory(world, unit);

        Assert.Equal(15, player.Economy.Get(ResourceType.Wood));
    }

    [Fact]
    [Trait("Category", "GatheringAction")]
    [Trait("Category", "Gathering")]
    public void DepositInventory_ShouldClearInventory()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player = world.GetPlayerById(1)!;

        var definition = TestDefinitionFactory.CreateVillager();

        var unit = UnitFactory.Create(
            definition,
            1,
            new GridPosition(5, 5));

        world.Entities.Add(unit, player);

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

        unit.CurrentTask = EntityState.Gathering;

        unit.Gather.CurrentLoad = 10;
        unit.Gather.TargetResourceId = 5;
        unit.Gather.CarriedResource = ResourceType.Wood;
        unit.Gather.Phase = GatherPhase.Gathering;

        GatherActions.StopGathering(unit);

        Assert.Equal(EntityState.Idle, unit.CurrentTask);
        Assert.Equal(10, unit.Gather.CurrentLoad);//Stop gathering does not reset the load
        Assert.Null(unit.Gather.TargetResourceId);
        Assert.Equal(GatherPhase.None, unit.Gather.Phase);
    }
}