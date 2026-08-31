using RTSEngine.Core.AI.Actions;
using RTSEngine.Core.Actions;
using RTSEngine.Core.Commands;
using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Players;
using RTSEngine.Core.State;
using RTSEngine.Core.Settings;
using RTSEngine.Core.Systems.Pathfinding;
using RTSEngine.Tests.TestHelpers;

namespace RTSEngine.Tests.AI;

public class ProductionAIActionsTests
{
    private readonly RuntimeContext _context;
    private readonly GameWorld _world;
    private readonly Player _player;

    public ProductionAIActionsTests()
    {
        _context = new RuntimeContext
        {
            World = TestWorldFactory.CreateWorldWithTwoPlayers(),
            UnitRepository = new UnitDefinitionRepository(
            [
                TestDefinitionFactory.CreateVillager()
            ]),
            BuildingRepository = new BuildingDefinitionRepository(
            [
                TestDefinitionFactory.CreateTownCenter()
            ]),
            CommandQueue = new CommandQueue(),
            PathFinder = new AStarPathFinder(new GroundMovementFilter()),
            Settings = new GameSettings()
        };

        _world = _context.World;
        _player = _world.GetPlayerById(1)!;
    }

    [Fact]
    [Trait("Category", "AI")]
    public void TrainVillager_ShouldReturnTrue_WhenConditionsMet()
    {
        var tc = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            _player.Id,
            new GridPosition(5, 5));
        tc.IsCompleted = true;
        _world.Entities.Add(tc, _player);

        PopulationActions.IncreaseCap(_player, 10);
        _player.Economy.Add(ResourceType.Food, 100);

        var result = ProductionAIActions.TrainVillager(_context, tc);

        Assert.True(result);
    }

    [Fact]
    [Trait("Category", "AI")]
    public void TrainVillager_ShouldReturnFalse_WhenNoFood()
    {
        var tc = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            _player.Id,
            new GridPosition(5, 5));
        tc.IsCompleted = true;
        _world.Entities.Add(tc, _player);

        PopulationActions.IncreaseCap(_player, 10);

        var result = ProductionAIActions.TrainVillager(_context, tc);

        Assert.False(result);
    }

    [Fact]
    [Trait("Category", "AI")]
    public void TrainVillager_ShouldSpendFood_WhenSuccessful()
    {
        var tc = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            _player.Id,
            new GridPosition(5, 5));
        tc.IsCompleted = true;
        _world.Entities.Add(tc, _player);

        PopulationActions.IncreaseCap(_player, 10);
        _player.Economy.Add(ResourceType.Food, 100);

        ProductionAIActions.TrainVillager(_context, tc);

        Assert.Equal(50, _player.Economy.Get(ResourceType.Food));
    }

    [Fact]
    [Trait("Category", "AI")]
    public void TrainVillager_ShouldReservePopulation_WhenSuccessful()
    {
        var tc = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            _player.Id,
            new GridPosition(5, 5));
        tc.IsCompleted = true;
        _world.Entities.Add(tc, _player);

        PopulationActions.IncreaseCap(_player, 10);
        _player.Economy.Add(ResourceType.Food, 100);

        ProductionAIActions.TrainVillager(_context, tc);

        Assert.Equal(1, _player.Population.Reserved);
    }
}
