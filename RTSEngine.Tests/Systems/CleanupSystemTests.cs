using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Systems;
using RTSEngine.Core.State;
using RTSEngine.Core.Entities.States;
using RTSEngine.Core.Helpers;
using RTSEngine.Tests.TestHelpers;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Settings;
using RTSEngine.Core.Events;

namespace RTSEngine.Tests.Systems;

public class CleanupSystemTests
{
    private RuntimeContext CreateContext(GameWorld world)
    {
        return SimulationTestHelper.CreateContext(world);
    }

    [Fact]
    [Trait("Category", "Cleanup")]
    public void TickDecay_ShouldDecrementDecayTicks_WhenUnitIsDecaying()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world);
        var player = world.GetPlayerById(1)!;

        var def = new UnitDefinition
        {
            Id = "militia",
            Name = "Militia",
            MaxHealth = 60,
            MovementSpeed = (FixedPoint)1f,
            MeleeAttack = 6,
            AttackRange = 1,
            AttackCooldownTicks = 4
        };

        var unit = UnitFactory.Create(def, 1, new GridPosition(2, 2));
        world.Entities.Add(unit, player);

        unit.Health.TakeDamage(60);
        unit.CurrentTask = EntityState.Decaying;
        unit.DecayTicksRemaining = 5;

        CleanupSystem.Update(context);

        Assert.Equal(4, unit.DecayTicksRemaining);
        Assert.Equal(EntityState.Decaying, unit.CurrentTask);
    }

    [Fact]
    [Trait("Category", "Cleanup")]
    public void TickDecay_ShouldSetDead_WhenDecayTicksReachesZero()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world);
        var player = world.GetPlayerById(1)!;

        var def = new UnitDefinition
        {
            Id = "militia",
            Name = "Militia",
            MaxHealth = 60,
            MovementSpeed = (FixedPoint)1f,
            MeleeAttack = 6,
            AttackRange = 1,
            AttackCooldownTicks = 4
        };

        var unit = UnitFactory.Create(def, 1, new GridPosition(2, 2));
        world.Entities.Add(unit, player);

        unit.Health.TakeDamage(60);
        unit.CurrentTask = EntityState.Decaying;
        unit.DecayTicksRemaining = 1;

        CleanupSystem.Update(context);

        Assert.Equal(0, unit.DecayTicksRemaining);
        Assert.Equal(EntityState.Dead, unit.CurrentTask);
    }

    [Fact]
    [Trait("Category", "Cleanup")]
    public void RemoveDeadUnits_ShouldRemoveUnit_WhenCurrentTaskIsDead()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world);
        var player = world.GetPlayerById(1)!;

        var def = new UnitDefinition
        {
            Id = "militia",
            Name = "Militia",
            MaxHealth = 60,
            MovementSpeed = (FixedPoint)1f,
            MeleeAttack = 6,
            AttackRange = 1,
            AttackCooldownTicks = 4
        };

        var unit = UnitFactory.Create(def, 1, new GridPosition(2, 2));
        world.Entities.Add(unit, player);

        unit.Health.TakeDamage(60);
        unit.CurrentTask = EntityState.Dead;

        CleanupSystem.Update(context);

        Assert.Null(world.Entities.GetUnitById(unit.Id));
        Assert.DoesNotContain(unit.Id, player.UnitIds);
    }

    [Fact]
    [Trait("Category", "Cleanup")]
    public void RemoveDeadUnits_ShouldNotRemoveUnit_WhenDecaying()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world);
        var player = world.GetPlayerById(1)!;

        var def = new UnitDefinition
        {
            Id = "militia",
            Name = "Militia",
            MaxHealth = 60,
            MovementSpeed = (FixedPoint)1f,
            MeleeAttack = 6,
            AttackRange = 1,
            AttackCooldownTicks = 4
        };

        var unit = UnitFactory.Create(def, 1, new GridPosition(2, 2));
        world.Entities.Add(unit, player);

        unit.Health.TakeDamage(60);
        unit.CurrentTask = EntityState.Decaying;
        unit.DecayTicksRemaining = 3;

        CleanupSystem.Update(context);

        Assert.NotNull(world.Entities.GetUnitById(unit.Id));
        Assert.Contains(unit.Id, player.UnitIds);
    }

    [Fact]
    [Trait("Category", "Cleanup")]
    public void FullDecayCycle_ShouldRemoveUnit_AfterDecayTicks()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world);
        var player = world.GetPlayerById(1)!;

        var def = new UnitDefinition
        {
            Id = "militia",
            Name = "Militia",
            MaxHealth = 60,
            MovementSpeed = (FixedPoint)1f,
            MeleeAttack = 6,
            AttackRange = 1,
            AttackCooldownTicks = 4
        };

        var unit = UnitFactory.Create(def, 1, new GridPosition(2, 2));
        world.Entities.Add(unit, player);

        unit.Health.TakeDamage(60);
        unit.CurrentTask = EntityState.Decaying;
        unit.DecayTicksRemaining = 3;

        CleanupSystem.Update(context);
        Assert.Equal(2, unit.DecayTicksRemaining);
        Assert.NotNull(world.Entities.GetUnitById(unit.Id));

        CleanupSystem.Update(context);
        Assert.Equal(1, unit.DecayTicksRemaining);
        Assert.NotNull(world.Entities.GetUnitById(unit.Id));

        CleanupSystem.Update(context);
        Assert.Equal(0, unit.DecayTicksRemaining);
        Assert.Equal(EntityState.Dead, unit.CurrentTask);

        CleanupSystem.Update(context);
        Assert.Null(world.Entities.GetUnitById(unit.Id));
        Assert.DoesNotContain(unit.Id, player.UnitIds);
    }

    [Fact]
    [Trait("Category", "Cleanup")]
    public void RemoveDeadUnits_ShouldNotAffectAliveUnits()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers();
        var context = CreateContext(world);
        var player = world.GetPlayerById(1)!;

        var def = new UnitDefinition
        {
            Id = "militia",
            Name = "Militia",
            MaxHealth = 60,
            MovementSpeed = (FixedPoint)1f,
            MeleeAttack = 6,
            AttackRange = 1,
            AttackCooldownTicks = 4
        };

        var alive = UnitFactory.Create(def, 1, new GridPosition(2, 2));
        var dead = UnitFactory.Create(def, 1, new GridPosition(3, 2));
        world.Entities.Add(alive, player);
        world.Entities.Add(dead, player);

        dead.Health.TakeDamage(60);
        dead.CurrentTask = EntityState.Dead;

        CleanupSystem.Update(context);

        Assert.NotNull(world.Entities.GetUnitById(alive.Id));
        Assert.Null(world.Entities.GetUnitById(dead.Id));
    }
}
