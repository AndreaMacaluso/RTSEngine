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
using System.Diagnostics;
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
        var townCenter = BuildingFactory.Create(
        TestDefinitionFactory.CreateTownCenter(),
        ownerId: 1,
        position: new GridPosition(1, 1));
        world.AddEntity(townCenter);
        unit.Gather.TargetResourceId = tree.Id;
        unit.Gather.Phase = GatherPhase.Gathering;
        unit.Gather.CurrentLoad = unit.Gather.Capacity - 1;
        unit.Gather.CarriedResource = ResourceType.Wood;

        world.AddEntity(unit);
        GatherSystem.Update(world);
        Assert.Equal(20,unit.Gather.Capacity);
        Assert.NotNull(unit.Gather.DepositPosition);
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
    public void Update_ShouldSwitchToWaitingForDeposit_WhenDepositIsNotFound()
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
        unit.Gather.CarriedResource = ResourceType.Wood;
        world.AddEntity(unit);
        GatherSystem.Update(world);

        Assert.Equal(
            GatherPhase.WaitingForDeposit,
            unit.Gather.Phase);
        Assert.Equal(20, unit.Gather.CurrentLoad);
        Assert.Equal(ResourceType.Wood, unit.Gather.CarriedResource);
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

        unit.Gather.DepositPosition =   new GridPosition(5, 6);



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

        Assert.NotNull(player);
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
        //     villager.Movement.Destination);
        Assert.Equal(UnitTask.Gathering,villager.CurrentTask);        
        Assert.Equal(tree.Id,villager.Gather.TargetResourceId);
        Assert.Equal(GatherPhase.MovingToResource,villager.Gather.Phase);
        Assert.NotEmpty(villager.Movement.PathQueue);
        Assert.Equal(0,villager.Gather.CurrentLoad);
        Assert.Equal(ResourceType.Wood,villager.Gather.CarriedResource);
        
        //2 STEP GATHER
        SimulationTestHelper.RunTicks(world, 3);
        Assert.Equal(UnitTask.Gathering,villager.CurrentTask);
        Assert.Equal(GatherPhase.Gathering,villager.Gather.Phase);
        Assert.Equal(tree.Id,villager.Gather.TargetResourceId);
        Assert.Equal(1,villager.Gather.CurrentLoad);

        //3 STEP MOVE TO DEPOSIT
        SimulationTestHelper.RunTicks(world, 19);
        Assert.Equal(20, villager.Gather.CurrentLoad);
        Assert.NotNull(villager.Gather.DepositPosition);
        Assert.Equal(UnitTask.Gathering, villager.CurrentTask);
        Assert.Equal(GatherPhase.MovingToDeposit, villager.Gather.Phase);

      
        //4 STEP DEPOSIT
        SimulationTestHelper.RunTicks(world, 3);
        Assert.Equal(20, villager.Gather.CurrentLoad);
        Assert.Equal(UnitTask.Gathering, villager.CurrentTask);
        Assert.Equal(GatherPhase.Depositing, villager.Gather.Phase);
        Assert.Equal(tree.Id, villager.Gather.TargetResourceId);

        //5 STEP MOVING TO RESOURCE AGAIN
        SimulationTestHelper.RunTicks(world, 1);
        Assert.Equal(0, villager.Gather.CurrentLoad);
        Assert.Equal(UnitTask.Gathering, villager.CurrentTask);
        Assert.Equal(GatherPhase.MovingToResource, villager.Gather.Phase);
        Assert.Equal(20, player.Economy.Get(ResourceType.Wood));
        Assert.Equal(tree.Id, villager.Gather.TargetResourceId);
        Assert.True(tree.Amount < initialAmount);
                // Assert
        // Assert.True(player.Economy.Get(ResourceType.Wood) > 0);

        // Assert.True(tree.Amount < initialAmount);
        // Assert.Equal(
        //     new GridPosition(5, 5),
        //     villager.Movement.Destination);
        //Assert.NotEmpty(villager.Movement.PathQueue);
    }

    [Fact]
    [Trait("Category", "GatheringSystem")]
    [Trait("Category", "Gathering")]
    [Trait("Category", "Gathering.Loop")]
    public void GatherLoop_ShouldRetarget_WhenResourceIsDepleted()
    {
        // Arrange
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();

        var player = world.GetPlayerById(1)!;

        var villager = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            player.Id,
            new GridPosition(1, 1));

        world.AddEntity(villager);

        var tree1 = new Tree(new GridPosition(5, 1));
        var tree2 = new Tree(new GridPosition(8, 1));

        world.AddResource(tree1);
        world.AddResource(tree2);

        var townCenter = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            player.Id,
            new GridPosition(1, 5));

        world.AddEntity(townCenter);

        world.AddCommand(new GatherCommand
        {
            UnitIds = [villager.Id],
            ResourceId = tree1.Id
        });

        // Start gathering
        SimulationTestHelper.RunTicks(world, 5);

        Assert.Equal(tree1.Id, villager.Gather.TargetResourceId);

        // Force depletion
        tree1.Amount = 0;

        // Cleanup + retarget
        SimulationTestHelper.RunTicks(world, 2);
        Assert.Equal(
            GatherPhase.MovingToDeposit,
            villager.Gather.Phase);
        SimulationTestHelper.RunTicks(world, 5);

       // Assert.True(GatherActions.CanContinueGathering(world, villager));
        Assert.Equal(UnitTask.Gathering, villager.CurrentTask);
        Assert.Equal(tree2.Id, villager.Gather.TargetResourceId);
        Assert.Equal(
            GatherPhase.MovingToResource,
            villager.Gather.Phase);
    }
    [Fact]
    [Trait("Category", "GatheringSystem")]
    [Trait("Category", "Gathering")]
    [Trait("Category", "Gathering.Loop")]
    public void GatherLoop_ShouldAllowMultipleVillagersToGatherSameResource()
    {
        // Arrange
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();

        var villager1 = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            1,
            new GridPosition(1, 1));

        var villager2 = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            1,
            new GridPosition(2, 1));

        world.AddEntity(villager1);
        world.AddEntity(villager2);

        var townCenter = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            1,
            new GridPosition(1, 5));

        townCenter.IsCompleted = true;

        world.AddEntity(townCenter);

        var tree = new Tree(new GridPosition(5, 1));
        world.AddResource(tree);

        world.AddCommand(new GatherCommand
        {
            UnitIds = [villager1.Id],
            ResourceId = tree.Id
        });

        world.AddCommand(new GatherCommand
        {
            UnitIds = [villager2.Id],
            ResourceId = tree.Id
        });

        // Act
        SimulationTestHelper.RunTicks(world, 20);
            
        // Assert
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
        // Arrange
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();

        var villager1 = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            1,
            new GridPosition(1, 1));

        var villager2 = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            2,
            new GridPosition(2, 1));

        world.AddEntity(villager1);
        world.AddEntity(villager2);

        var townCenter1 = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            1,
            new GridPosition(1, 5));

        townCenter1.IsCompleted = true;

        var townCenter2 = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            2,
            new GridPosition(10, 5));

        townCenter2.IsCompleted = true;

        world.AddEntity(townCenter1);
        world.AddEntity(townCenter2);

        var tree = new Tree(new GridPosition(5, 1));
        world.AddResource(tree);

        world.AddCommand(new GatherCommand
        {
            UnitIds = [villager1.Id],
            ResourceId = tree.Id
        });

        world.AddCommand(new GatherCommand
        {
            UnitIds = [villager2.Id],
            ResourceId = tree.Id
        });

        // Act
        SimulationTestHelper.RunTicks(world, 20);

        var player1 = world.GetPlayerById(1)!;
        var player2 = world.GetPlayerById(2)!;

        // Assert
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
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();

        var unit = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            1,
            new GridPosition(5, 5));
        var tree = new Tree(new GridPosition(6, 5));

        world.AddResource(tree);

        unit.CurrentTask = UnitTask.Gathering;
        unit.Gather.TargetResourceId = tree.Id;
        unit.Gather.Phase = GatherPhase.Gathering;
        unit.Gather.CurrentLoad = 20;
        unit.Gather.CarriedResource = ResourceType.Wood;
        world.AddEntity(unit);

        GatherSystem.Update(world);

        Assert.Equal(GatherPhase.WaitingForDeposit, unit.Gather.Phase);
        Assert.Equal(20, unit.Gather.CurrentLoad);
        Assert.Equal(ResourceType.Wood, unit.Gather.CarriedResource);
        Assert.Equal(UnitTask.Gathering, unit.CurrentTask);
    }

    [Fact]
    [Trait("Category", "GatheringSystem")]
    [Trait("Category", "Gathering")]
    [Trait("Category", "Gathering.WaitingForDeposit")]
    public void WaitingForDeposit_ShouldRetryPeriodically()
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
        unit.Gather.CurrentLoad = 20;
        unit.Gather.CarriedResource = ResourceType.Wood;
        world.AddEntity(unit);

        GatherSystem.Update(world);
        Assert.Equal(GatherPhase.WaitingForDeposit, unit.Gather.Phase);

        for (int i = 0; i < 3; i++)
        {
            GatherSystem.Update(world);
            Assert.Equal(GatherPhase.WaitingForDeposit, unit.Gather.Phase);
        }
    }

    [Fact]
    [Trait("Category", "GatheringSystem")]
    [Trait("Category", "Gathering")]
    [Trait("Category", "Gathering.WaitingForDeposit")]
    public void WaitingForDeposit_ShouldStopAfterMaxWaitTicks_WhenNoDepositAvailable()
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
        unit.Gather.CurrentLoad = 20;
        unit.Gather.CarriedResource = ResourceType.Wood;
        world.AddEntity(unit);

        GatherSystem.Update(world);
        Assert.Equal(GatherPhase.WaitingForDeposit, unit.Gather.Phase);

        for (int i = 0; i < 4; i++)
        {
            GatherSystem.Update(world);
        }

        Assert.Equal(GatherPhase.None, unit.Gather.Phase);
        Assert.Equal(UnitTask.Idle, unit.CurrentTask);
        Assert.Equal(20, unit.Gather.CurrentLoad);
        Assert.Equal(ResourceType.Wood, unit.Gather.CarriedResource);
    }

    [Fact]
    [Trait("Category", "GatheringSystem")]
    [Trait("Category", "Gathering")]
    [Trait("Category", "Gathering.WaitingForDeposit")]
    public void WaitingForDeposit_ShouldTransitionToMovingToDeposit_WhenDepositBecomesAvailable()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();

        var unit = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            1,
            new GridPosition(5, 5));
        var tree = new Tree(new GridPosition(6, 5));

        world.AddResource(tree);

        unit.CurrentTask = UnitTask.Gathering;
        unit.Gather.TargetResourceId = tree.Id;
        unit.Gather.Phase = GatherPhase.Gathering;
        unit.Gather.CurrentLoad = 20;
        unit.Gather.CarriedResource = ResourceType.Wood;
        world.AddEntity(unit);

        GatherSystem.Update(world);
        Assert.Equal(GatherPhase.WaitingForDeposit, unit.Gather.Phase);

        var townCenter = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            ownerId: 1,
            position: new GridPosition(1, 1));
        world.AddEntity(townCenter);

        unit.Gather.WaitingForDepositTicks = 3;
        GatherSystem.Update(world);

        Assert.Equal(GatherPhase.MovingToDeposit, unit.Gather.Phase);
        Assert.NotNull(unit.Gather.DepositPosition);
    }

    [Fact]
    [Trait("Category", "GatheringSystem")]
    [Trait("Category", "Gathering")]
    [Trait("Category", "Gathering.WaitingForDeposit")]
    public void MovingToDeposit_ShouldTransitionToWaitingForDeposit_WhenStuck()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();

        var unit = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            1,
            new GridPosition(5, 5));

        world.AddEntity(unit);

        unit.CurrentTask = UnitTask.Gathering;
        unit.Gather.Phase = GatherPhase.MovingToDeposit;
        unit.Gather.DepositPosition = new GridPosition(8, 8);
        unit.Gather.CurrentLoad = 20;
        unit.Gather.CarriedResource = ResourceType.Wood;
        unit.Gather.WaitingForDepositTicks = 1;
        unit.Movement.Destination = new GridPosition(8, 8);

        GatherSystem.Update(world);

        Assert.Equal(GatherPhase.WaitingForDeposit, unit.Gather.Phase);
        Assert.Equal(20, unit.Gather.CurrentLoad);
        Assert.Equal(ResourceType.Wood, unit.Gather.CarriedResource);
    }
}
