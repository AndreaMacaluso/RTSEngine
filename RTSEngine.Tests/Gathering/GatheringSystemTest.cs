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
    [Fact]
    [Trait("Category", "GatheringSystem")]
    [Trait("Category", "Gathering")]
    public void Update_ShouldSwitchToGathering_WhenUnitReachedResource()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();

        var unit = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            1,
            new GridPosition(5, 5));

        world.AddEntity(unit);
        
        var tree = new Tree(new GridPosition(6, 5));

        world.AddResource(tree);

        unit.Gather.TargetResourceId = tree.Id;

        unit.Gather.Phase = GatherPhase.MovingToResource;

        GatherSystem.Update(world);

        Assert.Equal(
            GatherPhase.Gathering,
            unit.Gather.Phase);
    }

    [Fact]
    [Trait("Category", "GatheringSystem")]
    [Trait("Category", "Gathering")]
    public void Update_ShouldStopGathering_WhenResourceDoesNotExist()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();

        var unit = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            1,
            new GridPosition(5, 5));

        world.AddEntity(unit);

        unit.CurrentTask = UnitTask.Gathering;
        unit.Gather.Phase = GatherPhase.MovingToResource;
        unit.Gather.TargetResourceId = 999;

        GatherSystem.Update(world);

        Assert.Equal(UnitTask.Idle, unit.CurrentTask);
        Assert.Equal(GatherPhase.None, unit.Gather.Phase);
    }

    [Fact]
    [Trait("Category", "GatheringSystem")]
    [Trait("Category", "Gathering")]
    public void Update_ShouldSwitchToMovingToDeposit_WhenInventoryBecomesFull()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();

        var unit = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            1,
            new GridPosition(5, 5));
        var tree = new Tree(new GridPosition(6, 5));

        world.AddResource(tree);
        
        unit.Gather.TargetResourceId = tree.Id;
        unit.Gather.Phase = GatherPhase.Gathering;
        unit.Gather.CurrentLoad = unit.Gather.Capacity - 1;
        unit.Gather.DepositPosition = new GridPosition(7, 7);
        world.AddEntity(unit);
        GatherSystem.Update(world);

        Assert.Equal(
            GatherPhase.MovingToDeposit,
            unit.Gather.Phase);

        Assert.Contains(
        world.PendingCommands,
        c => c is MoveCommand);
    }

    [Fact]
    [Trait("Category", "GatheringSystem")]
    [Trait("Category", "Gathering")]
    public void Update_ShouldSwitchToDepositing_WhenDepositReached()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();

        var unit = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            1,
            new GridPosition(5, 5));

        world.AddEntity(unit);

        unit.Gather.Phase = GatherPhase.MovingToDeposit;

        GatherSystem.Update(world);

        Assert.Equal(
            GatherPhase.Depositing,
            unit.Gather.Phase);
    }

    [Fact]
    [Trait("Category", "GatheringSystem")]
    [Trait("Category", "Gathering")]
    public void Update_ShouldResumeGathering_WhenResourceStillExists()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();

        var unit = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            1,
            new GridPosition(5, 5));

        var tree = new Tree(new GridPosition(6, 5));
        world.AddResource(tree);
        unit.Gather.TargetResourceId = tree.Id;
        world.AddEntity(unit);

        var resource = world.GetResourceById( unit.Gather.TargetResourceId ?? 0);
        Assert.NotNull(resource);
        Assert.True(GatherActions.CanContinueGathering(world, unit));
       
        unit.Gather.CarriedResource = ResourceType.Wood;
        unit.Gather.CurrentLoad = 10;
        unit.Gather.Phase = GatherPhase.Depositing;
        unit.CurrentTask = UnitTask.Gathering;
        GatherSystem.Update(world);

        Assert.Equal(
            GatherPhase.MovingToResource,
            unit.Gather.Phase);

        Assert.Single(world.PendingCommands);
    }

    [Fact]
    [Trait("Category", "GatheringSystem")]
    [Trait("Category", "Gathering")]
    public void Update_ShouldStopGathering_WhenResourceIsDepleted()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();

        var unit = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            1,
            new GridPosition(5, 5));

        world.AddEntity(unit);

        var tree = new Tree(new GridPosition(6, 5));

        world.AddResource(tree);

        tree.Gather(tree.Amount);

        unit.Gather.TargetResourceId = tree.Id;
        unit.Gather.Phase = GatherPhase.Depositing;
        unit.CurrentTask = UnitTask.Gathering;

        GatherSystem.Update(world);

        Assert.Equal(UnitTask.Idle, unit.CurrentTask);
        Assert.Equal(GatherPhase.None, unit.Gather.Phase);
    }

    [Fact]
    [Trait("Category", "GatheringSystem")]
    [Trait("Category", "Gathering")]
    public void Update_ShouldIgnoreUnitsThatCannotGather()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();

        var definition = TestDefinitionFactory.CreateMilitia();

        var unit = UnitFactory.Create(
            definition,
            1,
            new GridPosition(5, 5));

        world.AddEntity(unit);

        unit.Gather.Phase = GatherPhase.MovingToResource;

        GatherSystem.Update(world);

        Assert.Equal(
            GatherPhase.MovingToResource,
            unit.Gather.Phase);
    }

    [Fact]
    [Trait("Category", "GatheringSystem")]
    [Trait("Category", "Gathering")]
    public void Update_ShouldIgnoreUnitsWithGatherPhaseNone()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();

        var unit = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            1,
            new GridPosition(5, 5));

        world.AddEntity(unit);

        unit.Gather.Phase = GatherPhase.None;

        GatherSystem.Update(world);

        Assert.Equal(
            GatherPhase.None,
            unit.Gather.Phase);
    }
  
    [Fact]
    [Trait("Category", "GatheringSystem")]
    [Trait("Category", "Gathering")]
    [Trait("Category", "Gathering.Loop")]
    public void GatherLoop_ShouldCollectAndDepositResources()
    {
        // Arrange
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();

        Player? player = world.GetPlayerById(1);
        var villager = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            1,
            new GridPosition(1, 1));

        world.AddEntity(villager);

        var tree = new Tree(new GridPosition(5, 1));
        world.AddResource(tree);
        int initialAmount = tree.Amount;
        var townCenter = BuildingFactory.Create(
        TestDefinitionFactory.CreateTownCenter(),
        ownerId: 1,
        position: new GridPosition(1, 5));


        world.AddEntity(townCenter);
        var command = new GatherCommand
        {
            UnitIds = [villager.Id],
            ResourceId = tree.Id
        };

        world.AddCommand(command);
        //1 STEP SEND TO GATHER
        SimulationTestHelper.RunTicks(world,1);
    
        //SimulationTestHelper.RunTicks(world, 1);
        // Assert.Equal(
        //     new GridPosition(5, 5),
        //     villager.Movement.TargetPosition);
        Assert.Equal(UnitTask.Gathering,villager.CurrentTask);        
        Assert.Equal(tree.Id,villager.Gather.TargetResourceId);
        Assert.Equal(GatherPhase.MovingToResource,villager.Gather.Phase);
        Assert.NotEmpty(villager.Movement.PathQueue);
        Assert.Equal(0,villager.Gather.CurrentLoad);
        Assert.Equal(ResourceType.Wood,villager.Gather.CarriedResource);
        
        //2 STEP GATHER
        SimulationTestHelper.RunTicks(world, 4);
        Assert.Equal(UnitTask.Gathering,villager.CurrentTask);
        Assert.Equal(GatherPhase.Gathering,villager.Gather.Phase);
        Assert.Equal(tree.Id,villager.Gather.TargetResourceId);
        Assert.Equal(1,villager.Gather.CurrentLoad);

        //3 STEP MOVE TO DEPOSIT
        SimulationTestHelper.RunTicks(world, 19);
        Assert.Equal(20, villager.Gather.CurrentLoad);
        Assert.Null(villager.Gather.DepositPosition);
        Assert.Equal(UnitTask.Gathering, villager.CurrentTask);
        Assert.Equal(GatherPhase.MovingToDeposit, villager.Gather.Phase);

      
        //4 STEP DEPOSIT
        SimulationTestHelper.RunTicks(world, 1);
        Assert.Equal(20, villager.Gather.CurrentLoad);
        Assert.Equal(UnitTask.Gathering, villager.CurrentTask);
        Assert.Equal(GatherPhase.Depositing, villager.Gather.Phase);
        Assert.Equal(tree.Id, villager.Gather.TargetResourceId);

        //5 STEP MOVING TO RESOURCE AGAIN
        SimulationTestHelper.RunTicks(world, 1);
        Assert.Equal(0, villager.Gather.CurrentLoad);
        Assert.Equal(UnitTask.Gathering, villager.CurrentTask);
        Assert.Equal(GatherPhase.MovingToResource, villager.Gather.Phase);
        Assert.Equal(20, player.Wood);
        Assert.Equal(tree.Id, villager.Gather.TargetResourceId);
        Assert.True(tree.Amount < initialAmount);
                // Assert
        // Assert.True(player.Wood > 0);

        // Assert.True(tree.Amount < initialAmount);
        // Assert.Equal(
        //     new GridPosition(5, 5),
        //     villager.Movement.TargetPosition);
        //Assert.NotEmpty(villager.Movement.PathQueue);
    }
}
