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

    [Fact]
    [Trait("Category", "AI")]
    public void Execute_ShouldAssignTwoVillagersToSameResource()
    {
        // Arrange - 2 idle villagers, 1 tree
        var villager1 = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            _player.Id,
            new GridPosition(5, 5));

        var villager2 = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            _player.Id,
            new GridPosition(6, 5));

        _world.AddEntity(villager1);
        _world.AddEntity(villager2);

        _world.AddResource(
            new Tree(new GridPosition(10, 5)));

        // Act
        GatherDecision.Execute(_world, _player);

        CommandSystem.Update(_context);

        // Assert - both should gather (min 2 per resource)
        Assert.Equal(UnitTask.Gathering, villager1.CurrentTask);
        Assert.Equal(UnitTask.Gathering, villager2.CurrentTask);
        Assert.NotNull(villager1.Gather.TargetResourceId);
        Assert.NotNull(villager2.Gather.TargetResourceId);
    }

    [Fact]
    [Trait("Category", "AI")]
    public void Execute_ShouldPrioritizeFoodOverWood()
    {
        // Arrange - 1 idle villager, 1 berry bush (food), 1 tree (wood)
        var villager = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            _player.Id,
            new GridPosition(5, 5));

        _world.AddEntity(villager);

        _world.AddResource(
            new BerryBush(new GridPosition(10, 5)));

        _world.AddResource(
            new Tree(new GridPosition(5, 10)));

        // Act
        GatherDecision.Execute(_world, _player);

        CommandSystem.Update(_context);

        // Assert - should gather food first (higher priority)
        Assert.Equal(UnitTask.Gathering, villager.CurrentTask);
        Assert.Equal(ResourceType.Food, villager.Gather.CarriedResource);
    }

    [Fact]
    [Trait("Category", "AI")]
    public void Execute_ShouldSkipResource_WhenTargetReached()
    {
        // Arrange - 1 idle villager, 1 tree, but wood >= 500
        var villager = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            _player.Id,
            new GridPosition(5, 5));

        _world.AddEntity(villager);

        _world.AddResource(
            new Tree(new GridPosition(10, 5)));

        _player.Economy.Add(ResourceType.Wood, 500);

        // Act
        GatherDecision.Execute(_world, _player);

        CommandSystem.Update(_context);

        // Assert - should not gather wood (target reached)
        Assert.Equal(UnitTask.Idle, villager.CurrentTask);
    }
}