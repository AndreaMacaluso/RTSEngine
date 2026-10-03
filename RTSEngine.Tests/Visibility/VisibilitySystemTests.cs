using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Definitions;
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
public class VisibilitySystemTests
{
    private static GridPosition At(int x, int y) => new(x, y);

    // ------------------------------------------------------------------
    // coverage
    // ------------------------------------------------------------------

    [Fact]
    public void MarksVisible_AroundTheUnit()
    {
        var world = CreateFogWorld();
        SpawnUnit(world, playerId: 1, new GridPosition(20, 20));

        Update(world);

        Assert.Equal(TileVisibility.Visible, world.Fog.Get(1, At(20, 20)));
        Assert.Equal(TileVisibility.Visible, world.Fog.Get(1, At(24, 20)));
        Assert.Equal(TileVisibility.Visible, world.Fog.Get(1, At(20, 24)));
        Assert.Equal(TileVisibility.Hidden, world.Fog.Get(1, At(0, 0)));

        // corner of the sight-4 bounding square: outside the disc
        Assert.Equal(TileVisibility.Hidden, world.Fog.Get(1, At(24, 24)));
    }

    [Fact]
    public void Building_UsesItsOwnSightRange()
    {
        var world = CreateFogWorld();
        SpawnBuilding(world, playerId: 1, new GridPosition(20, 20), sightRange: 8);

        Update(world);

        Assert.Equal(TileVisibility.Visible, world.Fog.Get(1, At(20, 28)));
        Assert.Equal(TileVisibility.Visible, world.Fog.Get(1, At(28, 20)));
        Assert.Equal(TileVisibility.Hidden, world.Fog.Get(1, At(0, 0)));
    }

    [Fact]
    public void OnePlayersGrid_DoesNotSeeTheOthers()
    {
        var world = CreateFogWorld();
        SpawnUnit(world, playerId: 1, new GridPosition(20, 20));

        Update(world);

        Assert.Equal(TileVisibility.Visible, world.Fog.Get(1, At(20, 20)));
        Assert.Equal(TileVisibility.Hidden, world.Fog.Get(2, At(20, 20)));
    }

    // ------------------------------------------------------------------
    // moving source
    // ------------------------------------------------------------------

    [Fact]
    public void SourceMoves_OldTileBecomesExplored_NeverHidden()
    {
        var world = CreateFogWorld();
        var unit = SpawnUnit(world, playerId: 1, new GridPosition(10, 10));

        Update(world);
        Assert.Equal(TileVisibility.Visible, world.Fog.Get(1, At(10, 10)));

        unit.Position = new GridPosition(30, 30);
        Update(world);

        Assert.Equal(TileVisibility.Visible, world.Fog.Get(1, At(30, 30)));
        Assert.Equal(TileVisibility.Explored, world.Fog.Get(1, At(10, 10)));
        Assert.Equal(TileVisibility.Hidden, world.Fog.Get(1, At(0, 0)));
    }

    [Fact]
    public void StationarySource_NoOscillationOnLaterTicks()
    {
        var world = CreateFogWorld();
        SpawnUnit(world, playerId: 1, new GridPosition(20, 20));

        for (int i = 0; i < 5; i++)
            Update(world);

        Assert.Equal(TileVisibility.Visible, world.Fog.Get(1, At(20, 20)));
        Assert.Equal(TileVisibility.Visible, world.Fog.Get(1, At(24, 20)));
    }

    // ------------------------------------------------------------------
    // remembered enemy buildings
    // ------------------------------------------------------------------

    [Fact]
    public void Remembers_EnemyBuildingAsSoonAsVisible()
    {
        var world = CreateFogWorld();
        SpawnUnit(world, playerId: 1, new GridPosition(20, 20));
        var enemyBuilding = SpawnBuilding(world, playerId: 2, new GridPosition(22, 20));

        Update(world);

        var viewer = world.GetPlayerById(1)!;
        Assert.Contains(enemyBuilding.Id, viewer.RememberedEnemyBuildingIds);
    }

    [Fact]
    public void DoesNotRemember_NeverSeenBuilding()
    {
        var world = CreateFogWorld();
        SpawnUnit(world, playerId: 1, new GridPosition(20, 20));
        var enemyBuilding = SpawnBuilding(world, playerId: 2, new GridPosition(0, 0));

        Update(world);

        var viewer = world.GetPlayerById(1)!;
        Assert.DoesNotContain(enemyBuilding.Id, viewer.RememberedEnemyBuildingIds);
    }

    [Fact]
    public void Forgets_EnemyBuildingOnDeath()
    {
        var world = CreateFogWorld();
        SpawnUnit(world, playerId: 1, new GridPosition(20, 20));
        var enemyBuilding = SpawnBuilding(world, playerId: 2, new GridPosition(22, 20));

        Update(world);
        var viewer = world.GetPlayerById(1)!;
        Assert.Contains(enemyBuilding.Id, viewer.RememberedEnemyBuildingIds);

        enemyBuilding.Health.CurrentHealth = 0;
        Update(world);

        Assert.DoesNotContain(enemyBuilding.Id, viewer.RememberedEnemyBuildingIds);
    }

    [Fact]
    public void DoesNotForget_LivingBuildingEvenWhenNoLongerVisible()
    {
        var world = CreateFogWorld();
        var unit = SpawnUnit(world, playerId: 1, new GridPosition(20, 20));
        var enemyBuilding = SpawnBuilding(world, playerId: 2, new GridPosition(22, 20));

        Update(world);
        var viewer = world.GetPlayerById(1)!;
        Assert.Contains(enemyBuilding.Id, viewer.RememberedEnemyBuildingIds);

        // the unit moves away: the tile degrades, the building is still alive
        unit.Position = new GridPosition(0, 0);
        Update(world);

        Assert.Equal(TileVisibility.Explored, world.Fog.Get(1, At(22, 20)));
        Assert.Contains(enemyBuilding.Id, viewer.RememberedEnemyBuildingIds);
    }

    // ------------------------------------------------------------------
    // determinism
    // ------------------------------------------------------------------

    [Fact]
    public void TwoIdenticalRuns_IdenticalGrids()
    {
        var worldA = CreateFogWorld();
        var worldB = CreateFogWorld();

        SpawnUnit(worldA, playerId: 1, new GridPosition(10, 10));
        SpawnUnit(worldB, playerId: 1, new GridPosition(10, 10));
        SpawnBuilding(worldA, playerId: 2, new GridPosition(13, 10));
        SpawnBuilding(worldB, playerId: 2, new GridPosition(13, 10));

        for (int i = 0; i < 20; i++)
        {
            Update(worldA);
            Update(worldB);
        }

        for (int y = 0; y < 40; y++)
        {
            for (int x = 0; x < 40; x++)
            {
                Assert.Equal(worldA.Fog.Get(1, At(x, y)), worldB.Fog.Get(1, At(x, y)));
                Assert.Equal(worldA.Fog.Get(2, At(x, y)), worldB.Fog.Get(2, At(x, y)));
            }
        }

        var rememberedA = worldA.GetPlayerById(1)!.RememberedEnemyBuildingIds.OrderBy(id => id);
        var rememberedB = worldB.GetPlayerById(1)!.RememberedEnemyBuildingIds.OrderBy(id => id);
        Assert.Equal(rememberedA, rememberedB);
    }

    // ------------------------------------------------------------------
    // bypass
    // ------------------------------------------------------------------

    [Fact]
    public void VisibilitySystem_InAllVisible_IsANoOp()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers(width: 20, height: 20);
        var context = SimulationTestHelper.CreateContext(world);

        VisibilitySystem.Update(context);

        Assert.False(world.Fog.IsEnabled);
        Assert.Null(world.Fog.GetState(1));
    }
}
