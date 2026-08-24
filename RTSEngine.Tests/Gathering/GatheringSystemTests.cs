using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Entities.Resources;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.States;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.State;
using RTSEngine.Core.Systems;
using RTSEngine.Tests.TestHelpers;
using RTSEngine.Core.Actions;
using RTSEngine.Core.Commands;
using RTSEngine.Core.Players;

namespace RTSEngine.Tests.Gathering;

public class GatheringSystemTests
{
    private readonly GameWorld _world;
    private readonly Unit _villager;
    private readonly Tree _tree;
    private readonly Building _townCenter;

    public GatheringSystemTests()
    {
        _world = TestWorldFactory.CreateWorldWithTwoPlayers();

        var player = _world.GetPlayerById(1)!;

        _villager = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            1,
            new GridPosition(5, 5));
        _world.Entities.Add(_villager, player);

        _tree = new Tree(new GridPosition(6, 5));
        _world.Entities.Add(_tree);

        _townCenter = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            1,
            new GridPosition(1, 1));
        _world.Entities.Add(_townCenter, player);
    }

    [Fact]
    [Trait("Category", "GatheringSystem")]
    [Trait("Category", "Gathering")]
    public void Update_ShouldSwitchToGathering_WhenUnitReachedResource()
    {
        _villager.Gather.TargetResourceId = _tree.Id;
        _villager.Gather.Phase = GatherPhase.MovingToResource;

        GatherSystem.Update(_world);

        Assert.Equal(GatherPhase.Gathering, _villager.Gather.Phase);
    }

    [Fact]
    [Trait("Category", "GatheringSystem")]
    [Trait("Category", "Gathering")]
    public void Update_ShouldStopGathering_WhenResourceDoesNotExist()
    {
        _villager.CurrentTask = UnitTask.Gathering;
        _villager.Gather.Phase = GatherPhase.MovingToResource;
        _villager.Gather.TargetResourceId = 999;

        GatherSystem.Update(_world);

        Assert.Equal(UnitTask.Idle, _villager.CurrentTask);
        Assert.Equal(GatherPhase.None, _villager.Gather.Phase);
    }

    [Fact]
    [Trait("Category", "GatheringSystem")]
    [Trait("Category", "Gathering")]
    public void Update_ShouldSwitchToMovingToDeposit_WhenInventoryBecomesFull()
    {
        _townCenter.IsCompleted = true;
        _townCenter.Health.CurrentHealth = _townCenter.Health.MaxHealth;
        _villager.Gather.TargetResourceId = _tree.Id;
        _villager.Gather.Phase = GatherPhase.Gathering;
        _villager.Gather.CurrentLoad = _villager.Gather.Capacity - 1;
        _villager.Gather.CarriedResource = ResourceType.Wood;

        GatherSystem.Update(_world);

        Assert.Equal(20, _villager.Gather.Capacity);
        Assert.NotNull(_villager.Gather.DepositPosition);
        Assert.Equal(GatherPhase.MovingToDeposit, _villager.Gather.Phase);
        Assert.Contains(_world.PendingCommands, c => c is MoveCommand);
    }

    [Fact]
    [Trait("Category", "GatheringSystem")]
    [Trait("Category", "Gathering")]
    public void Update_ShouldSwitchToWaitingForDeposit_WhenDepositIsNotFound()
    {
        _villager.Gather.TargetResourceId = _tree.Id;
        _villager.Gather.Phase = GatherPhase.Gathering;
        _villager.Gather.CurrentLoad = _villager.Gather.Capacity - 1;
        _villager.Gather.CarriedResource = ResourceType.Wood;

        GatherSystem.Update(_world);

        Assert.Equal(GatherPhase.WaitingForDeposit, _villager.Gather.Phase);
        Assert.Equal(20, _villager.Gather.CurrentLoad);
        Assert.Equal(ResourceType.Wood, _villager.Gather.CarriedResource);
    }

    [Fact]
    [Trait("Category", "GatheringSystem")]
    [Trait("Category", "Gathering")]
    public void Update_ShouldSwitchToDepositing_WhenDepositReached()
    {
        _villager.Gather.Phase = GatherPhase.MovingToDeposit;
        _villager.Gather.DepositPosition = new GridPosition(5, 6);

        GatherSystem.Update(_world);

        Assert.Equal(GatherPhase.Depositing, _villager.Gather.Phase);
    }

    [Fact]
    [Trait("Category", "GatheringSystem")]
    [Trait("Category", "Gathering")]
    public void Update_ShouldResumeGathering_WhenResourceStillExists()
    {
        _villager.Gather.TargetResourceId = _tree.Id;

        var resource = _world.Entities.GetResourceById(_villager.Gather.TargetResourceId ?? 0);
        Assert.NotNull(resource);
        Assert.True(GatherActions.CanContinueGathering(_world, _villager));

        _villager.Gather.CarriedResource = ResourceType.Wood;
        _villager.Gather.CurrentLoad = 10;
        _villager.Gather.Phase = GatherPhase.Depositing;
        _villager.CurrentTask = UnitTask.Gathering;
        GatherSystem.Update(_world);

        Assert.Equal(GatherPhase.MovingToResource, _villager.Gather.Phase);
        Assert.Single(_world.PendingCommands);
    }

    [Fact]
    [Trait("Category", "GatheringSystem")]
    [Trait("Category", "Gathering")]
    public void Update_ShouldStopGathering_WhenResourceIsDepleted()
    {
        _tree.Gather(_tree.Amount);

        _villager.Gather.TargetResourceId = _tree.Id;
        _villager.Gather.Phase = GatherPhase.Depositing;
        _villager.CurrentTask = UnitTask.Gathering;

        GatherSystem.Update(_world);

        Assert.Equal(UnitTask.Idle, _villager.CurrentTask);
        Assert.Equal(GatherPhase.None, _villager.Gather.Phase);
    }

    [Fact]
    [Trait("Category", "GatheringSystem")]
    [Trait("Category", "Gathering")]
    public void Update_ShouldIgnoreUnitsThatCannotGather()
    {
        var player = _world.GetPlayerById(1)!;
        var militia = UnitFactory.Create(
            TestDefinitionFactory.CreateMilitia(),
            1,
            new GridPosition(5, 5));
        _world.Entities.Add(militia, player);

        militia.Gather.Phase = GatherPhase.MovingToResource;

        GatherSystem.Update(_world);

        Assert.Equal(GatherPhase.MovingToResource, militia.Gather.Phase);
    }

    [Fact]
    [Trait("Category", "GatheringSystem")]
    [Trait("Category", "Gathering")]
    public void Update_ShouldIgnoreUnitsWithGatherPhaseNone()
    {
        _villager.Gather.Phase = GatherPhase.None;

        GatherSystem.Update(_world);

        Assert.Equal(GatherPhase.None, _villager.Gather.Phase);
    }

    [Fact]
    [Trait("Category", "GatheringSystem")]
    [Trait("Category", "Gathering")]
    [Trait("Category", "Gathering.Loop")]
    public void GatherLoop_ShouldCollectAndDepositResources()
    {
        var player = _world.GetPlayerById(1)!;
        var villager = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            1,
            new GridPosition(1, 1));
        villager.Health.CurrentHealth = 50;
        _world.Entities.Add(villager, player);

        var tree = new Tree(new GridPosition(5, 1));
        _world.Entities.Add(tree);
        int initialAmount = tree.Amount;

        var townCenter = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            1,
            new GridPosition(1, 5));
        townCenter.IsCompleted = true;
        townCenter.Health.CurrentHealth = townCenter.Health.MaxHealth;
        _world.Entities.Add(townCenter, player);

        _world.AddCommand(new GatherCommand
        {
            UnitIds = [villager.Id],
            ResourceId = tree.Id
        });

        SimulationTestHelper.RunTicks(_world, 1);

        Assert.Equal(UnitTask.Gathering, villager.CurrentTask);
        Assert.Equal(tree.Id, villager.Gather.TargetResourceId);
        Assert.Equal(GatherPhase.MovingToResource, villager.Gather.Phase);
        Assert.NotEmpty(villager.Movement.PathQueue);
        Assert.Equal(0, villager.Gather.CurrentLoad);
        Assert.Equal(ResourceType.Wood, villager.Gather.CarriedResource);

        SimulationTestHelper.RunTicks(_world, 3);
        Assert.Equal(UnitTask.Gathering, villager.CurrentTask);
        Assert.Equal(GatherPhase.Gathering, villager.Gather.Phase);
        Assert.Equal(tree.Id, villager.Gather.TargetResourceId);
        Assert.Equal(1, villager.Gather.CurrentLoad);

        SimulationTestHelper.RunTicks(_world, 19);
        Assert.Equal(20, villager.Gather.CurrentLoad);
        Assert.NotNull(villager.Gather.DepositPosition);
        Assert.Equal(UnitTask.Gathering, villager.CurrentTask);
        Assert.Equal(GatherPhase.MovingToDeposit, villager.Gather.Phase);

        SimulationTestHelper.RunTicks(_world, 3);
        Assert.Equal(20, villager.Gather.CurrentLoad);
        Assert.Equal(UnitTask.Gathering, villager.CurrentTask);
        Assert.Equal(GatherPhase.Depositing, villager.Gather.Phase);
        Assert.Equal(tree.Id, villager.Gather.TargetResourceId);

        SimulationTestHelper.RunTicks(_world, 1);
        Assert.Equal(0, villager.Gather.CurrentLoad);
        Assert.Equal(UnitTask.Gathering, villager.CurrentTask);
        Assert.Equal(GatherPhase.MovingToResource, villager.Gather.Phase);

        Assert.Equal(20, player.Economy.Get(ResourceType.Wood));
        Assert.Equal(tree.Id, villager.Gather.TargetResourceId);
        Assert.True(tree.Amount < initialAmount);
    }

    [Fact]
    [Trait("Category", "GatheringSystem")]
    [Trait("Category", "Gathering")]
    [Trait("Category", "Gathering.Loop")]
    public void GatherLoop_ShouldRetarget_WhenResourceIsDepleted()
    {
        var player = _world.GetPlayerById(1)!;

        var villager = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            player.Id,
            new GridPosition(1, 1));
        villager.Health.CurrentHealth = 50;
        _world.Entities.Add(villager, player);

        var tree1 = new Tree(new GridPosition(5, 1));
        var tree2 = new Tree(new GridPosition(8, 1));
        _world.Entities.Add(tree1);
        _world.Entities.Add(tree2);

        var townCenter = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            player.Id,
            new GridPosition(1, 5));
        townCenter.IsCompleted = true;
        townCenter.Health.CurrentHealth = townCenter.Health.MaxHealth;
        _world.Entities.Add(townCenter, player);

        _world.AddCommand(new GatherCommand
        {
            UnitIds = [villager.Id],
            ResourceId = tree1.Id
        });

        SimulationTestHelper.RunTicks(_world, 5);
        Assert.Equal(tree1.Id, villager.Gather.TargetResourceId);

        villager.Gather.CurrentLoad = 10;
        villager.Gather.CarriedResource = ResourceType.Wood;
        tree1.Amount = 0;

        SimulationTestHelper.RunTicks(_world, 2);
        Assert.Equal(GatherPhase.MovingToResource, villager.Gather.Phase);
        Assert.Equal(tree2.Id, villager.Gather.TargetResourceId);

        SimulationTestHelper.RunTicks(_world, 5);
        Assert.Equal(UnitTask.Gathering, villager.CurrentTask);
        Assert.Equal(GatherPhase.Gathering, villager.Gather.Phase);
        Assert.Equal(tree2.Id, villager.Gather.TargetResourceId);
    }

    [Fact]
    [Trait("Category", "GatheringSystem")]
    [Trait("Category", "Gathering")]
    [Trait("Category", "Gathering.Loop")]
    public void GatherLoop_ShouldAllowMultipleVillagersToGatherSameResource()
    {
        var player1 = _world.GetPlayerById(1)!;

        var villager1 = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            1,
            new GridPosition(1, 1));
        villager1.Health.CurrentHealth = 50;

        var villager2 = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            1,
            new GridPosition(2, 1));
        villager2.Health.CurrentHealth = 50;

        _world.Entities.Add(villager1, player1);
        _world.Entities.Add(villager2, player1);

        var townCenter = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            1,
            new GridPosition(1, 5));
        townCenter.IsCompleted = true;
        townCenter.Health.CurrentHealth = townCenter.Health.MaxHealth;
        _world.Entities.Add(townCenter, player1);

        var tree = new Tree(new GridPosition(5, 1));
        _world.Entities.Add(tree);

        _world.AddCommand(new GatherCommand
        {
            UnitIds = [villager1.Id],
            ResourceId = tree.Id
        });

        _world.AddCommand(new GatherCommand
        {
            UnitIds = [villager2.Id],
            ResourceId = tree.Id
        });

        SimulationTestHelper.RunTicks(_world, 20);

        Assert.Equal(UnitTask.Gathering, villager1.CurrentTask);
        Assert.Equal(UnitTask.Gathering, villager2.CurrentTask);
        Assert.Equal(tree.Id, villager1.Gather.TargetResourceId);
        Assert.Equal(tree.Id, villager2.Gather.TargetResourceId);
        Assert.Equal(GatherPhase.Gathering, villager2.Gather.Phase);
        Assert.Equal(GatherPhase.Gathering, villager1.Gather.Phase);
        Assert.True(villager1.Gather.CurrentLoad > 0);
        Assert.True(villager2.Gather.CurrentLoad > 0);
    }

    [Fact]
    [Trait("Category", "GatheringSystem")]
    [Trait("Category", "Gathering")]
    [Trait("Category", "Gathering.Loop")]
    public void GatherLoop_ShouldAllowDifferentPlayersToGatherSameResource()
    {
        var player1 = _world.GetPlayerById(1)!;
        var player2 = _world.GetPlayerById(2)!;

        var villager1 = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            1,
            new GridPosition(1, 1));
        villager1.Health.CurrentHealth = 50;

        var villager2 = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            2,
            new GridPosition(2, 1));
        villager2.Health.CurrentHealth = 50;

        _world.Entities.Add(villager1, player1);
        _world.Entities.Add(villager2, player2);

        var townCenter1 = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            1,
            new GridPosition(1, 5));
        townCenter1.IsCompleted = true;
        townCenter1.Health.CurrentHealth = townCenter1.Health.MaxHealth;

        var townCenter2 = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            2,
            new GridPosition(10, 5));
        townCenter2.IsCompleted = true;
        townCenter2.Health.CurrentHealth = townCenter2.Health.MaxHealth;

        _world.Entities.Add(townCenter1, player1);
        _world.Entities.Add(townCenter2, player2);

        var tree = new Tree(new GridPosition(5, 1));
        _world.Entities.Add(tree);

        _world.AddCommand(new GatherCommand
        {
            UnitIds = [villager1.Id],
            ResourceId = tree.Id
        });

        _world.AddCommand(new GatherCommand
        {
            UnitIds = [villager2.Id],
            ResourceId = tree.Id
        });

        SimulationTestHelper.RunTicks(_world, 20);

        Assert.Equal(UnitTask.Gathering, villager1.CurrentTask);
        Assert.Equal(UnitTask.Gathering, villager2.CurrentTask);
        Assert.Equal(tree.Id, villager1.Gather.TargetResourceId);
        Assert.Equal(tree.Id, villager2.Gather.TargetResourceId);
        Assert.Equal(GatherPhase.Gathering, villager1.Gather.Phase);
        Assert.Equal(GatherPhase.Gathering, villager2.Gather.Phase);
        Assert.True(villager1.Gather.CurrentLoad > 0);
        Assert.True(villager2.Gather.CurrentLoad > 0);
        Assert.Equal(0, player1.Economy.Get(ResourceType.Wood));
        Assert.Equal(0, player2.Economy.Get(ResourceType.Wood));
    }

    [Fact]
    [Trait("Category", "GatheringSystem")]
    [Trait("Category", "Gathering")]
    [Trait("Category", "Gathering.WaitingForDeposit")]
    public void WaitingForDeposit_ShouldPreserveResources_WhenBeginMoveToDepositFails()
    {
        _villager.CurrentTask = UnitTask.Gathering;
        _villager.Gather.TargetResourceId = _tree.Id;
        _villager.Gather.Phase = GatherPhase.Gathering;
        _villager.Gather.CurrentLoad = 20;
        _villager.Gather.CarriedResource = ResourceType.Wood;

        GatherSystem.Update(_world);

        Assert.Equal(GatherPhase.WaitingForDeposit, _villager.Gather.Phase);
        Assert.Equal(20, _villager.Gather.CurrentLoad);
        Assert.Equal(ResourceType.Wood, _villager.Gather.CarriedResource);
        Assert.Equal(UnitTask.Gathering, _villager.CurrentTask);
    }

    [Fact]
    [Trait("Category", "GatheringSystem")]
    [Trait("Category", "Gathering")]
    [Trait("Category", "Gathering.WaitingForDeposit")]
    public void WaitingForDeposit_ShouldRetryPeriodically()
    {
        _villager.Gather.TargetResourceId = _tree.Id;
        _villager.Gather.Phase = GatherPhase.Gathering;
        _villager.Gather.CurrentLoad = 20;
        _villager.Gather.CarriedResource = ResourceType.Wood;

        GatherSystem.Update(_world);
        Assert.Equal(GatherPhase.WaitingForDeposit, _villager.Gather.Phase);

        for (int i = 0; i < 3; i++)
        {
            GatherSystem.Update(_world);
            Assert.Equal(GatherPhase.WaitingForDeposit, _villager.Gather.Phase);
        }
    }

    [Fact]
    [Trait("Category", "GatheringSystem")]
    [Trait("Category", "Gathering")]
    [Trait("Category", "Gathering.WaitingForDeposit")]
    public void WaitingForDeposit_ShouldStopAfterMaxWaitTicks_WhenNoDepositAvailable()
    {
        _villager.Gather.TargetResourceId = _tree.Id;
        _villager.Gather.Phase = GatherPhase.Gathering;
        _villager.Gather.CurrentLoad = 20;
        _villager.Gather.CarriedResource = ResourceType.Wood;

        GatherSystem.Update(_world);
        Assert.Equal(GatherPhase.WaitingForDeposit, _villager.Gather.Phase);

        for (int i = 0; i < 4; i++)
        {
            GatherSystem.Update(_world);
        }

        Assert.Equal(GatherPhase.None, _villager.Gather.Phase);
        Assert.Equal(UnitTask.Idle, _villager.CurrentTask);
        Assert.Equal(20, _villager.Gather.CurrentLoad);
        Assert.Equal(ResourceType.Wood, _villager.Gather.CarriedResource);
    }

    [Fact]
    [Trait("Category", "GatheringSystem")]
    [Trait("Category", "Gathering")]
    [Trait("Category", "Gathering.WaitingForDeposit")]
    public void WaitingForDeposit_ShouldTransitionToMovingToDeposit_WhenDepositBecomesAvailable()
    {
        _villager.CurrentTask = UnitTask.Gathering;
        _villager.Gather.TargetResourceId = _tree.Id;
        _villager.Gather.Phase = GatherPhase.Gathering;
        _villager.Gather.CurrentLoad = 20;
        _villager.Gather.CarriedResource = ResourceType.Wood;

        GatherSystem.Update(_world);
        Assert.Equal(GatherPhase.WaitingForDeposit, _villager.Gather.Phase);

        _townCenter.IsCompleted = true;
        _townCenter.Health.CurrentHealth = _townCenter.Health.MaxHealth;
        _villager.Gather.WaitingForDepositTicks = 3;
        GatherSystem.Update(_world);

        Assert.Equal(GatherPhase.MovingToDeposit, _villager.Gather.Phase);
        Assert.NotNull(_villager.Gather.DepositPosition);
    }

    [Fact]
    [Trait("Category", "GatheringSystem")]
    [Trait("Category", "Gathering")]
    [Trait("Category", "Gathering.WaitingForDeposit")]
    public void MovingToDeposit_ShouldTransitionToWaitingForDeposit_WhenStuck()
    {
        _villager.CurrentTask = UnitTask.Gathering;
        _villager.Gather.Phase = GatherPhase.MovingToDeposit;
        _villager.Gather.DepositPosition = new GridPosition(8, 8);
        _villager.Gather.CurrentLoad = 20;
        _villager.Gather.CarriedResource = ResourceType.Wood;
        _villager.Gather.WaitingForDepositTicks = 1;
        _villager.Movement.Destination = new GridPosition(8, 8);

        GatherSystem.Update(_world);

        Assert.Equal(GatherPhase.WaitingForDeposit, _villager.Gather.Phase);
        Assert.Equal(20, _villager.Gather.CurrentLoad);
        Assert.Equal(ResourceType.Wood, _villager.Gather.CarriedResource);
    }
}
