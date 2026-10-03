using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.States;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Helpers;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Map.Visibility;
using RTSEngine.Core.Settings;
using RTSEngine.Core.State;
using RTSEngine.Core.Systems;
using RTSEngine.Tests.TestHelpers;
using Xunit;

using static RTSEngine.Tests.TestHelpers.FogTestFactory;

namespace RTSEngine.Tests.Systems;

[Trait("Category", "Visibility")]
public class CombatFogTests
{
    private static Unit Spawn(
        GameWorld world,
        int playerId,
        GridPosition position,
        int attackRange = 1,
        int sightRange = 4)
    {
        var definition = TestDefinitionFactory.CreateMilitiaWithCombatStats(
            attackRange: attackRange);
        definition.SightRange = sightRange;

        var unit = new Unit(playerId, position, definition);
        world.Entities.Add(unit, world.GetPlayerById(playerId)!);
        return unit;
    }

    private static void Tick(RuntimeContext context)
    {
        VisibilitySystem.Update(context);
        CombatSystem.Update(context);
    }

    // ------------------------------------------------------------------
    // acquisition
    // ------------------------------------------------------------------

    [Fact]
    public void Acquisition_OnlyWhenTheTargetIsVisible()
    {
        var world = CreateFogWorld();
        // sight 6, vision 4: a target at distance 5 is in range but not in vision
        var attacker = Spawn(world, 1, new GridPosition(20, 20), attackRange: 6, sightRange: 4);
        var target = Spawn(world, 2, new GridPosition(25, 20), attackRange: 1, sightRange: 4);

        var context = SimulationTestHelper.CreateContext(world);

        Assert.True(WorldQueries.ChebyshevDistance(attacker.Position, target.Position) <= attacker.Combat.AttackRange);

        Tick(context);

        Assert.False(world.Fog.IsVisible(1, target.Position));
        Assert.Null(attacker.Combat.TargetEntityId);
        Assert.Equal(CombatPhase.Idle, attacker.Combat.Phase);

        // the target enters the vision disc
        target.Position = new GridPosition(23, 20);
        Tick(context);

        Assert.True(world.Fog.IsVisible(1, target.Position));
        Assert.Equal(target.Id, attacker.Combat.TargetEntityId);
    }

    [Fact]
    public void Acquisition_DoesNotHappenWhenTheSourceCannotSee()
    {
        var world = CreateFogWorld();
        // melee range but sight 0: the adjacent target stays Hidden
        var attacker = Spawn(world, 1, new GridPosition(21, 20), attackRange: 1, sightRange: 0);
        var target = Spawn(world, 2, new GridPosition(20, 20), attackRange: 1, sightRange: 4);

        var context = SimulationTestHelper.CreateContext(world);

        Tick(context);

        Assert.Equal(1, WorldQueries.ChebyshevDistance(attacker.Position, target.Position));
        Assert.Equal(TileVisibility.Hidden, world.Fog.Get(1, target.Position));
        Assert.Null(attacker.Combat.TargetEntityId);
        Assert.Equal(CombatPhase.Idle, attacker.Combat.Phase);
    }

    [Fact]
    public void Acquisition_InAllVisible_NoRestriction()
    {
        var world = CreateAllVisibleWorld();
        var attacker = Spawn(world, 1, new GridPosition(20, 20), attackRange: 6, sightRange: 1);
        var target = Spawn(world, 2, new GridPosition(25, 20), attackRange: 1, sightRange: 1);

        var context = SimulationTestHelper.CreateContext(world);

        Tick(context);

        // sight 1 but AllVisible: the gate is a bypass
        Assert.Equal(target.Id, attacker.Combat.TargetEntityId);
    }

    // ------------------------------------------------------------------
    // lock release
    // ------------------------------------------------------------------

    [Fact]
    public void Release_WhenTheTargetLeavesSight()
    {
        var world = CreateFogWorld();
        var attacker = Spawn(world, 1, new GridPosition(20, 20), attackRange: 1, sightRange: 4);
        var target = Spawn(world, 2, new GridPosition(21, 20), attackRange: 1, sightRange: 4);

        var context = SimulationTestHelper.CreateContext(world);

        Tick(context);
        Assert.Equal(target.Id, attacker.Combat.TargetEntityId);
        Assert.Equal(CombatPhase.MovingToTarget, attacker.Combat.Phase);

        target.Position = new GridPosition(0, 0);
        Tick(context);

        Assert.False(world.Fog.IsVisible(1, target.Position));
        Assert.Null(attacker.Combat.TargetEntityId);
        Assert.Equal(CombatPhase.Idle, attacker.Combat.Phase);
        Assert.Equal(EntityState.Idle, attacker.CurrentTask);
    }

    [Fact]
    public void Lock_HeldWhileTheTargetStaysVisible()
    {
        var world = CreateFogWorld();
        var attacker = Spawn(world, 1, new GridPosition(20, 20), attackRange: 1, sightRange: 4);
        var target = Spawn(world, 2, new GridPosition(21, 20), attackRange: 1, sightRange: 4);

        var context = SimulationTestHelper.CreateContext(world);

        for (int i = 0; i < 3; i++)
            Tick(context);

        Assert.True(world.Fog.IsVisible(1, target.Position));
        Assert.Equal(target.Id, attacker.Combat.TargetEntityId);
        Assert.False(target.IsDead);
    }

    [Fact]
    public void Lock_ReleasedIfTheTargetDies()
    {
        var world = CreateFogWorld();
        var attacker = Spawn(world, 1, new GridPosition(20, 20), attackRange: 1, sightRange: 4);
        var target = Spawn(world, 2, new GridPosition(21, 20), attackRange: 1, sightRange: 4);

        var context = SimulationTestHelper.CreateContext(world);

        Tick(context);
        Assert.Equal(target.Id, attacker.Combat.TargetEntityId);

        target.Health.CurrentHealth = 0;
        Tick(context);

        Assert.Null(attacker.Combat.TargetEntityId);
        Assert.Equal(CombatPhase.Idle, attacker.Combat.Phase);
    }
}
