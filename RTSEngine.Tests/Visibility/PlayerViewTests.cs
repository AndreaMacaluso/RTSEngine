using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Entities.Resources;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.Units;
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
public class PlayerViewTests
{

    // ------------------------------------------------------------------
    // entities
    // ------------------------------------------------------------------

    [Fact]
    public void AllVisible_ViewContainsEverything()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers(
            width: 40, height: 40, visibility: MapVisibility.AllVisible);
        SpawnUnit(world, 1, new GridPosition(2, 2));
        SpawnUnit(world, 2, new GridPosition(30, 30));
        var enemyBuilding = SpawnBuilding(world, 2, new GridPosition(35, 35));
        world.Entities.Add(new Tree(new GridPosition(38, 38)));

        var view = world.ViewFor(1);

        Assert.Equal(2, view.Units.Count);
        Assert.Contains(enemyBuilding, view.Buildings);
        Assert.Single(view.Resources);
        Assert.True(view.IsVisible(new GridPosition(35, 35)));
    }

    [Fact]
    public void OwnUnits_AlwaysPresent()
    {
        var world = CreateFogWorld();
        SpawnUnit(world, 1, new GridPosition(20, 20));
        SpawnUnit(world, 1, new GridPosition(0, 0));
        Update(world);

        var view = world.ViewFor(1);

        Assert.Equal(2, view.Units.Count);
        Assert.All(view.Units, u => Assert.Equal(1, u.OwnerId));
    }

    [Fact]
    public void VisibleEnemyUnit_Appears()
    {
        var world = CreateFogWorld();
        SpawnUnit(world, 1, new GridPosition(20, 20));
        var enemy = SpawnUnit(world, 2, new GridPosition(22, 20));
        Update(world);

        var view = world.ViewFor(1);

        Assert.Contains(enemy, view.Units);
    }

    [Fact]
    public void NeverSeenEnemyUnit_DoesNotAppear()
    {
        var world = CreateFogWorld();
        SpawnUnit(world, 1, new GridPosition(20, 20));
        var enemy = SpawnUnit(world, 2, new GridPosition(0, 0));
        Update(world);

        var view = world.ViewFor(1);

        Assert.DoesNotContain(enemy, view.Units);
        Assert.Equal(TileVisibility.Hidden, view.At(enemy.Position));
    }

    [Fact]
    public void EnemyUnitOnExploredTile_DoesNotAppear()
    {
        var world = CreateFogWorld();
        var mine = SpawnUnit(world, 1, new GridPosition(20, 20));
        var enemy = SpawnUnit(world, 2, new GridPosition(22, 20));
        Update(world);
        Assert.Contains(enemy, world.ViewFor(1).Units);

        // my unit moves away: the tile stays Explored, the enemy does not
        mine.Position = new GridPosition(0, 0);
        Update(world);

        var view = world.ViewFor(1);

        Assert.Equal(TileVisibility.Explored, view.At(enemy.Position));
        Assert.DoesNotContain(enemy, view.Units);
    }

    [Fact]
    public void VisibleEnemyBuilding_Appears()
    {
        var world = CreateFogWorld();
        SpawnUnit(world, 1, new GridPosition(20, 20));
        var enemy = SpawnBuilding(world, 2, new GridPosition(22, 20));
        Update(world);

        Assert.Contains(enemy, world.ViewFor(1).Buildings);
    }

    [Fact]
    public void RememberedEnemyBuilding_StaysInViewAfterTheFogFallsBack()
    {
        var world = CreateFogWorld();
        var unit = SpawnUnit(world, 1, new GridPosition(20, 20));
        var enemy = SpawnBuilding(world, 2, new GridPosition(22, 20));
        Update(world);
        Assert.Contains(enemy, world.ViewFor(1).Buildings);

        unit.Position = new GridPosition(0, 0);
        Update(world);

        var view = world.ViewFor(1);

        Assert.Equal(TileVisibility.Explored, view.At(enemy.Position));
        Assert.Contains(enemy, view.Buildings);
    }

    [Fact]
    public void NeverSeenEnemyBuilding_DoesNotAppear()
    {
        var world = CreateFogWorld();
        SpawnUnit(world, 1, new GridPosition(20, 20));
        var enemy = SpawnBuilding(world, 2, new GridPosition(0, 0));
        Update(world);

        Assert.DoesNotContain(enemy, world.ViewFor(1).Buildings);
    }

    [Fact]
    public void DestroyedEnemyBuilding_DisappearsFromView()
    {
        var world = CreateFogWorld();
        SpawnUnit(world, 1, new GridPosition(20, 20));
        var enemy = SpawnBuilding(world, 2, new GridPosition(22, 20));
        Update(world);
        Assert.Contains(enemy, world.ViewFor(1).Buildings);

        enemy.Health.CurrentHealth = 0;
        Update(world);

        Assert.DoesNotContain(enemy, world.ViewFor(1).Buildings);
    }

    // ------------------------------------------------------------------
    // resources (visible on Explored, never on Hidden)
    // ------------------------------------------------------------------

    [Fact]
    public void ResourceOnVisibleTile_Appears()
    {
        var world = CreateFogWorld();
        SpawnUnit(world, 1, new GridPosition(20, 20));
        var tree = new Tree(new GridPosition(21, 20));
        world.Entities.Add(tree);
        Update(world);

        Assert.Contains(tree, world.ViewFor(1).Resources);
    }

    [Fact]
    public void ResourceOnHiddenTile_DoesNotAppear()
    {
        var world = CreateFogWorld();
        SpawnUnit(world, 1, new GridPosition(20, 20));
        var tree = new Tree(new GridPosition(0, 0));
        world.Entities.Add(tree);
        Update(world);

        Assert.DoesNotContain(tree, world.ViewFor(1).Resources);
    }

    [Fact]
    public void ResourceOnExploredTile_Appears()
    {
        var world = CreateFogWorld();
        var unit = SpawnUnit(world, 1, new GridPosition(10, 10));
        var tree = new Tree(new GridPosition(11, 10));
        world.Entities.Add(tree);
        Update(world);
        Assert.Contains(tree, world.ViewFor(1).Resources);

        unit.Position = new GridPosition(30, 30);
        Update(world);

        var view = world.ViewFor(1);
        Assert.Equal(TileVisibility.Explored, view.At(tree.Position));
        Assert.Contains(tree, view.Resources);
    }

    // ------------------------------------------------------------------
    // it is live, not a copy
    // ------------------------------------------------------------------

    [Fact]
    public void ViewCreatedFirst_SeesWhatChangesAfter()
    {
        var world = CreateFogWorld();
        SpawnUnit(world, 1, new GridPosition(20, 20));
        Update(world);

        var view = world.ViewFor(1);

        var enemy = SpawnUnit(world, 2, new GridPosition(21, 20));
        var tree = new Tree(new GridPosition(19, 20));
        world.Entities.Add(tree);
        Update(world);

        Assert.Contains(enemy, view.Units);
        Assert.Contains(tree, view.Resources);
        Assert.Equal(TileVisibility.Visible, view.At(enemy.Position));
    }

    // ------------------------------------------------------------------
    // grid
    // ------------------------------------------------------------------

    [Fact]
    public void Grid_IsConsistentWithTheGate()
    {
        var world = CreateFogWorld();
        SpawnUnit(world, 1, new GridPosition(20, 20));
        SpawnUnit(world, 2, new GridPosition(22, 20));
        Update(world);

        var view = world.ViewFor(1);

        for (int y = 0; y < 40; y++)
        {
            for (int x = 0; x < 40; x++)
            {
                var position = new GridPosition(x, y);
                var tile = view.At(position);
                Assert.Equal(world.Fog.Get(1, position), tile);
                Assert.Equal(tile != TileVisibility.Hidden, view.IsSeen(position));
                Assert.Equal(tile == TileVisibility.Visible, view.IsVisible(position));
            }
        }
    }

    [Fact]
    public void EachPlayer_HasItsOwnView()
    {
        var world = CreateFogWorld();
        var mine = SpawnUnit(world, 1, new GridPosition(20, 20));
        var theirs = SpawnUnit(world, 2, new GridPosition(5, 5));
        Update(world);

        var view1 = world.ViewFor(1);
        var view2 = world.ViewFor(2);

        Assert.Contains(mine, view1.Units);
        Assert.DoesNotContain(theirs, view1.Units);
        Assert.Contains(theirs, view2.Units);
        Assert.DoesNotContain(mine, view2.Units);

        Assert.True(view1.IsVisible(new GridPosition(20, 20)));
        Assert.False(view2.IsVisible(new GridPosition(20, 20)));
        Assert.True(view2.IsVisible(new GridPosition(5, 5)));
        Assert.False(view1.IsVisible(new GridPosition(5, 5)));
    }
}
