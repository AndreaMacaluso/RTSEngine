using RTSEngine.Core.AI.Planning;
using RTSEngine.Core.AI.Decisions;
using RTSEngine.Core.AI.Actions;
using RTSEngine.Core.Actions;
using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.States;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Players;
using RTSEngine.Core.State;
using RTSEngine.Core.Systems;
using RTSEngine.Tests.TestHelpers;

namespace RTSEngine.Tests.AI;

public class ProductionPlannerTests
{
    private readonly RuntimeContext _context;
    private readonly GameWorld _world;
    private readonly Player _player;

    public ProductionPlannerTests()
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
            ])
        };

        _world = _context.World;
        _player = _world.GetPlayerById(1)!;
    }

    [Fact]
    [Trait("Category", "AI")]
    public void FindTownCenter_ShouldReturnNull_WhenNoTCExists()
    {
        var result = ProductionPlanner.FindTownCenter(_world, _player);

        Assert.Null(result);
    }

    [Fact]
    [Trait("Category", "AI")]
    public void FindTownCenter_ShouldReturnTC_WhenCompleted()
    {
        var tc = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            _player.Id,
            new GridPosition(5, 5));
        tc.IsCompleted = true;
        _world.AddEntity(tc);

        var result = ProductionPlanner.FindTownCenter(_world, _player);

        Assert.NotNull(result);
        Assert.Equal(tc.Id, result!.Id);
    }

    [Fact]
    [Trait("Category", "AI")]
    public void FindTownCenter_ShouldReturnNull_WhenNotCompleted()
    {
        var tc = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            _player.Id,
            new GridPosition(5, 5));
        tc.IsCompleted = false;
        _world.AddEntity(tc);

        var result = ProductionPlanner.FindTownCenter(_world, _player);

        Assert.Null(result);
    }

    [Fact]
    [Trait("Category", "AI")]
    public void CanTrainVillager_ShouldReturnTrue_WhenConditionsMet()
    {
        var tc = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            _player.Id,
            new GridPosition(5, 5));
        tc.IsCompleted = true;
        _world.AddEntity(tc);

        PopulationActions.IncreaseCap(_player, 10);
        _player.Economy.Add(ResourceType.Food, 100);

        var result = ProductionPlanner.CanTrainVillager(_player, tc);

        Assert.True(result);
    }

    [Fact]
    [Trait("Category", "AI")]
    public void CanTrainVillager_ShouldReturnFalse_WhenNoFood()
    {
        var tc = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            _player.Id,
            new GridPosition(5, 5));
        tc.IsCompleted = true;
        _world.AddEntity(tc);

        PopulationActions.IncreaseCap(_player, 10);

        var result = ProductionPlanner.CanTrainVillager(_player, tc);

        Assert.False(result);
    }

    [Fact]
    [Trait("Category", "AI")]
    public void CanTrainVillager_ShouldReturnFalse_WhenPopCapReached()
    {
        var tc = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            _player.Id,
            new GridPosition(5, 5));
        tc.IsCompleted = true;
        _world.AddEntity(tc);

        _player.Economy.Add(ResourceType.Food, 100);
        PopulationActions.IncreaseCap(_player, 5);
        PopulationActions.AddPopulation(_player, 5);

        var result = ProductionPlanner.CanTrainVillager(_player, tc);

        Assert.False(result);
    }

    [Fact]
    [Trait("Category", "AI")]
    public void CanTrainVillager_ShouldReturnFalse_WhenTCIsProducing()
    {
        var tc = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            _player.Id,
            new GridPosition(5, 5));
        tc.IsCompleted = true;
        tc.Production.Add(new ProductionTask("villager", 10));
        _world.AddEntity(tc);

        PopulationActions.IncreaseCap(_player, 10);
        _player.Economy.Add(ResourceType.Food, 100);

        var result = ProductionPlanner.CanTrainVillager(_player, tc);

        Assert.False(result);
    }

    [Fact]
    [Trait("Category", "AI")]
    public void CountHouses_ShouldReturnZero_WhenNoHouses()
    {
        var tc = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            _player.Id,
            new GridPosition(5, 5));
        tc.IsCompleted = true;
        _world.AddEntity(tc);

        var count = ProductionPlanner.CountHouses(_world, _player);

        Assert.Equal(0, count);
    }

    [Fact]
    [Trait("Category", "AI")]
    public void CountHouses_ShouldCountCompletedHouses()
    {
        var tc = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            _player.Id,
            new GridPosition(5, 5));
        tc.IsCompleted = true;
        _world.AddEntity(tc);

        var houseDef = TestDefinitionFactory.CreateHouseWithCost();
        var house1 = BuildingFactory.Create(houseDef, _player.Id, new GridPosition(10, 10));
        house1.IsCompleted = true;
        _world.AddEntity(house1);

        var house2 = BuildingFactory.Create(houseDef, _player.Id, new GridPosition(15, 15));
        house2.IsCompleted = true;
        _world.AddEntity(house2);

        var count = ProductionPlanner.CountHouses(_world, _player);

        Assert.Equal(2, count);
    }
}

public class ProductionDecisionTests
{
    private readonly RuntimeContext _context;
    private readonly GameWorld _world;
    private readonly Player _player;

    public ProductionDecisionTests()
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
            ])
        };

        _world = _context.World;
        _player = _world.GetPlayerById(1)!;
    }

    [Fact]
    [Trait("Category", "AI")]
    public void Execute_ShouldTrainVillager_WhenConditionsMet()
    {
        var tc = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            _player.Id,
            new GridPosition(5, 5));
        tc.IsCompleted = true;
        tc.Production.SpawnPoint = new GridPosition(7, 7);
        _world.AddEntity(tc);

        PopulationActions.IncreaseCap(_player, 10);
        _player.Economy.Add(ResourceType.Food, 100);

        ProductionDecision.Execute(_context, _player);
        CommandSystem.Update(_context);

        Assert.True(tc.Production.IsProducing);
    }

    [Fact]
    [Trait("Category", "AI")]
    public void Execute_ShouldNotTrain_WhenNoFood()
    {
        var tc = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            _player.Id,
            new GridPosition(5, 5));
        tc.IsCompleted = true;
        _world.AddEntity(tc);

        PopulationActions.IncreaseCap(_player, 10);

        ProductionDecision.Execute(_context, _player);

        Assert.False(tc.Production.IsProducing);
    }

    [Fact]
    [Trait("Category", "AI")]
    public void Execute_ShouldNotTrain_WhenNoTC()
    {
        PopulationActions.IncreaseCap(_player, 10);
        _player.Economy.Add(ResourceType.Food, 100);

        ProductionDecision.Execute(_context, _player);

        // Should not throw, just skip
    }
}

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
            ])
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
        _world.AddEntity(tc);

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
        _world.AddEntity(tc);

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
        _world.AddEntity(tc);

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
        _world.AddEntity(tc);

        PopulationActions.IncreaseCap(_player, 10);
        _player.Economy.Add(ResourceType.Food, 100);

        ProductionAIActions.TrainVillager(_context, tc);

        Assert.Equal(1, _player.Population.Reserved);
    }
}
