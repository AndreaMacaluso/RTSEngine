using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Entities.Runtime;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Helpers;
using RTSEngine.Core.Map;
using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Map.Visibility;
using RTSEngine.Core.Settings;
using RTSEngine.Core.State;
using RTSEngine.Core.Systems;
using RTSEngine.Tests.TestHelpers;
using Xunit;

using static RTSEngine.Tests.TestHelpers.FogTestFactory;

namespace RTSEngine.Tests.Visibility;

/// <summary>
/// The third mode: the map arrives already explored, terrain is readable from
/// turn one, and enemy units stay behind the fog anyway.
/// </summary>
[Trait("Category", "Visibility")]
public class ExploredModeTests
{
    [Fact]
    public void Explored_AllocatesTheGrid_WithoutMakingEverythingVisible()
    {
        var world = CreateExploredWorld();

        Assert.Equal(MapVisibility.Explored, world.Fog.Mode);
        Assert.True(world.Fog.IsEnabled);
    }

    [Fact]
    public void WholeMapStartsExplored_NeverHiddenNeverVisible()
    {
        var world = CreateExploredWorld();

        for (int y = 0; y < world.Map.Height; y++)
        {
            for (int x = 0; x < world.Map.Width; x++)
            {
                var position = new GridPosition(x, y);

                // Assert.Fail builds the message only when the tile is wrong,
                // so the loop does not interpolate a string per iteration.
                if (!world.Fog.IsSeen(1, position))
                    Assert.Fail($"tile {x},{y} should be seen as Explored");
                if (world.Fog.IsVisible(1, position))
                    Assert.Fail($"tile {x},{y} should not be visible without a source");
            }
        }
    }

    [Fact]
    public void AllVisible_WithoutFog_MakesEverythingVisible()
    {
        var world = TestWorldFactory.CreateWorldWithTwoPlayers(
            width: 40, height: 40, visibility: MapVisibility.AllVisible);

        Assert.False(world.Fog.IsEnabled);

        var position = new GridPosition(0, 0);
        Assert.True(world.Fog.IsVisible(1, position));
        Assert.True(world.Fog.IsSeen(1, position));
    }

    [Fact]
    public void TheSourceRevealsOnlyItsOwnSurroundings()
    {
        var world = CreateExploredWorld();
        SpawnUnit(world, playerId: 1, new GridPosition(20, 20));

        Update(world);

        Assert.True(world.Fog.IsVisible(1, new GridPosition(20, 20)));
        Assert.True(world.Fog.IsVisible(1, new GridPosition(24, 20)));

        // far from every vision source: explored, therefore usable
        Assert.False(world.Fog.IsVisible(1, new GridPosition(0, 0)));
        Assert.True(world.Fog.IsSeen(1, new GridPosition(0, 0)));
    }

    [Fact]
    public void EnemyHiddenEvenWhenTheMapWasAlwaysExplored()
    {
        var world = CreateExploredWorld();
        SpawnUnit(world, playerId: 1, new GridPosition(20, 20));
        var enemy = SpawnUnit(world, playerId: 2, new GridPosition(0, 0));

        Update(world);

        var scope = world.Fog.ScopeFor(1);
        Assert.False(FogQueries.IsVisible(world, scope, enemy.Position));
        Assert.DoesNotContain(
            FogQueries.GetVisibleEnemies(world, scope),
            u => u.Id == enemy.Id);
    }
}
