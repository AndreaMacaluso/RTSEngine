using RTSEngine.Core.AI;
using RTSEngine.Core.AI.Brains;
using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Players;
using RTSEngine.Core.State;
using RTSEngine.Core.Systems;
using RTSEngine.Tests.TestHelpers;

namespace RTSEngine.Tests.AI.Brains;

public class CombatBrainTests
{
    private readonly RuntimeContext _context;
    private readonly GameWorld _world;
    private readonly Player _player;
    private readonly Player _enemy;

    public CombatBrainTests()
    {
        _context = new RuntimeContext
        {
            World = TestWorldFactory.CreateWorldWithTwoPlayers(),
            UnitRepository = new UnitDefinitionRepository([
                TestDefinitionFactory.CreateMilitiaWithCombatStats()
            ]),
            BuildingRepository = new BuildingDefinitionRepository([
                TestDefinitionFactory.CreateTownCenter()
            ])
        };

        _world = _context.World;
        _player = _world.GetPlayerById(1)!;
        _enemy = _world.GetPlayerById(2)!;
    }

    [Fact]
    [Trait("Category", "AI")]
    [Trait("Category", "Brain")]
    public void ShouldAttackNearbyEnemy()
    {
        var militia = UnitFactory.Create(
            TestDefinitionFactory.CreateMilitiaWithCombatStats(),
            _player.Id,
            new GridPosition(5, 5));
        militia.Health.CurrentHealth = 60;

        var enemy = UnitFactory.Create(
            TestDefinitionFactory.CreateMilitiaWithCombatStats(),
            _enemy.Id,
            new GridPosition(6, 5));
        enemy.Health.CurrentHealth = 60;

        _world.Entities.Add(militia, _player);
        _world.Entities.Add(enemy, _enemy);

        var brain = new CombatBrain();
        brain.Execute(_context, _player);
        CommandSystem.Update(_context);

        Assert.Equal(UnitTask.Attacking, militia.CurrentTask);
        Assert.Equal(enemy.Id, militia.Combat.TargetEntityId);
    }

    [Fact]
    [Trait("Category", "AI")]
    [Trait("Category", "Brain")]
    public void ShouldIgnoreDistantEnemy()
    {
        var militia = UnitFactory.Create(
            TestDefinitionFactory.CreateMilitiaWithCombatStats(),
            _player.Id,
            new GridPosition(1, 1));

        var enemy = UnitFactory.Create(
            TestDefinitionFactory.CreateMilitiaWithCombatStats(),
            _enemy.Id,
            new GridPosition(30, 30));

        _world.Entities.Add(militia, _player);
        _world.Entities.Add(enemy, _enemy);

        var brain = new CombatBrain();
        brain.Execute(_context, _player);
        CommandSystem.Update(_context);

        Assert.Equal(UnitTask.Idle, militia.CurrentTask);
    }

    [Fact]
    [Trait("Category", "AI")]
    [Trait("Category", "Brain")]
    public void ShouldMoveToEnemyTC_WhenNoNearbyEnemy()
    {
        var militia = UnitFactory.Create(
            TestDefinitionFactory.CreateMilitiaWithCombatStats(),
            _player.Id,
            new GridPosition(5, 5));
        militia.Health.CurrentHealth = 60;

        var enemyTC = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            _enemy.Id,
            new GridPosition(30, 30));
        enemyTC.IsCompleted = true;
        enemyTC.Health.CurrentHealth = 1000;

        _world.Entities.Add(militia, _player);
        _world.Entities.Add(enemyTC, _enemy);

        var brain = new CombatBrain();
        brain.Execute(_context, _player);
        CommandSystem.Update(_context);

        Assert.Equal(UnitTask.Moving, militia.CurrentTask);
    }

    [Fact]
    [Trait("Category", "AI")]
    [Trait("Category", "Brain")]
    public void ShouldNotAttackOwnUnits()
    {
        var militia = UnitFactory.Create(
            TestDefinitionFactory.CreateMilitiaWithCombatStats(),
            _player.Id,
            new GridPosition(5, 5));

        var friendly = UnitFactory.Create(
            TestDefinitionFactory.CreateMilitiaWithCombatStats(),
            _player.Id,
            new GridPosition(6, 5));

        _world.Entities.Add(militia, _player);
        _world.Entities.Add(friendly, _player);

        var brain = new CombatBrain();
        brain.Execute(_context, _player);
        CommandSystem.Update(_context);

        Assert.Equal(UnitTask.Idle, militia.CurrentTask);
    }

    [Fact]
    [Trait("Category", "AI")]
    [Trait("Category", "Brain")]
    public void ShouldDoNothing_WhenNoEnemies()
    {
        var militia = UnitFactory.Create(
            TestDefinitionFactory.CreateMilitiaWithCombatStats(),
            _player.Id,
            new GridPosition(5, 5));

        _world.Entities.Add(militia, _player);

        var brain = new CombatBrain();
        brain.Execute(_context, _player);
        CommandSystem.Update(_context);

        Assert.Equal(UnitTask.Idle, militia.CurrentTask);
    }

    [Fact]
    [Trait("Category", "AI")]
    [Trait("Category", "Brain")]
    public void ShouldNotInterruptBusyUnits()
    {
        var militia = UnitFactory.Create(
            TestDefinitionFactory.CreateMilitiaWithCombatStats(),
            _player.Id,
            new GridPosition(5, 5));
        militia.CurrentTask = UnitTask.Gathering;

        var enemy = UnitFactory.Create(
            TestDefinitionFactory.CreateMilitiaWithCombatStats(),
            _enemy.Id,
            new GridPosition(6, 5));

        _world.Entities.Add(militia, _player);
        _world.Entities.Add(enemy, _enemy);

        var brain = new CombatBrain();
        brain.Execute(_context, _player);
        CommandSystem.Update(_context);

        Assert.Equal(UnitTask.Gathering, militia.CurrentTask);
    }
}
