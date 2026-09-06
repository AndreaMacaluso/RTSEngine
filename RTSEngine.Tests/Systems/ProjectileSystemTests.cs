using RTSEngine.Core.State;
using RTSEngine.Core.Systems;
using RTSEngine.Core.Entities;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Helpers;
using RTSEngine.Core.Commands;
using RTSEngine.Tests.TestHelpers;
using RTSEngine.Core.Settings;
using RTSEngine.Core.Systems.Pathfinding;

namespace RTSEngine.Tests.Systems;

public class ProjectileSystemTests
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
    [Trait("Category", "Projectile")]
    public void Projectile_ShouldMoveTowardsTarget()
    {
        var projectile = new Projectile
        {
            Id = 1,
            OwnerId = 1,
            X = 0f,
            Y = 0f,
            TargetX = 10f,
            TargetY = 0f,
            Damage = 10,
            Speed = 2.0f,
            IsActive = true
        };

        projectile.MoveTowardsTarget();

        Assert.Equal(2f, projectile.X);
        Assert.Equal(0f, projectile.Y);
        Assert.False(projectile.HasReachedTarget);
    }

    [Fact]
    [Trait("Category", "Projectile")]
    public void Projectile_ShouldReachTarget_WhenCloseEnough()
    {
        var projectile = new Projectile
        {
            Id = 1,
            OwnerId = 1,
            X = 0f,
            Y = 0f,
            TargetX = 1f,
            TargetY = 0f,
            Damage = 10,
            Speed = 2.0f,
            IsActive = true
        };

        projectile.MoveTowardsTarget();

        Assert.Equal(1f, projectile.X);
        Assert.Equal(0f, projectile.Y);
        Assert.True(projectile.HasReachedTarget);
    }

    [Fact]
    [Trait("Category", "Projectile")]
    public void Projectile_ShouldNotTrackMovingTarget()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world);
        var player1 = world.GetPlayerById(1)!;
        var player2 = world.GetPlayerById(2)!;

        var archerDef = new UnitDefinition
        {
            Id = "archer",
            Name = "Archer",
            MaxHealth = 40,
            MovementSpeed = 1.0f,
            Category = EntityCategory.Ranged,
            MeleeAttack = 0,
            RangedAttack = 5,
            AttackRange = 4,
            AttackCooldownTicks = 4,
            MeleeArmor = 0,
            RangedArmor = 1
        };

        var targetDef = new UnitDefinition
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

        var archer = UnitFactory.Create(archerDef, 1, new GridPosition(5, 5));
        var target = UnitFactory.Create(targetDef, 2, new GridPosition(10, 5));

        world.Entities.Add(archer, player1);
        world.Entities.Add(target, player2);

        ProjectileSystem.SpawnProjectile(
            context.Projectiles,
            ownerId: 1,
            originX: 5.5f,
            originY: 5.5f,
            targetEntityId: target.Id,
            targetX: 10.5f,
            targetY: 5.5f,
            damage: 5,
            speed: 2.0f,
            isSingleTarget: true);

        Assert.Single(context.Projectiles.Projectiles);

        target.Position = new GridPosition(12, 5);

        for (int i = 0; i < 3; i++)
        {
            ProjectileSystem.Update(context);
        }

        Assert.Empty(context.Projectiles.Projectiles);
    }

    [Fact]
    [Trait("Category", "Projectile")]
    public void Projectile_ShouldApplyDamage_WhenReachedTarget()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world);
        var player1 = world.GetPlayerById(1)!;
        var player2 = world.GetPlayerById(2)!;

        var targetDef = new UnitDefinition
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

        var target = UnitFactory.Create(targetDef, 2, new GridPosition(6, 5));
        target.Health.CurrentHealth = 60;
        world.Entities.Add(target, player2);

        ProjectileSystem.SpawnProjectile(
            context.Projectiles,
            ownerId: 1,
            originX: 5.5f,
            originY: 5.5f,
            targetEntityId: target.Id,
            targetX: 6.5f,
            targetY: 5.5f,
            damage: 5,
            speed: 2.0f,
            isSingleTarget: true);

        ProjectileSystem.Update(context);

        Assert.Empty(context.Projectiles.Projectiles);
        Assert.Equal(55, target.Health.CurrentHealth);
    }

    [Fact]
    [Trait("Category", "Projectile")]
    public void Projectile_ShouldContinueToLastPosition_WhenTargetDies()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world);
        var player1 = world.GetPlayerById(1)!;
        var player2 = world.GetPlayerById(2)!;

        var targetDef = new UnitDefinition
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

        var target = UnitFactory.Create(targetDef, 2, new GridPosition(10, 5));
        target.Health.CurrentHealth = 1;
        world.Entities.Add(target, player2);

        ProjectileSystem.SpawnProjectile(
            context.Projectiles,
            ownerId: 1,
            originX: 5.5f,
            originY: 5.5f,
            targetEntityId: target.Id,
            targetX: 10.5f,
            targetY: 5.5f,
            damage: 5,
            speed: 2.0f,
            isSingleTarget: true);

        target.Health.TakeDamage(1);

        ProjectileSystem.Update(context);

        Assert.Single(context.Projectiles.Projectiles);
        Assert.Equal(10.5f, context.Projectiles.Projectiles[0].TargetX);
        Assert.True(target.IsDead);
    }

    [Fact]
    [Trait("Category", "Projectile")]
    public void Projectile_ShouldBeRemoved_WhenInactive()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world);

        ProjectileSystem.SpawnProjectile(
            context.Projectiles,
            ownerId: 1,
            originX: 0f,
            originY: 0f,
            targetEntityId: null,
            targetX: 10f,
            targetY: 0f,
            damage: 10,
            speed: 2.0f);

        context.Projectiles.Projectiles[0].IsActive = false;

        ProjectileSystem.Update(context);

        Assert.Empty(context.Projectiles.Projectiles);
    }

    [Fact]
    [Trait("Category", "Projectile")]
    public void Projectile_ShouldDealDamageToBuilding()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world);
        var player2 = world.GetPlayerById(2)!;

        var building = BuildingFactory.Create(
            TestDefinitionFactory.CreateTownCenter(),
            ownerId: 2,
            position: new GridPosition(6, 5));
        building.IsCompleted = true;
        building.Health.CurrentHealth = 1000;
        world.Entities.Add(building, player2);

        ProjectileSystem.SpawnProjectile(
            context.Projectiles,
            ownerId: 1,
            originX: 5.5f,
            originY: 5.5f,
            targetEntityId: building.Id,
            targetX: 6.5f,
            targetY: 5.5f,
            damage: 40,
            speed: 2.0f,
            isSingleTarget: true);

        ProjectileSystem.Update(context);

        Assert.Empty(context.Projectiles.Projectiles);
        Assert.Equal(960, building.Health.CurrentHealth);
    }

    [Fact]
    [Trait("Category", "Projectile")]
    public void Projectile_ShouldDealSplashDamage_ToNearbyUnits()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world);
        var player2 = world.GetPlayerById(2)!;

        var targetDef = new UnitDefinition
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

        var target1 = UnitFactory.Create(targetDef, 2, new GridPosition(6, 5));
        target1.Health.CurrentHealth = 60;
        world.Entities.Add(target1, player2);

        var target2 = UnitFactory.Create(targetDef, 2, new GridPosition(6, 6));
        target2.Health.CurrentHealth = 60;
        world.Entities.Add(target2, player2);

        var target3 = UnitFactory.Create(targetDef, 2, new GridPosition(8, 5));
        target3.Health.CurrentHealth = 60;
        world.Entities.Add(target3, player2);

        ProjectileSystem.SpawnProjectile(
            context.Projectiles,
            ownerId: 1,
            originX: 5.5f,
            originY: 5.5f,
            targetEntityId: target1.Id,
            targetX: 6.5f,
            targetY: 5.5f,
            damage: 40,
            speed: 1.0f,
            isSingleTarget: false,
            splashRadius: 1.5f);

        ProjectileSystem.Update(context);

        Assert.Empty(context.Projectiles.Projectiles);
        Assert.Equal(20, target1.Health.CurrentHealth);
        Assert.Equal(40, target2.Health.CurrentHealth);
        Assert.Equal(60, target3.Health.CurrentHealth);
    }
}
