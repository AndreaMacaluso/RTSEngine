using RTSEngine.Core.State;
using RTSEngine.Core.Systems;
using RTSEngine.Core.Commands;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.States;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Helpers;
using RTSEngine.Tests.TestHelpers;
using RTSEngine.Core.Settings;
using RTSEngine.Core.Systems.Pathfinding;

namespace RTSEngine.Tests.Systems;

public class GuardAndStopTests
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
    [Trait("Category", "Guard")]
    public void GuardCommand_ShouldSetUnitToGuarding()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world);
        var player = world.GetPlayerById(1)!;

        var unitDef = new UnitDefinition
        {
            Id = "militia",
            Name = "Militia",
            MaxHealth = 60,
            MovementSpeed = 1.1f,
            Category = EntityCategory.Infantry,
            MeleeAttack = 6,
            AttackRange = 1,
            MeleeArmor = 1,
            RangedArmor = 0
        };

        var unit = UnitFactory.Create(unitDef, 1, new GridPosition(5, 5));
        world.Entities.Add(unit, player);

        context.CommandQueue.Enqueue(new AttackCommand
        {
            UnitIds = [unit.Id],
            Mode = AttackMode.Guard
        });

        CommandSystem.Update(context);

        Assert.Equal(EntityState.Attacking, unit.CurrentTask);
        Assert.Equal(CombatPhase.Guarding, unit.Combat.Phase);
    }

    [Fact]
    [Trait("Category", "Guard")]
    public void GuardUnit_ShouldAttackEnemy_InRange()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world);
        var player1 = world.GetPlayerById(1)!;
        var player2 = world.GetPlayerById(2)!;

        var unitDef = new UnitDefinition
        {
            Id = "militia",
            Name = "Militia",
            MaxHealth = 60,
            MovementSpeed = 1.1f,
            Category = EntityCategory.Infantry,
            MeleeAttack = 6,
            AttackRange = 1,
            MeleeArmor = 1,
            RangedArmor = 0
        };

        var enemyDef = new UnitDefinition
        {
            Id = "militia",
            Name = "Militia",
            MaxHealth = 60,
            MovementSpeed = 1.1f,
            Category = EntityCategory.Infantry,
            MeleeAttack = 6,
            AttackRange = 1,
            MeleeArmor = 1,
            RangedArmor = 0
        };

        var guard = UnitFactory.Create(unitDef, 1, new GridPosition(5, 5));
        var enemy = UnitFactory.Create(enemyDef, 2, new GridPosition(5, 6));

        world.Entities.Add(guard, player1);
        world.Entities.Add(enemy, player2);

        context.CommandQueue.Enqueue(new AttackCommand
        {
            UnitIds = [guard.Id],
            Mode = AttackMode.Guard
        });

        CommandSystem.Update(context);

        Assert.Equal(EntityState.Attacking, guard.CurrentTask);
        Assert.Equal(CombatPhase.Guarding, guard.Combat.Phase);

        CombatSystem.Update(context);

        Assert.Equal(CombatPhase.MovingToTarget, guard.Combat.Phase);
    }

    [Fact]
    [Trait("Category", "Guard")]
    public void GuardUnit_ShouldNotChaseEnemy()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world);
        var player1 = world.GetPlayerById(1)!;
        var player2 = world.GetPlayerById(2)!;

        var unitDef = new UnitDefinition
        {
            Id = "militia",
            Name = "Militia",
            MaxHealth = 60,
            MovementSpeed = 1.1f,
            Category = EntityCategory.Infantry,
            MeleeAttack = 6,
            AttackRange = 1,
            MeleeArmor = 1,
            RangedArmor = 0
        };

        var enemyDef = new UnitDefinition
        {
            Id = "militia",
            Name = "Militia",
            MaxHealth = 60,
            MovementSpeed = 1.1f,
            Category = EntityCategory.Infantry,
            MeleeAttack = 6,
            AttackRange = 1,
            MeleeArmor = 1,
            RangedArmor = 0
        };

        var guard = UnitFactory.Create(unitDef, 1, new GridPosition(5, 5));
        var enemy = UnitFactory.Create(enemyDef, 2, new GridPosition(10, 5));

        world.Entities.Add(guard, player1);
        world.Entities.Add(enemy, player2);

        context.CommandQueue.Enqueue(new AttackCommand
        {
            UnitIds = [guard.Id],
            Mode = AttackMode.Guard
        });

        CommandSystem.Update(context);
        CombatSystem.Update(context);

        Assert.Equal(EntityState.Attacking, guard.CurrentTask);
        Assert.Equal(CombatPhase.Guarding, guard.Combat.Phase);
        Assert.Equal(new GridPosition(5, 5), guard.Position);
    }

    [Fact]
    [Trait("Category", "Stop")]
    public void StopCommand_ShouldSetUnitToIdle()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world);
        var player = world.GetPlayerById(1)!;

        var unitDef = new UnitDefinition
        {
            Id = "militia",
            Name = "Militia",
            MaxHealth = 60,
            MovementSpeed = 1.1f,
            Category = EntityCategory.Infantry,
            MeleeAttack = 6,
            AttackRange = 1,
            MeleeArmor = 1,
            RangedArmor = 0
        };

        var unit = UnitFactory.Create(unitDef, 1, new GridPosition(5, 5));
        unit.CurrentTask = EntityState.Moving;
        world.Entities.Add(unit, player);

        context.CommandQueue.Enqueue(new StopCommand
        {
            UnitIds = [unit.Id]
        });

        CommandSystem.Update(context);

        Assert.Equal(EntityState.Idle, unit.CurrentTask);
    }

    [Fact]
    [Trait("Category", "Stop")]
    public void StopCommand_ShouldClearCombatState()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world);
        var player1 = world.GetPlayerById(1)!;
        var player2 = world.GetPlayerById(2)!;

        var unitDef = new UnitDefinition
        {
            Id = "militia",
            Name = "Militia",
            MaxHealth = 60,
            MovementSpeed = 1.1f,
            Category = EntityCategory.Infantry,
            MeleeAttack = 6,
            AttackRange = 1,
            MeleeArmor = 1,
            RangedArmor = 0
        };

        var enemyDef = new UnitDefinition
        {
            Id = "militia",
            Name = "Militia",
            MaxHealth = 60,
            MovementSpeed = 1.1f,
            Category = EntityCategory.Infantry,
            MeleeAttack = 6,
            AttackRange = 1,
            MeleeArmor = 1,
            RangedArmor = 0
        };

        var unit = UnitFactory.Create(unitDef, 1, new GridPosition(5, 5));
        var enemy = UnitFactory.Create(enemyDef, 2, new GridPosition(6, 5));

        world.Entities.Add(unit, player1);
        world.Entities.Add(enemy, player2);

        CombatSystem.BeginAttack(world, unit, enemy.Id);

        Assert.Equal(EntityState.Attacking, unit.CurrentTask);

        context.CommandQueue.Enqueue(new StopCommand
        {
            UnitIds = [unit.Id]
        });

        CommandSystem.Update(context);

        Assert.Equal(EntityState.Idle, unit.CurrentTask);
        Assert.Null(unit.Combat.TargetEntityId);
    }

    [Fact]
    [Trait("Category", "AutoAttack")]
    public void IdleMilitary_ShouldAutoAttackEnemyInRange()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world);
        var player1 = world.GetPlayerById(1)!;
        var player2 = world.GetPlayerById(2)!;

        var unitDef = new UnitDefinition
        {
            Id = "militia",
            Name = "Militia",
            MaxHealth = 60,
            MovementSpeed = 1.1f,
            Category = EntityCategory.Infantry,
            MeleeAttack = 6,
            AttackRange = 1,
            MeleeArmor = 1,
            RangedArmor = 0
        };

        var enemyDef = new UnitDefinition
        {
            Id = "militia",
            Name = "Militia",
            MaxHealth = 60,
            MovementSpeed = 1.1f,
            Category = EntityCategory.Infantry,
            MeleeAttack = 6,
            AttackRange = 1,
            MeleeArmor = 1,
            RangedArmor = 0
        };

        var unit = UnitFactory.Create(unitDef, 1, new GridPosition(5, 5));
        var enemy = UnitFactory.Create(enemyDef, 2, new GridPosition(5, 6));

        world.Entities.Add(unit, player1);
        world.Entities.Add(enemy, player2);

        Assert.Equal(EntityState.Idle, unit.CurrentTask);

        CombatSystem.Update(context);

        Assert.Equal(EntityState.Attacking, unit.CurrentTask);
    }

    [Fact]
    [Trait("Category", "AutoAttack")]
    public void Villager_ShouldNotAutoAttack()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world);
        var player1 = world.GetPlayerById(1)!;
        var player2 = world.GetPlayerById(2)!;

        var villagerDef = new UnitDefinition
        {
            Id = "villager",
            Name = "Villager",
            MaxHealth = 50,
            MovementSpeed = 1.0f,
            Category = EntityCategory.Villager,
            MeleeAttack = 3,
            AttackRange = 1,
            GatherCapacity = 20
        };

        var enemyDef = new UnitDefinition
        {
            Id = "militia",
            Name = "Militia",
            MaxHealth = 60,
            MovementSpeed = 1.1f,
            Category = EntityCategory.Infantry,
            MeleeAttack = 6,
            AttackRange = 1,
            MeleeArmor = 1,
            RangedArmor = 0
        };

        var villager = UnitFactory.Create(villagerDef, 1, new GridPosition(5, 5));
        var enemy = UnitFactory.Create(enemyDef, 2, new GridPosition(5, 6));

        world.Entities.Add(villager, player1);
        world.Entities.Add(enemy, player2);

        CombatSystem.Update(context);

        Assert.Equal(EntityState.Idle, villager.CurrentTask);
    }

    [Fact]
    [Trait("Category", "AttackMove")]
    public void AttackMoveCommand_ShouldSetUnitToAttackMoving()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world);
        var player = world.GetPlayerById(1)!;

        var unitDef = new UnitDefinition
        {
            Id = "militia",
            Name = "Militia",
            MaxHealth = 60,
            MovementSpeed = 1.1f,
            Category = EntityCategory.Infantry,
            MeleeAttack = 6,
            AttackRange = 1,
            MeleeArmor = 1,
            RangedArmor = 0
        };

        var unit = UnitFactory.Create(unitDef, 1, new GridPosition(5, 5));
        world.Entities.Add(unit, player);

        context.CommandQueue.Enqueue(new AttackCommand
        {
            UnitIds = [unit.Id],
            Mode = AttackMode.AttackMove,
            TargetPosition = new GridPosition(10, 10)
        });

        CommandSystem.Update(context);

        Assert.Equal(EntityState.Attacking, unit.CurrentTask);
        Assert.Equal(CombatPhase.AttackMoving, unit.Combat.Phase);
    }

    [Fact]
    [Trait("Category", "AttackMove")]
    public void AttackMoveUnit_ShouldAttackEnemy_InPath()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world);
        var player1 = world.GetPlayerById(1)!;
        var player2 = world.GetPlayerById(2)!;

        var unitDef = new UnitDefinition
        {
            Id = "militia",
            Name = "Militia",
            MaxHealth = 60,
            MovementSpeed = 1.1f,
            Category = EntityCategory.Infantry,
            MeleeAttack = 6,
            AttackRange = 1,
            MeleeArmor = 1,
            RangedArmor = 0
        };

        var enemyDef = new UnitDefinition
        {
            Id = "militia",
            Name = "Militia",
            MaxHealth = 60,
            MovementSpeed = 1.1f,
            Category = EntityCategory.Infantry,
            MeleeAttack = 6,
            AttackRange = 1,
            MeleeArmor = 1,
            RangedArmor = 0
        };

        var unit = UnitFactory.Create(unitDef, 1, new GridPosition(5, 5));
        var enemy = UnitFactory.Create(enemyDef, 2, new GridPosition(5, 6));

        world.Entities.Add(unit, player1);
        world.Entities.Add(enemy, player2);

        context.CommandQueue.Enqueue(new AttackCommand
        {
            UnitIds = [unit.Id],
            Mode = AttackMode.AttackMove,
            TargetPosition = new GridPosition(10, 10)
        });

        CommandSystem.Update(context);

        Assert.Equal(EntityState.Attacking, unit.CurrentTask);
        Assert.Equal(CombatPhase.AttackMoving, unit.Combat.Phase);

        CombatSystem.Update(context);

        Assert.Equal(CombatPhase.MovingToTarget, unit.Combat.Phase);
    }
}
