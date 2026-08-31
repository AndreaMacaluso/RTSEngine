using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Systems;
using RTSEngine.Core.State;
using RTSEngine.Core.Commands;
using RTSEngine.Core.Entities.States;
using RTSEngine.Core.Helpers;
using RTSEngine.Tests.TestHelpers;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Settings;
using RTSEngine.Core.Systems.Pathfinding;

namespace RTSEngine.Tests.Systems;

public class CombatSystemTests
{
    private RuntimeContext CreateContext(GameWorld world)
    {
        return new RuntimeContext
        {
            World = world,
            UnitRepository = new UnitDefinitionRepository([]),
            BuildingRepository = new BuildingDefinitionRepository([]),
            CommandQueue = new CommandQueue(),
            PathFinder = new AStarPathFinder(new GroundMovementFilter()),
            Settings = new GameSettings()
        };
    }

    [Fact]
    [Trait("Category", "Combat")]
    public void Attack_ShouldDealDamage_WhenInMeleeRange()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world);
        var player1 = world.GetPlayerById(1)!;
        var player2 = world.GetPlayerById(2)!;

        var attackerDef = new UnitDefinition
        {
            Id = "militia",
            Name = "Militia",
            MaxHealth = 60,
            MovementSpeed = 1f,
            AttackDamage = 6,
            AttackRange = 1,
            AttackCooldownTicks = 1
        };

        var targetDef = new UnitDefinition
        {
            Id = "militia",
            Name = "Militia",
            MaxHealth = 60,
            MovementSpeed = 1f,
            AttackDamage = 6
        };

        var attacker = UnitFactory.Create(attackerDef, 1, new GridPosition(2, 2));
        var target = UnitFactory.Create(targetDef, 2, new GridPosition(3, 2));

        world.Entities.Add(attacker, player1);
        world.Entities.Add(target, player2);

        CombatSystem.BeginAttack(world, attacker, target.Id);

        Assert.Equal(UnitTask.Attacking, attacker.CurrentTask);

        for (int i = 0; i < 10; i++)
        {
            MovementSystem.Update(context);
            CombatSystem.Update(context);
        }

        Assert.True(target.Health.CurrentHealth < 60);
    }

    [Fact]
    [Trait("Category", "Combat")]
    public void Attack_ShouldRespectCooldown()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world);
        var player1 = world.GetPlayerById(1)!;
        var player2 = world.GetPlayerById(2)!;

        var attackerDef = new UnitDefinition
        {
            Id = "militia",
            Name = "Militia",
            MaxHealth = 60,
            MovementSpeed = 1f,
            AttackDamage = 10,
            AttackRange = 1,
            AttackCooldownTicks = 4
        };

        var targetDef = new UnitDefinition
        {
            Id = "militia",
            Name = "Militia",
            MaxHealth = 100,
            MovementSpeed = 1f
        };

        var attacker = UnitFactory.Create(attackerDef, 1, new GridPosition(2, 2));
        var target = UnitFactory.Create(targetDef, 2, new GridPosition(3, 2));
        target.Health.CurrentHealth = 100;

        world.Entities.Add(attacker, player1);
        world.Entities.Add(target, player2);

        CombatSystem.BeginAttack(world, attacker, target.Id);

        CombatSystem.Update(context);
        Assert.Equal(100, target.Health.CurrentHealth);

        CombatSystem.Update(context);
        Assert.Equal(90, target.Health.CurrentHealth);

        CombatSystem.Update(context);
        Assert.Equal(90, target.Health.CurrentHealth);

        CombatSystem.Update(context);
        Assert.Equal(90, target.Health.CurrentHealth);

        CombatSystem.Update(context);
        Assert.Equal(90, target.Health.CurrentHealth);

        CombatSystem.Update(context);
        Assert.Equal(80, target.Health.CurrentHealth);
    }

    [Fact]
    [Trait("Category", "Combat")]
    public void Attack_ShouldStop_WhenTargetIsGone()
    {
        // Scenario 1: target dies during attack
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world);
        var player1 = world.GetPlayerById(1)!;
        var player2 = world.GetPlayerById(2)!;

        var attackerDef = new UnitDefinition
        {
            Id = "militia",
            Name = "Militia",
            MaxHealth = 60,
            MovementSpeed = 1f,
            AttackDamage = 30,
            AttackRange = 1,
            AttackCooldownTicks = 1
        };

        var targetDef = new UnitDefinition
        {
            Id = "villager",
            Name = "Villager",
            MaxHealth = 50,
            MovementSpeed = 1f
        };

        var attacker = UnitFactory.Create(attackerDef, 1, new GridPosition(2, 2));
        var target = UnitFactory.Create(targetDef, 2, new GridPosition(3, 2));

        world.Entities.Add(attacker, player1);
        world.Entities.Add(target, player2);

        CombatSystem.BeginAttack(world, attacker, target.Id);

        for (int i = 0; i < 20; i++)
        {
            MovementSystem.Update(context);
            CombatSystem.Update(context);
        }

        Assert.True(target.IsDead);
        Assert.Equal(UnitTask.Idle, attacker.CurrentTask);

        // Scenario 2: target does not exist
        var world2 = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context2 = CreateContext(world2);
        var player1_2 = world2.GetPlayerById(1)!;

        var attackerDef2 = new UnitDefinition
        {
            Id = "militia",
            Name = "Militia",
            MaxHealth = 60,
            MovementSpeed = 1f,
            AttackDamage = 6,
            AttackRange = 1,
            AttackCooldownTicks = 1
        };

        var attacker2 = UnitFactory.Create(attackerDef2, 1, new GridPosition(2, 2));

        world2.Entities.Add(attacker2, player1_2);

        CombatSystem.BeginAttack(world2, attacker2, 999);

        for (int i = 0; i < 10; i++)
        {
            MovementSystem.Update(context2);
            CombatSystem.Update(context2);
        }

        Assert.Equal(UnitTask.Idle, attacker2.CurrentTask);
    }

    [Fact]
    [Trait("Category", "Combat")]
    public void Attack_ShouldChaseTarget_WhenNotInMeleeRange()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world);
        var player1 = world.GetPlayerById(1)!;
        var player2 = world.GetPlayerById(2)!;

        var attackerDef = new UnitDefinition
        {
            Id = "militia",
            Name = "Militia",
            MaxHealth = 60,
            MovementSpeed = 1f,
            AttackDamage = 6,
            AttackRange = 1,
            AttackCooldownTicks = 4
        };

        var targetDef = new UnitDefinition
        {
            Id = "villager",
            Name = "Villager",
            MaxHealth = 50,
            MovementSpeed = 1f
        };

        var attacker = UnitFactory.Create(attackerDef, 1, new GridPosition(1, 1));
        var target = UnitFactory.Create(targetDef, 2, new GridPosition(5, 1));
        target.Health.CurrentHealth = 50;

        world.Entities.Add(attacker, player1);
        world.Entities.Add(target, player2);

        CombatSystem.BeginAttack(world, attacker, target.Id);

        Assert.Equal(CombatPhase.MovingToTarget, attacker.Combat.Phase);

        for (int i = 0; i < 20; i++)
        {
            MovementSystem.Update(context);
            CombatSystem.Update(context);
        }

        Assert.Equal(UnitTask.Attacking, attacker.CurrentTask);
    }

    [Fact]
    [Trait("Category", "Combat")]
    public void Unit_ShouldDie_AndReleaseTile_WhenHealthReachesZero()
    {
        // Unit dies when health reaches zero
        var def = new UnitDefinition
        {
            Id = "villager",
            Name = "Villager",
            MaxHealth = 50,
            MovementSpeed = 1f
        };

        var unit = UnitFactory.Create(def, 1, new GridPosition(1, 1));
        unit.Health.CurrentHealth = 50;

        Assert.False(unit.IsDead);

        unit.Health.TakeDamage(50);

        Assert.True(unit.IsDead);
        Assert.Equal(0, unit.Health.CurrentHealth);

        // Dead unit does not block tile
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player = world.GetPlayerById(1)!;

        var def2 = new UnitDefinition
        {
            Id = "villager",
            Name = "Villager",
            MaxHealth = 50,
            MovementSpeed = 1f
        };

        var unit2 = UnitFactory.Create(def2, 1, new GridPosition(3, 3));
        unit2.Health.CurrentHealth = 50;

        world.Entities.Add(unit2, player);

        Assert.True(WorldQueries.IsTileBlocked(world, 3, 3));

        unit2.Health.TakeDamage(50);
        world.Entities.RebuildSpatialIndex();

        Assert.False(WorldQueries.IsTileBlocked(world, 3, 3));
    }
}
