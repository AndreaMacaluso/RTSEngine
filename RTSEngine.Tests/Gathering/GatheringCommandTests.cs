using RTSEngine.Core.Commands;
using RTSEngine.Core.Entities.Resources;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.State;
using RTSEngine.Core.Systems;
using RTSEngine.Tests.TestHelpers;
using RTSEngine.Core.Entities.States;

namespace RTSEngine.Tests.Gathering;

public class GatherCommandTests
{
    [Fact]
    [Trait("Category", "GatheringCommand")]
    [Trait("Category", "Gathering")]
    public void GatherCommand_ShouldAssignGatherTask()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();

        var unit = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            1,
            new GridPosition(1, 1));

        world.AddEntity(unit);

        var tree = new Tree(new GridPosition(5, 5));
        world.AddResource(tree);

        var command = new GatherCommand
        {
            UnitIds = [unit.Id],
            ResourceId = tree.Id
        };

        world.AddCommand(command);

        CommandSystem.Update(world);

        Assert.Equal(UnitTask.Gathering, unit.CurrentTask);
    }

    [Fact]
    [Trait("Category", "GatheringCommand")]
    [Trait("Category", "Gathering")]
    public void GatherCommand_ShouldAssignTargetResource()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();

        var unit = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            1,
            new GridPosition(1, 1));

        world.AddEntity(unit);

        var tree = new Tree(new GridPosition(5, 5));
        world.AddResource(tree);

        var command = new GatherCommand
        {
            UnitIds = [unit.Id],
            ResourceId = tree.Id
        };

        world.AddCommand(command);

        CommandSystem.Update(world);

        Assert.Equal(tree.Id, unit.Gather.TargetResourceId);
    }

    [Fact]
    [Trait("Category", "GatheringCommand")]
    [Trait("Category", "Gathering")]
    public void GatherCommand_ShouldAssignMovingToResourcePhase()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();

        var unit = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            1,
            new GridPosition(1, 1));

        world.AddEntity(unit);

        var tree = new Tree(new GridPosition(5, 5));
        world.AddResource(tree);

        world.AddCommand(new GatherCommand
        {
            UnitIds = [unit.Id],
            ResourceId = tree.Id
        });

        CommandSystem.Update(world);

        Assert.Equal(
            GatherPhase.MovingToResource,
            unit.Gather.Phase);
    }

    [Fact]
    [Trait("Category", "GatheringCommand")]
    [Trait("Category", "Gathering")]
    public void GatherCommand_ShouldAssignMovementPath()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();

        var unit = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            1,
            new GridPosition(1, 1));

        world.AddEntity(unit);

        var tree = new Tree(new GridPosition(5, 5));
        world.AddResource(tree);

        world.AddCommand(new GatherCommand
        {
            UnitIds = [unit.Id],
            ResourceId = tree.Id
        });

        CommandSystem.Update(world);

        Assert.NotEmpty(unit.Movement.PathQueue);
    }
}