using RTSEngine.Core.State;
using RTSEngine.Core.Systems;
using RTSEngine.Core.Commands;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.States;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Helpers;
using RTSEngine.Tests.TestHelpers;
using RTSEngine.Core.Settings;
using RTSEngine.Core.Systems.Pathfinding;

namespace RTSEngine.Tests.Systems;

public class GroundAttackTests
{
    private RuntimeContext CreateContext(GameWorld world)
    {
        return new RuntimeContext
        {
            World = world,
            UnitRepository = new UnitDefinitionRepository([
                TestDefinitionFactory.CreateVillager(),
                TestDefinitionFactory.CreateMilitia()
            ]),
            BuildingRepository = new BuildingDefinitionRepository([
                TestDefinitionFactory.CreateTownCenter(),
                TestDefinitionFactory.CreateBarracks()
            ]),
            CommandQueue = new CommandQueue(),
            PathFinder = new AStarPathFinder(new GroundMovementFilter()),
            Settings = new GameSettings()
        };
    }

    [Fact]
    [Trait("Category", "GroundAttack")]
    public void GroundAttackCommand_ShouldSetTargetGroundPosition()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world);
        var player = world.GetPlayerById(1)!;

        var catapultDef = new UnitDefinition
        {
            Id = "catapult",
            Name = "Catapult",
            MaxHealth = 60,
            MovementSpeed = 0.6f,
            Category = EntityCategory.Siege,
            MeleeAttack = 0,
            RangedAttack = 40,
            AttackRange = 6,
            AttackCooldownTicks = 4,
            MeleeArmor = 0,
            RangedArmor = 0
        };

        var catapult = UnitFactory.Create(catapultDef, 1, new GridPosition(5, 5));
        world.Entities.Add(catapult, player);

        context.CommandQueue.Enqueue(new AttackCommand
        {
            UnitIds = [catapult.Id],
            Mode = AttackMode.Ground,
            TargetPosition = new GridPosition(10, 5)
        });

        CommandSystem.Update(context);

        Assert.Equal(EntityState.Attacking, catapult.CurrentTask);
        Assert.Equal(new GridPosition(10, 5), catapult.Combat.TargetGroundPosition);
        Assert.Null(catapult.Combat.TargetEntityId);
    }

    [Fact]
    [Trait("Category", "GroundAttack")]
    public void GroundAttack_ShouldSpawnProjectile_WhenInRange()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world);
        var player = world.GetPlayerById(1)!;

        var catapultDef = new UnitDefinition
        {
            Id = "catapult",
            Name = "Catapult",
            MaxHealth = 60,
            MovementSpeed = 0.6f,
            Category = EntityCategory.Siege,
            MeleeAttack = 0,
            RangedAttack = 40,
            AttackRange = 6,
            AttackCooldownTicks = 4,
            MeleeArmor = 0,
            RangedArmor = 0
        };

        var catapult = UnitFactory.Create(catapultDef, 1, new GridPosition(5, 5));
        world.Entities.Add(catapult, player);

        catapult.Combat.TargetGroundPosition = new GridPosition(8, 5);
        catapult.CurrentTask = EntityState.Attacking;
        catapult.Combat.Phase = CombatPhase.Attacking;

        CombatSystem.Update(context);

        Assert.Single(context.Projectiles.Projectiles);
        Assert.False(context.Projectiles.Projectiles[0].IsSingleTarget);
        Assert.Equal(1.5f, context.Projectiles.Projectiles[0].SplashRadius);
    }

    [Fact]
    [Trait("Category", "GroundAttack")]
    public void GroundAttack_ShouldDamageUnitsInSplashRadius()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world);
        var player1 = world.GetPlayerById(1)!;
        var player2 = world.GetPlayerById(2)!;

        var catapultDef = new UnitDefinition
        {
            Id = "catapult",
            Name = "Catapult",
            MaxHealth = 60,
            MovementSpeed = 0.6f,
            Category = EntityCategory.Siege,
            MeleeAttack = 0,
            RangedAttack = 40,
            AttackRange = 6,
            AttackCooldownTicks = 4,
            MeleeArmor = 0,
            RangedArmor = 0
        };

        var militiaDef = new UnitDefinition
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

        var catapult = UnitFactory.Create(catapultDef, 1, new GridPosition(5, 5));
        world.Entities.Add(catapult, player1);

        var target1 = UnitFactory.Create(militiaDef, 2, new GridPosition(10, 5));
        target1.Health.CurrentHealth = 60;
        world.Entities.Add(target1, player2);

        var target2 = UnitFactory.Create(militiaDef, 2, new GridPosition(10, 6));
        target2.Health.CurrentHealth = 60;
        world.Entities.Add(target2, player2);

        catapult.Combat.TargetGroundPosition = new GridPosition(10, 5);
        catapult.CurrentTask = EntityState.Attacking;
        catapult.Combat.Phase = CombatPhase.Attacking;

        CombatSystem.Update(context);

        Assert.Single(context.Projectiles.Projectiles);

        for (int i = 0; i < 10; i++)
        {
            ProjectileSystem.Update(context);
        }

        Assert.Empty(context.Projectiles.Projectiles);
        Assert.Equal(20, target1.Health.CurrentHealth);
        Assert.Equal(40, target2.Health.CurrentHealth);
    }

    [Fact]
    [Trait("Category", "GroundAttack")]
    public void GroundAttack_ShouldMoveToTarget_WhenOutOfRange()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world);
        var player = world.GetPlayerById(1)!;

        var catapultDef = new UnitDefinition
        {
            Id = "catapult",
            Name = "Catapult",
            MaxHealth = 60,
            MovementSpeed = 0.6f,
            Category = EntityCategory.Siege,
            MeleeAttack = 0,
            RangedAttack = 40,
            AttackRange = 6,
            AttackCooldownTicks = 4,
            MeleeArmor = 0,
            RangedArmor = 0
        };

        var catapult = UnitFactory.Create(catapultDef, 1, new GridPosition(5, 5));
        world.Entities.Add(catapult, player);

        context.CommandQueue.Enqueue(new AttackCommand
        {
            UnitIds = [catapult.Id],
            Mode = AttackMode.Ground,
            TargetPosition = new GridPosition(20, 5)
        });

        CommandSystem.Update(context);

        Assert.Equal(EntityState.Attacking, catapult.CurrentTask);
        Assert.Equal(CombatPhase.MovingToTarget, catapult.Combat.Phase);
    }

    [Fact]
    [Trait("Category", "GroundAttack")]
    public void NonSiegeUnit_ShouldNotReceiveGroundAttack()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world);
        var player = world.GetPlayerById(1)!;

        var militiaDef = new UnitDefinition
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

        var militia = UnitFactory.Create(militiaDef, 1, new GridPosition(5, 5));
        world.Entities.Add(militia, player);

        context.CommandQueue.Enqueue(new AttackCommand
        {
            UnitIds = [militia.Id],
            Mode = AttackMode.Ground,
            TargetPosition = new GridPosition(10, 5)
        });

        CommandSystem.Update(context);

        Assert.Equal(EntityState.Idle, militia.CurrentTask);
        Assert.Null(militia.Combat.TargetGroundPosition);
    }
}
