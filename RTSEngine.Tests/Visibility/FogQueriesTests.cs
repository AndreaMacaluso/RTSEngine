using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Entities.Resources;
using RTSEngine.Core.Entities.Runtime;
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

namespace RTSEngine.Tests.Visibility;

[Trait("Category", "Visibility")]
public class FogQueriesTests
{
    private static VisionScope ScopeOf(GameWorld world, int playerId)
        => world.Fog.ScopeFor(playerId);

    // ------------------------------------------------------------------
    // tiles
    // ------------------------------------------------------------------

    [Fact]
    public void IsVisible_MatchesTheGrid()
    {
        var world = CreateFogWorld();
        SpawnUnit(world, 1, new GridPosition(20, 20));
        Update(world);

        var scope = ScopeOf(world, 1);

        Assert.True(FogQueries.IsVisible(world, scope, new GridPosition(20, 20)));
        Assert.True(FogQueries.IsVisible(world, scope, new GridPosition(24, 20)));
        Assert.False(FogQueries.IsVisible(world, scope, new GridPosition(0, 0)));
    }

    [Fact]
    public void IsSeen_AlsoRecognisesExplored()
    {
        var world = CreateFogWorld();
        var unit = SpawnUnit(world, 1, new GridPosition(20, 20));
        Update(world);

        var scope = ScopeOf(world, 1);
        Assert.False(FogQueries.IsSeen(world, scope, new GridPosition(0, 0)));

        unit.Position = new GridPosition(35, 35);
        Update(world);

        Assert.False(FogQueries.IsVisible(world, scope, new GridPosition(20, 20)));
        Assert.True(FogQueries.IsSeen(world, scope, new GridPosition(20, 20)));
    }

    [Fact]
    public void Scope_DoesNotSeeOtherPlayers()
    {
        var world = CreateFogWorld();
        SpawnUnit(world, 2, new GridPosition(5, 5));
        Update(world);

        // player 2 sees their own tile, player 1 does not
        Assert.True(FogQueries.IsVisible(world, ScopeOf(world, 2), new GridPosition(5, 5)));
        Assert.False(FogQueries.IsVisible(world, ScopeOf(world, 1), new GridPosition(5, 5)));
    }

    // ------------------------------------------------------------------
    // enemy units
    // ------------------------------------------------------------------

    [Fact]
    public void GetVisibleEnemies_OnlyVisibleEnemyUnits()
    {
        var world = CreateFogWorld();
        SpawnUnit(world, 1, new GridPosition(20, 20));
        SpawnUnit(world, 2, new GridPosition(22, 20));
        SpawnUnit(world, 2, new GridPosition(0, 0));
        SpawnUnit(world, 1, new GridPosition(21, 20));
        Update(world);

        var scope = ScopeOf(world, 1);
        var result = FogQueries.GetVisibleEnemies(world, scope).ToList();

        Assert.Single(result);
        Assert.Equal(2, result[0].OwnerId);
        Assert.Equal(new GridPosition(22, 20), result[0].Position);
    }

    [Fact]
    public void GetVisibleEnemies_ExcludesUnitsOnExplored()
    {
        var world = CreateFogWorld();
        SpawnUnit(world, 1, new GridPosition(20, 20));
        var enemy = SpawnUnit(world, 2, new GridPosition(22, 20));
        Update(world);
        Assert.Single(FogQueries.GetVisibleEnemies(world, ScopeOf(world, 1)));

        enemy.Position = new GridPosition(35, 35);
        Update(world);

        Assert.Empty(FogQueries.GetVisibleEnemies(world, ScopeOf(world, 1)));
    }

    [Fact]
    public void GetVisibleEnemies_NeverOwnUnits()
    {
        var world = CreateFogWorld();
        SpawnUnit(world, 1, new GridPosition(20, 20));
        SpawnUnit(world, 2, new GridPosition(22, 20));
        Update(world);

        // without the scope exclusion every list would have 2 entries
        var from1 = FogQueries.GetVisibleEnemies(world, ScopeOf(world, 1)).ToList();
        var from2 = FogQueries.GetVisibleEnemies(world, ScopeOf(world, 2)).ToList();

        Assert.Single(from1);
        Assert.Equal(2, from1[0].OwnerId);

        Assert.Single(from2);
        Assert.Equal(1, from2[0].OwnerId);
    }

    [Fact]
    public void GetVisibleEnemies_ExcludesTheDead()
    {
        var world = CreateFogWorld();
        SpawnUnit(world, 1, new GridPosition(20, 20));
        var enemy = SpawnUnit(world, 2, new GridPosition(22, 20));
        Update(world);

        enemy.Health.CurrentHealth = 0;

        Assert.Empty(FogQueries.GetVisibleEnemies(world, ScopeOf(world, 1)));
    }

    [Fact]
    public void GetVisibleEnemies_InAllVisible_EveryEnemy()
    {
        var world = CreateAllVisibleWorld();
        SpawnUnit(world, 1, new GridPosition(20, 20));
        SpawnUnit(world, 2, new GridPosition(2, 2));
        SpawnUnit(world, 2, new GridPosition(30, 30));

        var scope = ScopeOf(world, 1);
        Assert.Equal(2, FogQueries.GetVisibleEnemies(world, scope).Count());
    }

    // ------------------------------------------------------------------
    // enemy buildings
    // ------------------------------------------------------------------

    [Fact]
    public void GetKnownEnemyBuildings_IncludesTheVisible()
    {
        var world = CreateFogWorld();
        SpawnUnit(world, 1, new GridPosition(20, 20));
        var enemy = SpawnBuilding(world, 2, new GridPosition(22, 20));
        Update(world);

        var scope = ScopeOf(world, 1);
        Assert.Contains(enemy, FogQueries.GetKnownEnemyBuildings(world, scope));
    }

    [Fact]
    public void GetKnownEnemyBuildings_IncludesRememberedAfterTheFogFallsBack()
    {
        var world = CreateFogWorld();
        var unit = SpawnUnit(world, 1, new GridPosition(20, 20));
        var enemy = SpawnBuilding(world, 2, new GridPosition(22, 20));
        Update(world);

        unit.Position = new GridPosition(35, 35);
        Update(world);

        var scope = ScopeOf(world, 1);
        Assert.False(FogQueries.IsVisible(world, scope, new GridPosition(22, 20)));
        Assert.Contains(enemy, FogQueries.GetKnownEnemyBuildings(world, scope));
    }

    [Fact]
    public void GetKnownEnemyBuildings_ExcludesNeverSeenAndDestroyed()
    {
        var world = CreateFogWorld();
        SpawnUnit(world, 1, new GridPosition(20, 20));
        var neverSeen = SpawnBuilding(world, 2, new GridPosition(0, 0));
        var seen = SpawnBuilding(world, 2, new GridPosition(22, 20));
        Update(world);

        var scope = ScopeOf(world, 1);
        Assert.DoesNotContain(neverSeen, FogQueries.GetKnownEnemyBuildings(world, scope));
        Assert.Contains(seen, FogQueries.GetKnownEnemyBuildings(world, scope));

        seen.Health.CurrentHealth = 0;
        Assert.DoesNotContain(seen, FogQueries.GetKnownEnemyBuildings(world, scope));
    }

    [Fact]
    public void GetKnownEnemyBuildings_InAllVisible_AllBuildings()
    {
        var world = CreateAllVisibleWorld();
        SpawnUnit(world, 1, new GridPosition(20, 20));
        var a = SpawnBuilding(world, 2, new GridPosition(2, 2));
        var b = SpawnBuilding(world, 2, new GridPosition(30, 30));
        var own = SpawnBuilding(world, 1, new GridPosition(25, 25));

        var scope = ScopeOf(world, 1);
        var result = FogQueries.GetKnownEnemyBuildings(world, scope).ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(a, result);
        Assert.Contains(b, result);
        Assert.DoesNotContain(own, result);
    }

    // ------------------------------------------------------------------
    // resources
    // ------------------------------------------------------------------

    [Fact]
    public void FindClosestVisibleResource_SkipsTheHiddenOne()
    {
        var world = CreateFogWorld();
        SpawnUnit(world, 1, new GridPosition(20, 20));

        // closer but hidden
        var hiddenTree = new Tree(new GridPosition(2, 2));
        world.Entities.Add(hiddenTree);

        // farther but in view
        var visibleTree = new Tree(new GridPosition(21, 20));
        world.Entities.Add(visibleTree);

        Update(world);

        var result = FogQueries.FindClosestVisibleResource(
            world, ScopeOf(world, 1), new GridPosition(20, 20), ResourceType.Wood);

        Assert.NotNull(result);
        Assert.Equal(visibleTree.Id, result!.Id);
    }

    [Fact]
    public void FindClosestVisibleResource_AcceptsTheOneOnExplored()
    {
        var world = CreateFogWorld();
        var unit = SpawnUnit(world, 1, new GridPosition(20, 20));
        var tree = new Tree(new GridPosition(21, 20));
        world.Entities.Add(tree);
        Update(world);

        unit.Position = new GridPosition(35, 35);
        Update(world);

        var scope = ScopeOf(world, 1);
        Assert.False(FogQueries.IsVisible(world, scope, tree.Position));
        Assert.True(FogQueries.IsSeen(world, scope, tree.Position));

        var result = FogQueries.FindClosestVisibleResource(
            world, scope, new GridPosition(21, 21), ResourceType.Wood);

        Assert.NotNull(result);
        Assert.Equal(tree.Id, result!.Id);
    }

    [Fact]
    public void FindClosestVisibleResource_DeterministicTieBreakOnId()
    {
        var world = CreateFogWorld();
        SpawnUnit(world, 1, new GridPosition(20, 20));

        var a = new Tree(new GridPosition(21, 20));
        var b = new Tree(new GridPosition(19, 20));
        world.Entities.Add(a);
        world.Entities.Add(b);
        Update(world);

        // same distance: lowest Id wins, always in the same order
        var first = FogQueries.FindClosestVisibleResource(
            world, ScopeOf(world, 1), new GridPosition(20, 20), ResourceType.Wood);
        var second = FogQueries.FindClosestVisibleResource(
            world, ScopeOf(world, 1), new GridPosition(20, 20), ResourceType.Wood);

        Assert.NotNull(first);
        Assert.Equal(first!.Id, second!.Id);
        Assert.Equal(Math.Min(a.Id, b.Id), first.Id);
    }

    [Fact]
    public void FindClosestVisibleResource_FiltersByType()
    {
        var world = CreateFogWorld();
        SpawnUnit(world, 1, new GridPosition(20, 20));

        var tree = new Tree(new GridPosition(21, 20));
        var gold = new GoldMine(new GridPosition(22, 20));
        world.Entities.Add(tree);
        world.Entities.Add(gold);
        Update(world);

        var scope = ScopeOf(world, 1);

        var wood = FogQueries.FindClosestVisibleResource(
            world, scope, new GridPosition(20, 20), ResourceType.Wood);
        var goldResult = FogQueries.FindClosestVisibleResource(
            world, scope, new GridPosition(20, 20), ResourceType.Gold);

        Assert.Equal(tree.Id, wood!.Id);
        Assert.Equal(gold.Id, goldResult!.Id);
    }
}
