using RTSEngine.Core.Commands;
using RTSEngine.Core.Entities.Resources;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.State;
using RTSEngine.Core.Systems;
using RTSEngine.Tests.TestHelpers;
namespace RTSEngine.Tests.Systems;

public class CommandSystemTests
{
    [Fact]
    public void MoveCommand_ShouldSetTargetPosition()
    {
        var world = TestWorldFactory.CreateWorld();

        var unit = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            1,
            new GridPosition(1, 1));

        world.AddEntity(unit);

        world.AddCommand(new MoveCommand
        {
            UnitIds = [unit.Id],
            Target = new GridPosition(5, 5)
        });

        CommandSystem.Update(world);

        Assert.Equal(
            new GridPosition(5, 5),
            unit.Movement.Destination);
    }

    [Fact]
    public void GatherCommand_ShouldSetGatherTask()
    {
        var world = TestWorldFactory.CreateWorld();

        var unit = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            1,
            new GridPosition(1, 1));

        world.AddEntity(unit);
        var tree = new Tree(
            new GridPosition(2, 3));
       

        world.AddResource(tree);

        world.AddCommand(new GatherCommand
        {
            UnitIds = [unit.Id],
            ResourceId = tree.Id
        });

        CommandSystem.Update(world);

        Assert.Equal(
            UnitTask.Gathering,
            unit.CurrentTask);
    }

    [Fact]
    public void GatherCommand_ShouldAssignTargetResourceId()
    {
        var world = TestWorldFactory.CreateWorld();

        var unit = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            1,
            new GridPosition(1, 1));

        world.AddEntity(unit);

        var tree = new Tree(
            new GridPosition(2, 3));

        world.AddResource(tree);
        world.AddCommand(new GatherCommand
        {
            UnitIds = [unit.Id],
            ResourceId = tree.Id
        });

        CommandSystem.Update(world);

        Assert.Equal(
            tree.Id,
            unit.Gather.TargetResourceId);
    }

    [Fact]
    public void GatherCommand_ShouldGenerateMovementPath()
    {
        var world = TestWorldFactory.CreateWorld();

        var unit = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            1,
            new GridPosition(1, 1));

        world.AddEntity(unit);

        var tree = new Tree(
            new GridPosition(2, 3));

        world.AddResource(tree);

        world.AddCommand(new GatherCommand
        {
            UnitIds = [unit.Id],
            ResourceId = tree.Id
        });

        CommandSystem.Update(world);

        Assert.NotEmpty(unit.Movement.PathQueue);
    }

    [Fact]
    public void GatherCommand_ShouldIgnoreMissingResource()
    {
        var world = TestWorldFactory.CreateWorld();

        var unit = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            1,
            new GridPosition(1, 1));

        world.AddEntity(unit);

        world.AddCommand(new GatherCommand
        {
            UnitIds = [unit.Id],
            ResourceId = 999
        });

        CommandSystem.Update(world);

        Assert.Equal(
            UnitTask.Idle,
            unit.CurrentTask);
    }

    [Fact]
    public void GatherCommand_ShouldIgnoreMissingUnit()
    {
        var world = TestWorldFactory.CreateWorld();

        var tree = new Tree(
            new GridPosition(2, 3));

        world.AddResource(tree);

        world.AddCommand(new GatherCommand
        {
            UnitIds = [999],
            ResourceId = tree.Id
        });

        CommandSystem.Update(world);
    }
}