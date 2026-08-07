using RTSEngine.Core.Commands;
using RTSEngine.Core.Entities.Resources;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.State;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Systems;
using RTSEngine.Tests.TestHelpers;
using RTSEngine.Core.Entities.Definitions;

namespace RTSEngine.Tests.Systems;

public class CommandSystemTests
{
    private readonly RuntimeContext _context;
    private readonly GameWorld _world;

    public CommandSystemTests()
    {
        _context = new RuntimeContext
        {
            World = TestWorldFactory.CreateWorld(),
            UnitRepository = new UnitDefinitionRepository([]),
            BuildingRepository = new BuildingDefinitionRepository([])
        };

        _world = _context.World;
    }


    [Fact]
    public void MoveCommand_ShouldSetTargetPosition()
    {
        var unit = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            1,
            new GridPosition(1,1));

        _world.AddEntity(unit);

        _world.AddCommand(new MoveCommand
        {
            UnitIds = [unit.Id],
            Target = new GridPosition(5,5)
        });

        CommandSystem.Update(_context);

        Assert.Equal(
            new GridPosition(5,5),
            unit.Movement.Destination);
    }


    [Fact]
    public void GatherCommand_ShouldSetGatherTask()
    {
        var unit = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            1,
            new GridPosition(1,1));

        _world.AddEntity(unit);

        var tree = new Tree(
            new GridPosition(2,3));

        _world.AddResource(tree);

        _world.AddCommand(new GatherCommand
        {
            UnitIds = [unit.Id],
            ResourceId = tree.Id
        });

        CommandSystem.Update(_context);

        Assert.Equal(
            UnitTask.Gathering,
            unit.CurrentTask);
    }


    [Fact]
    public void GatherCommand_ShouldAssignTargetResourceId()
    {
        var unit = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            1,
            new GridPosition(1,1));

        _world.AddEntity(unit);

        var tree = new Tree(
            new GridPosition(2,3));

        _world.AddResource(tree);

        _world.AddCommand(new GatherCommand
        {
            UnitIds = [unit.Id],
            ResourceId = tree.Id
        });

        CommandSystem.Update(_context);

        Assert.Equal(
            tree.Id,
            unit.Gather.TargetResourceId);
    }


    [Fact]
    public void GatherCommand_ShouldGenerateMovementPath()
    {
        var unit = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            1,
            new GridPosition(1,1));

        _world.AddEntity(unit);

        var tree = new Tree(
            new GridPosition(2,3));

        _world.AddResource(tree);

        _world.AddCommand(new GatherCommand
        {
            UnitIds = [unit.Id],
            ResourceId = tree.Id
        });

        CommandSystem.Update(_context);

        Assert.NotEmpty(
            unit.Movement.PathQueue);
    }


    [Fact]
    public void GatherCommand_ShouldIgnoreMissingResource()
    {
        var unit = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            1,
            new GridPosition(1,1));

        _world.AddEntity(unit);

        _world.AddCommand(new GatherCommand
        {
            UnitIds = [unit.Id],
            ResourceId = 999
        });

        CommandSystem.Update(_context);

        Assert.Equal(
            UnitTask.Idle,
            unit.CurrentTask);
    }


    [Fact]
    public void GatherCommand_ShouldIgnoreMissingUnit()
    {
        var tree = new Tree(
            new GridPosition(2,3));

        _world.AddResource(tree);

        _world.AddCommand(new GatherCommand
        {
            UnitIds = [999],
            ResourceId = tree.Id
        });

        CommandSystem.Update(_context);
    }
}