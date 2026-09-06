using RTSEngine.Core.State;
using RTSEngine.Core.Systems;
using RTSEngine.Core.Commands;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Helpers;
using RTSEngine.Tests.TestHelpers;
using RTSEngine.Core.Settings;
using RTSEngine.Core.Systems.Pathfinding;

namespace RTSEngine.Tests.Systems;

public class WatchTowerTests
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
    [Trait("Category", "WatchTower")]
    public void Tower_ShootArrowAtEnemyInRange()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world);
        var player1 = world.GetPlayerById(1)!;
        var player2 = world.GetPlayerById(2)!;

        var towerDef = new BuildingDefinition
        {
            Id = "watch_tower",
            Name = "Watch Tower",
            MaxHealth = 500,
            Width = 1,
            Height = 1,
            MeleeArmor = 4,
            RangedArmor = 4,
            Attack = 6,
            AttackRange = 5,
            AttackCooldownTicks = 2,
            BuildTimeTicks = 20
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

        var tower = BuildingFactory.Create(towerDef, 1, new GridPosition(5, 5));
        tower.IsCompleted = true;
        tower.Health.CurrentHealth = 500;

        var enemy = UnitFactory.Create(enemyDef, 2, new GridPosition(7, 5));
        enemy.Health.CurrentHealth = 60;

        world.Entities.Add(tower, player1);
        world.Entities.Add(enemy, player2);

        CombatSystem.Update(context);

        Assert.Single(context.Projectiles.Projectiles);
        Assert.Equal(60, enemy.Health.CurrentHealth);
    }

    [Fact]
    [Trait("Category", "WatchTower")]
    public void Tower_ShouldNotShoot_EnemyOutOfRange()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world);
        var player1 = world.GetPlayerById(1)!;
        var player2 = world.GetPlayerById(2)!;

        var towerDef = new BuildingDefinition
        {
            Id = "watch_tower",
            Name = "Watch Tower",
            MaxHealth = 500,
            Width = 1,
            Height = 1,
            MeleeArmor = 4,
            RangedArmor = 4,
            Attack = 6,
            AttackRange = 5,
            AttackCooldownTicks = 2,
            BuildTimeTicks = 20
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

        var tower = BuildingFactory.Create(towerDef, 1, new GridPosition(5, 5));
        tower.IsCompleted = true;
        tower.Health.CurrentHealth = 500;

        var enemy = UnitFactory.Create(enemyDef, 2, new GridPosition(11, 5));
        enemy.Health.CurrentHealth = 60;

        world.Entities.Add(tower, player1);
        world.Entities.Add(enemy, player2);

        CombatSystem.Update(context);

        Assert.Empty(context.Projectiles.Projectiles);
        Assert.Equal(60, enemy.Health.CurrentHealth);
    }

    [Fact]
    [Trait("Category", "WatchTower")]
    public void Tower_ShouldRespectCooldown()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world);
        var player1 = world.GetPlayerById(1)!;
        var player2 = world.GetPlayerById(2)!;

        var towerDef = new BuildingDefinition
        {
            Id = "watch_tower",
            Name = "Watch Tower",
            MaxHealth = 500,
            Width = 1,
            Height = 1,
            MeleeArmor = 4,
            RangedArmor = 4,
            Attack = 6,
            AttackRange = 5,
            AttackCooldownTicks = 2,
            BuildTimeTicks = 20
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

        var tower = BuildingFactory.Create(towerDef, 1, new GridPosition(5, 5));
        tower.IsCompleted = true;
        tower.Health.CurrentHealth = 500;

        var enemy = UnitFactory.Create(enemyDef, 2, new GridPosition(7, 5));
        enemy.Health.CurrentHealth = 60;

        world.Entities.Add(tower, player1);
        world.Entities.Add(enemy, player2);

        CombatSystem.Update(context);
        Assert.Single(context.Projectiles.Projectiles);

        world.AdvanceTick();
        CombatSystem.Update(context);
        Assert.Single(context.Projectiles.Projectiles);

        world.AdvanceTick();
        CombatSystem.Update(context);
        Assert.Equal(2, context.Projectiles.Projectiles.Count);
    }
}
