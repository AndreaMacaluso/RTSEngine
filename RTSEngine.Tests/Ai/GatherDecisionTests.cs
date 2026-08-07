using RTSEngine.Core.AI.Decisions;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Players;
using RTSEngine.Core.State;
using RTSEngine.Core.Systems;
using RTSEngine.Tests.TestHelpers;
using RTSEngine.Core.Entities.Resources;
namespace RTSEngine.Tests.AI.Decisions;

public class GatherDecisionTests
{
    private readonly RuntimeContext _context;
    private readonly GameWorld _world;
    private readonly Player _player;

    public GatherDecisionTests()
    {
        _context = new RuntimeContext
        {
            World = TestWorldFactory.CreateWorldWithTwoPlayers(),
            UnitRepository = new UnitDefinitionRepository([]),
            BuildingRepository = new BuildingDefinitionRepository([])
        };

        _world = _context.World;
        _player = _world.GetPlayerById(1)!;
    }

    [Fact]
    [Trait("Category", "AI")]
    public void Execute_ShouldAssignIdleVillagersToNearestTree()
    {
        // Arrange
        var villager = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            _player.Id,
            new GridPosition(5, 5));

        _world.AddEntity(villager);

        _world.AddResource(
            new Tree(new GridPosition(10, 5)));

        // Act
        GatherDecision.Execute(_world, _player);

        CommandSystem.Update(_context);

        // Assert
        Assert.Equal(
            UnitTask.Gathering,
            villager.CurrentTask);

        Assert.NotNull(villager.Gather.TargetResourceId);
    }

    [Fact]
    [Trait("Category", "AI")]
    public void Execute_ShouldIgnoreBusyVillagers()
    {
        // Arrange
        var villager = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            _player.Id,
            new GridPosition(5, 5));

        villager.CurrentTask = UnitTask.Gathering;

        _world.AddEntity(villager);

        _world.AddResource(
            new Tree(new GridPosition(10, 5)));

        // Act
        GatherDecision.Execute(_world, _player);

        CommandSystem.Update(_context);

        // Assert
        Assert.Equal(
            UnitTask.Gathering,
            villager.CurrentTask);

        Assert.Null(villager.Gather.TargetResourceId);
    }

    [Fact]
    [Trait("Category", "AI")]
    public void Execute_ShouldDoNothing_WhenNoResourcesExist()
    {
        // Arrange
        var villager = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            _player.Id,
            new GridPosition(5, 5));

        _world.AddEntity(villager);

        // Act
        GatherDecision.Execute(_world, _player);

        CommandSystem.Update(_context);

        // Assert
        Assert.Equal(
            UnitTask.Idle,
            villager.CurrentTask);

        Assert.Null(villager.Gather.TargetResourceId);
    }
}