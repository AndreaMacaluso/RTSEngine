using RTSEngine.Core.Commands;
using RTSEngine.Core.Entities.Resources;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.State;
using RTSEngine.Core.Systems;
using RTSEngine.Tests.TestHelpers;
using RTSEngine.Core.Entities.States;
using RTSEngine.Core.Entities.Definitions;

namespace RTSEngine.Tests.Gathering;

public class GatherCommandTests
{
    private readonly RuntimeContext _context;
    private readonly GameWorld _world;

    public GatherCommandTests()
    {
        _context = new RuntimeContext
        {
            World = TestWorldFactory.CreateWorldWithTwoPlayers(),
            UnitRepository = new UnitDefinitionRepository([]),
            BuildingRepository = new BuildingDefinitionRepository([])
        };

        _world = _context.World;
    }


    [Fact]
    [Trait("Category", "GatheringCommand")]
    [Trait("Category", "Gathering")]
    public void GatherCommand_ShouldAssignGatherTaskAndMovement()
    {
        var unit = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            1,
            new GridPosition(1,1));

        _world.AddEntity(unit);

        var tree = new Tree(
            new GridPosition(5,5));

        _world.AddResource(tree);

        _world.AddCommand(
            new GatherCommand
            {
                UnitIds = [unit.Id],
                ResourceId = tree.Id
            });

        CommandSystem.Update(_context);

        Assert.Equal(
            UnitTask.Gathering,
            unit.CurrentTask);

        Assert.Equal(
            tree.Id,
            unit.Gather.TargetResourceId);

        Assert.Equal(
            GatherPhase.MovingToResource,
            unit.Gather.Phase);

        Assert.NotEmpty(
            unit.Movement.PathQueue);
    }
}