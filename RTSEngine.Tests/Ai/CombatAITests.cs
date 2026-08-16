using RTSEngine.Core.AI.Decisions;
using RTSEngine.Core.AI.Planning;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Players;
using RTSEngine.Core.State;
using RTSEngine.Core.Systems;
using RTSEngine.Tests.TestHelpers;

namespace RTSEngine.Tests.AI;

public class CombatAITests
{
    private readonly RuntimeContext _context;
    private readonly GameWorld _world;
    private readonly Player _player;
    private readonly Player _enemy;

    public CombatAITests()
    {
        _context = new RuntimeContext
        {
            World = TestWorldFactory.CreateWorldWithTwoPlayers(),
            UnitRepository = new UnitDefinitionRepository([]),
            BuildingRepository = new BuildingDefinitionRepository([])
        };

        _world = _context.World;
        _player = _world.GetPlayerById(1)!;
        _enemy = _world.GetPlayerById(2)!;
    }

    [Fact]
    [Trait("Category", "AI")]
    [Trait("Category", "Combat")]
    public void CombatDecision_ShouldAttackNearestEnemy()
    {
        var militiaDef = new UnitDefinition
        {
            Id = "militia",
            Name = "Militia",
            MaxHealth = 60,
            MovementSpeed = 1f,
            AttackDamage = 6,
            AttackRange = 1,
            AttackCooldownTicks = 4
        };

        var enemyDef = new UnitDefinition
        {
            Id = "villager",
            Name = "Villager",
            MaxHealth = 50,
            MovementSpeed = 1f
        };

        var militia = UnitFactory.Create(
            militiaDef, _player.Id, new GridPosition(5, 5));

        var enemy = UnitFactory.Create(
            enemyDef, _enemy.Id, new GridPosition(6, 5));

        _world.AddEntity(militia);
        _world.AddEntity(enemy);

        CombatDecision.Execute(_world, _player);

        CommandSystem.Update(_context);

        Assert.Equal(UnitTask.Attacking, militia.CurrentTask);
        Assert.Equal(enemy.Id, militia.Combat.TargetEntityId);
    }

    [Fact]
    [Trait("Category", "AI")]
    [Trait("Category", "Combat")]
    public void CombatDecision_ShouldIgnoreDistantEnemies()
    {
        var militiaDef = new UnitDefinition
        {
            Id = "militia",
            Name = "Militia",
            MaxHealth = 60,
            MovementSpeed = 1f,
            AttackDamage = 6,
            AttackRange = 1,
            AttackCooldownTicks = 4
        };

        var enemyDef = new UnitDefinition
        {
            Id = "villager",
            Name = "Villager",
            MaxHealth = 50,
            MovementSpeed = 1f
        };

        var militia = UnitFactory.Create(
            militiaDef, _player.Id, new GridPosition(1, 1));

        var enemy = UnitFactory.Create(
            enemyDef, _enemy.Id, new GridPosition(30, 30));

        _world.AddEntity(militia);
        _world.AddEntity(enemy);

        CombatDecision.Execute(_world, _player);

        CommandSystem.Update(_context);

        Assert.Equal(UnitTask.Idle, militia.CurrentTask);
    }

    [Fact]
    [Trait("Category", "AI")]
    [Trait("Category", "Combat")]
    public void CombatDecision_ShouldNotAttackOwnUnits()
    {
        var militiaDef = new UnitDefinition
        {
            Id = "militia",
            Name = "Militia",
            MaxHealth = 60,
            MovementSpeed = 1f,
            AttackDamage = 6,
            AttackRange = 1,
            AttackCooldownTicks = 4
        };

        var friendlyDef = new UnitDefinition
        {
            Id = "villager",
            Name = "Villager",
            MaxHealth = 50,
            MovementSpeed = 1f
        };

        var militia = UnitFactory.Create(
            militiaDef, _player.Id, new GridPosition(5, 5));

        var friendly = UnitFactory.Create(
            friendlyDef, _player.Id, new GridPosition(6, 5));

        _world.AddEntity(militia);
        _world.AddEntity(friendly);

        CombatDecision.Execute(_world, _player);

        CommandSystem.Update(_context);

        Assert.Equal(UnitTask.Idle, militia.CurrentTask);
    }

    [Fact]
    [Trait("Category", "AI")]
    [Trait("Category", "Combat")]
    public void CombatDecision_ShouldIgnoreNonCombatUnits()
    {
        var villagerDef = new UnitDefinition
        {
            Id = "villager",
            Name = "Villager",
            MaxHealth = 50,
            MovementSpeed = 1f
        };

        var enemyDef = new UnitDefinition
        {
            Id = "villager",
            Name = "Villager",
            MaxHealth = 50,
            MovementSpeed = 1f
        };

        var villager = UnitFactory.Create(
            villagerDef, _player.Id, new GridPosition(5, 5));

        var enemy = UnitFactory.Create(
            enemyDef, _enemy.Id, new GridPosition(6, 5));

        _world.AddEntity(villager);
        _world.AddEntity(enemy);

        CombatDecision.Execute(_world, _player);

        CommandSystem.Update(_context);

        Assert.Equal(UnitTask.Idle, villager.CurrentTask);
    }

    [Fact]
    [Trait("Category", "AI")]
    [Trait("Category", "Combat")]
    public void CombatDecision_ShouldDoNothing_WhenNoEnemies()
    {
        var militiaDef = new UnitDefinition
        {
            Id = "militia",
            Name = "Militia",
            MaxHealth = 60,
            MovementSpeed = 1f,
            AttackDamage = 6,
            AttackRange = 1,
            AttackCooldownTicks = 4
        };

        var militia = UnitFactory.Create(
            militiaDef, _player.Id, new GridPosition(5, 5));

        _world.AddEntity(militia);

        CombatDecision.Execute(_world, _player);

        CommandSystem.Update(_context);

        Assert.Equal(UnitTask.Idle, militia.CurrentTask);
    }

    [Fact]
    [Trait("Category", "AI")]
    [Trait("Category", "Combat")]
    public void CombatPlanner_ShouldFindNearestEnemy()
    {
        var militiaDef = new UnitDefinition
        {
            Id = "militia",
            Name = "Militia",
            MaxHealth = 60,
            MovementSpeed = 1f,
            AttackDamage = 6
        };

        var enemyDef = new UnitDefinition
        {
            Id = "villager",
            Name = "Villager",
            MaxHealth = 50,
            MovementSpeed = 1f
        };

        var militia = UnitFactory.Create(
            militiaDef, _player.Id, new GridPosition(5, 5));

        var enemy1 = UnitFactory.Create(
            enemyDef, _enemy.Id, new GridPosition(8, 5));

        var enemy2 = UnitFactory.Create(
            enemyDef, _enemy.Id, new GridPosition(6, 5));

        _world.AddEntity(militia);
        _world.AddEntity(enemy1);
        _world.AddEntity(enemy2);

        var nearest = CombatPlanner.FindNearestEnemy(
            _world, _player, militia);

        Assert.NotNull(nearest);
        Assert.Equal(enemy2.Id, nearest.Id);
    }

    [Fact]
    [Trait("Category", "AI")]
    [Trait("Category", "Combat")]
    public void CombatDecision_ShouldNotInterruptBusyUnits()
    {
        var militiaDef = new UnitDefinition
        {
            Id = "militia",
            Name = "Militia",
            MaxHealth = 60,
            MovementSpeed = 1f,
            AttackDamage = 6,
            AttackRange = 1,
            AttackCooldownTicks = 4
        };

        var enemyDef = new UnitDefinition
        {
            Id = "villager",
            Name = "Villager",
            MaxHealth = 50,
            MovementSpeed = 1f
        };

        var militia = UnitFactory.Create(
            militiaDef, _player.Id, new GridPosition(5, 5));

        militia.CurrentTask = UnitTask.Gathering;

        var enemy = UnitFactory.Create(
            enemyDef, _enemy.Id, new GridPosition(6, 5));

        _world.AddEntity(militia);
        _world.AddEntity(enemy);

        CombatDecision.Execute(_world, _player);

        CommandSystem.Update(_context);

        Assert.Equal(UnitTask.Gathering, militia.CurrentTask);
    }
}
