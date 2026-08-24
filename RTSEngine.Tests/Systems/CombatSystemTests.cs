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

namespace RTSEngine.Tests.Systems;

public class CombatSystemTests
{
    [Fact]
    [Trait("Category", "Combat")]
    public void Attack_ShouldDealDamage_WhenInMeleeRange()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
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
            MovementSystem.Update(world);
            CombatSystem.Update(world);
        }

        Assert.True(target.Health.CurrentHealth < 60);
    }

    [Fact]
    [Trait("Category", "Combat")]
    public void Attack_ShouldRespectCooldown()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
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

        CombatSystem.Update(world);
        Assert.Equal(100, target.Health.CurrentHealth);

        CombatSystem.Update(world);
        Assert.Equal(90, target.Health.CurrentHealth);

        CombatSystem.Update(world);
        Assert.Equal(90, target.Health.CurrentHealth);

        CombatSystem.Update(world);
        Assert.Equal(90, target.Health.CurrentHealth);

        CombatSystem.Update(world);
        Assert.Equal(90, target.Health.CurrentHealth);

        CombatSystem.Update(world);
        Assert.Equal(80, target.Health.CurrentHealth);
    }

    [Fact]
    [Trait("Category", "Combat")]
    public void Attack_ShouldStop_WhenTargetDies()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
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
            MovementSystem.Update(world);
            CombatSystem.Update(world);
        }

        Assert.True(target.IsDead);
        Assert.Equal(UnitTask.Idle, attacker.CurrentTask);
    }

    [Fact]
    [Trait("Category", "Combat")]
    public void Attack_ShouldChaseTarget_WhenNotInMeleeRange()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
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
            MovementSystem.Update(world);
            CombatSystem.Update(world);
        }

        Assert.Equal(UnitTask.Attacking, attacker.CurrentTask);
    }

    [Fact]
    [Trait("Category", "Combat")]
    public void Attack_ShouldStop_WhenTargetNotExists()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player1 = world.GetPlayerById(1)!;

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

        var attacker = UnitFactory.Create(attackerDef, 1, new GridPosition(2, 2));

        world.Entities.Add(attacker, player1);

        CombatSystem.BeginAttack(world, attacker, 999);

        for (int i = 0; i < 10; i++)
        {
            MovementSystem.Update(world);
            CombatSystem.Update(world);
        }

        Assert.Equal(UnitTask.Idle, attacker.CurrentTask);
    }

    [Fact]
    [Trait("Category", "Combat")]
    public void Unit_ShouldDie_WhenHealthReachesZero()
    {
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
    }

    [Fact]
    [Trait("Category", "Combat")]
    // Design: unità morte non è blocking — il corpo è calpestabile.
    // La tile viene liberata quando RemoveDeadEntities rimuove l'entity dalla lista.
    public void DeadUnit_ShouldNotBlockTile()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var player = world.GetPlayerById(1)!;

        var def = new UnitDefinition
        {
            Id = "villager",
            Name = "Villager",
            MaxHealth = 50,
            MovementSpeed = 1f
        };

        var unit = UnitFactory.Create(def, 1, new GridPosition(3, 3));
        unit.Health.CurrentHealth = 50;

        world.Entities.Add(unit, player);

        Assert.True(WorldQueries.IsTileBlocked(world, 3, 3));

        unit.Health.TakeDamage(50);
        world.Entities.RebuildSpatialIndex();

        Assert.False(WorldQueries.IsTileBlocked(world, 3, 3));
    }
}
