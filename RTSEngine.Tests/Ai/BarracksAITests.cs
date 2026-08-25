using RTSEngine.Core.AI.Brains;
using RTSEngine.Core.AI.Actions;
using RTSEngine.Core.Actions;
using RTSEngine.Core.Commands;
using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Players;
using RTSEngine.Core.State;
using RTSEngine.Core.Systems;
using RTSEngine.Core.Systems.Pathfinding;
using RTSEngine.Tests.TestHelpers;

namespace RTSEngine.Tests.AI;

public class BarracksAITests
{
    private readonly RuntimeContext _context;
    private readonly GameWorld _world;
    private readonly Player _player;

    public BarracksAITests()
    {
        _context = new RuntimeContext
        {
            World = TestWorldFactory.CreateWorldWithTwoPlayers(),
            UnitRepository = new UnitDefinitionRepository(
            [
                TestDefinitionFactory.CreateVillager(),
                TestDefinitionFactory.CreateMilitiaWithCombatStats()
            ]),
            BuildingRepository = new BuildingDefinitionRepository(
            [
                TestDefinitionFactory.CreateTownCenter(),
                TestDefinitionFactory.CreateBarracks()
            ]),
            CommandQueue = new CommandQueue(),
            PathFinder = new AStarPathFinder(new GroundMovementFilter())
        };

        _world = _context.World;
        _player = _world.GetPlayerById(1)!;
    }

    [Fact]
    [Trait("Category", "AI")]
    [Trait("Category", "Barracks")]
    public void ConstructionDecision_ShouldRequestBarracks_WhenPopReached()
    {
        var tc = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            _player.Id,
            new GridPosition(5, 5));
        tc.IsCompleted = true;
        _world.Entities.Add(tc, _player);

        var villager = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            _player.Id,
            new GridPosition(10, 10));
        _world.Entities.Add(villager, _player);

        PopulationActions.IncreaseCap(_player, 20);
        PopulationActions.AddPopulation(_player, 15);
        _player.Economy.Add(ResourceType.Wood, 200);

        new ConstructionBrain().Execute(_context, _player);
        CommandSystem.Update(_context);

        var barracks = _world.Entities.Buildings.Values
            .FirstOrDefault(b => b.OwnerId == _player.Id && b.Definition.Id == "barracks");

        Assert.NotNull(barracks);
    }

    [Fact]
    [Trait("Category", "AI")]
    [Trait("Category", "Barracks")]
    public void ConstructionDecision_ShouldNotRequestBarracks_BelowPop()
    {
        PopulationActions.IncreaseCap(_player, 20);
        PopulationActions.AddPopulation(_player, 10);
        _player.Economy.Add(ResourceType.Wood, 200);

        new ConstructionBrain().Execute(_context, _player);
        CommandSystem.Update(_context);

        var barracks = _world.Entities.Buildings.Values
            .FirstOrDefault(b => b.OwnerId == _player.Id && b.Definition.Id == "barracks");

        Assert.Null(barracks);
    }

    [Fact]
    [Trait("Category", "AI")]
    [Trait("Category", "Barracks")]
    public void ConstructionDecision_ShouldNotRequestBarracks_WhenAlreadyExists()
    {
        PopulationActions.IncreaseCap(_player, 20);
        PopulationActions.AddPopulation(_player, 15);
        _player.Economy.Add(ResourceType.Wood, 200);

        var existingBarracks = BuildingFactory.Create(
            TestDefinitionFactory.CreateBarracks(),
            _player.Id,
            new GridPosition(20, 20));
        existingBarracks.IsCompleted = true;
        _world.Entities.Add(existingBarracks, _player);

        new ConstructionBrain().Execute(_context, _player);
        CommandSystem.Update(_context);

        var barracksCount = _world.Entities.Buildings.Values
            .Count(b => b.OwnerId == _player.Id && b.Definition.Id == "barracks");

        Assert.Equal(1, barracksCount);
    }

    [Fact]
    [Trait("Category", "AI")]
    [Trait("Category", "Barracks")]
    public void ProductionDecision_ShouldTrainMilitia_WhenPopReached()
    {
        var tc = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            _player.Id,
            new GridPosition(5, 5));
        tc.IsCompleted = true;
        _world.Entities.Add(tc, _player);

        var barracks = BuildingFactory.Create(
            TestDefinitionFactory.CreateBarracks(),
            _player.Id,
            new GridPosition(20, 20));
        barracks.IsCompleted = true;
        barracks.Production.SpawnPoint = new GridPosition(22, 22);
        _world.Entities.Add(barracks, _player);

        PopulationActions.IncreaseCap(_player, 20);
        PopulationActions.AddPopulation(_player, 15);
        _player.Economy.Add(ResourceType.Food, 200);

        new ProductionBrain().Execute(_context, _player);
        CommandSystem.Update(_context);

        Assert.True(barracks.Production.IsProducing);
    }

    [Fact]
    [Trait("Category", "AI")]
    [Trait("Category", "Barracks")]
    public void ProductionDecision_ShouldTrainMilitia_UntilTargetCount()
    {
        var tc = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            _player.Id,
            new GridPosition(5, 5));
        tc.IsCompleted = true;
        _world.Entities.Add(tc, _player);

        var barracks = BuildingFactory.Create(
            TestDefinitionFactory.CreateBarracks(),
            _player.Id,
            new GridPosition(20, 20));
        barracks.IsCompleted = true;
        barracks.Production.SpawnPoint = new GridPosition(22, 22);
        _world.Entities.Add(barracks, _player);

        PopulationActions.IncreaseCap(_player, 30);
        PopulationActions.AddPopulation(_player, 15);
        _player.Economy.Add(ResourceType.Food, 500);

        for (int i = 0; i < 5; i++)
        {
            new ProductionBrain().Execute(_context, _player);
            CommandSystem.Update(_context);
        }

        Assert.True(barracks.Production.IsProducing);
    }

    [Fact]
    [Trait("Category", "AI")]
    [Trait("Category", "Barracks")]
    public void ProductionDecision_ShouldNotTrainMilitia_WhenNoFood()
    {
        var barracks = BuildingFactory.Create(
            TestDefinitionFactory.CreateBarracks(),
            _player.Id,
            new GridPosition(20, 20));
        barracks.IsCompleted = true;
        _world.Entities.Add(barracks, _player);

        PopulationActions.IncreaseCap(_player, 20);
        PopulationActions.AddPopulation(_player, 15);

        new ProductionBrain().Execute(_context, _player);
        CommandSystem.Update(_context);

        Assert.False(barracks.Production.IsProducing);
    }

    [Fact]
    [Trait("Category", "AI")]
    [Trait("Category", "Barracks")]
    public void ProductionDecision_ShouldNotTrainMilitia_WhenNoBarracks()
    {
        PopulationActions.IncreaseCap(_player, 20);
        PopulationActions.AddPopulation(_player, 15);
        _player.Economy.Add(ResourceType.Food, 200);

        new ProductionBrain().Execute(_context, _player);

        // Should not throw
    }

    [Fact]
    [Trait("Category", "AI")]
    [Trait("Category", "Barracks")]
    public void ProductionAIActions_TrainMilitia_ShouldReturnTrue()
    {
        var barracks = BuildingFactory.Create(
            TestDefinitionFactory.CreateBarracks(),
            _player.Id,
            new GridPosition(20, 20));
        barracks.IsCompleted = true;
        _world.Entities.Add(barracks, _player);

        PopulationActions.IncreaseCap(_player, 20);
        _player.Economy.Add(ResourceType.Food, 200);

        var result = ProductionAIActions.TrainMilitia(_context, barracks);

        Assert.True(result);
    }

    [Fact]
    [Trait("Category", "AI")]
    [Trait("Category", "Barracks")]
    public void ProductionAIActions_TrainMilitia_ShouldSpendFood()
    {
        var barracks = BuildingFactory.Create(
            TestDefinitionFactory.CreateBarracks(),
            _player.Id,
            new GridPosition(20, 20));
        barracks.IsCompleted = true;
        _world.Entities.Add(barracks, _player);

        PopulationActions.IncreaseCap(_player, 20);
        _player.Economy.Add(ResourceType.Food, 200);

        ProductionAIActions.TrainMilitia(_context, barracks);

        Assert.Equal(150, _player.Economy.Get(ResourceType.Food));
    }

    [Fact]
    [Trait("Category", "AI")]
    [Trait("Category", "Barracks")]
    public void ProductionAIActions_TrainMilitia_ShouldReservePopulation()
    {
        var barracks = BuildingFactory.Create(
            TestDefinitionFactory.CreateBarracks(),
            _player.Id,
            new GridPosition(20, 20));
        barracks.IsCompleted = true;
        _world.Entities.Add(barracks, _player);

        PopulationActions.IncreaseCap(_player, 20);
        _player.Economy.Add(ResourceType.Food, 200);

        ProductionAIActions.TrainMilitia(_context, barracks);

        Assert.Equal(1, _player.Population.Reserved);
    }
}

public class MilitiaCombatAIFullLoopTests
{
    private readonly RuntimeContext _context;
    private readonly GameWorld _world;
    private readonly Player _player;
    private readonly Player _enemy;

    public MilitiaCombatAIFullLoopTests()
    {
        _context = new RuntimeContext
        {
            World = TestWorldFactory.CreateWorldWithTwoPlayers(),
            UnitRepository = new UnitDefinitionRepository(
            [
                TestDefinitionFactory.CreateMilitiaWithCombatStats()
            ]),
            BuildingRepository = new BuildingDefinitionRepository([]),
            CommandQueue = new CommandQueue(),
            PathFinder = new AStarPathFinder(new GroundMovementFilter())
        };

        _world = _context.World;
        _player = _world.GetPlayerById(1)!;
        _enemy = _world.GetPlayerById(2)!;
    }

    [Fact]
    [Trait("Category", "AI")]
    [Trait("Category", "Combat")]
    public void IdleMilitia_ShouldAutoAttackEnemy()
    {
        var militia = UnitFactory.Create(
            TestDefinitionFactory.CreateMilitiaWithCombatStats(),
            _player.Id,
            new GridPosition(5, 5));
        militia.Health.CurrentHealth = 60;

        var enemy = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            _enemy.Id,
            new GridPosition(6, 5));
        enemy.Health.CurrentHealth = 50;

        _world.Entities.Add(militia, _player);
        _world.Entities.Add(enemy, _enemy);

        new CombatBrain().Execute(_context, _player);
        CommandSystem.Update(_context);

        Assert.Equal(UnitTask.Attacking, militia.CurrentTask);
        Assert.Equal(enemy.Id, militia.Combat.TargetEntityId);
    }

    [Fact]
    [Trait("Category", "AI")]
    [Trait("Category", "Combat")]
    public void BusyMilitia_ShouldNotInterruptForCombat()
    {
        var militia = UnitFactory.Create(
            TestDefinitionFactory.CreateMilitiaWithCombatStats(),
            _player.Id,
            new GridPosition(5, 5));
        militia.CurrentTask = UnitTask.Gathering;

        var enemy = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            _enemy.Id,
            new GridPosition(6, 5));

        _world.Entities.Add(militia, _player);
        _world.Entities.Add(enemy, _enemy);

        new CombatBrain().Execute(_context, _player);
        CommandSystem.Update(_context);

        Assert.Equal(UnitTask.Gathering, militia.CurrentTask);
    }

    [Fact]
    [Trait("Category", "AI")]
    [Trait("Category", "Combat")]
    public void Militia_ShouldStopAttacking_WhenTargetDies()
    {
        var militia = UnitFactory.Create(
            TestDefinitionFactory.CreateMilitiaWithCombatStats(),
            _player.Id,
            new GridPosition(5, 5));
        militia.Health.CurrentHealth = 60;

        var enemy = UnitFactory.Create(
            TestDefinitionFactory.CreateVillager(),
            _enemy.Id,
            new GridPosition(6, 5));
        enemy.Health.CurrentHealth = 1;

        _world.Entities.Add(militia, _player);
        _world.Entities.Add(enemy, _enemy);

        new CombatBrain().Execute(_context, _player);
        CommandSystem.Update(_context);

        Assert.Equal(UnitTask.Attacking, militia.CurrentTask);

        CombatSystem.Update(_context);
        CombatSystem.Update(_context);
        CombatSystem.Update(_context);

        Assert.True(enemy.IsDead);
        Assert.Equal(UnitTask.Idle, militia.CurrentTask);
    }
}
