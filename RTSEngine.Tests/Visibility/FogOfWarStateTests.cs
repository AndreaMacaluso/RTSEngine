using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Map.Visibility;
using RTSEngine.Core.Settings;
using RTSEngine.Core.Systems;
using RTSEngine.Tests.TestHelpers;
using Xunit;

namespace RTSEngine.Tests.Visibility;

[Trait("Category", "Visibility")]
public class FogOfWarStateTests
{
    private static FogOfWarState NewState(int size = 40) => new(size, size);

    private static void Tick(FogOfWarState state, params (int X, int Y)[] sources)
    {
        state.BeginProjection();
        foreach (var (x, y) in sources)
            state.Project(new GridPosition(x, y));
        state.DegradeStale();
        state.EndProjection();
    }

    private static GridPosition At(int x, int y) => new(x, y);

    // ------------------------------------------------------------------
    // state transitions
    // ------------------------------------------------------------------

    [Fact]
    public void NewState_IsAllHidden()
    {
        var state = NewState();

        for (int x = 0; x < 40; x++)
            for (int y = 0; y < 40; y++)
                Assert.Equal(TileVisibility.Hidden, state.Get(At(x, y)));

        Assert.Empty(state.VisibleIndexes);
    }

    [Fact]
    public void SourceCovers_TurnsVisible()
    {
        var state = NewState();

        Tick(state, (20, 20));

        Assert.Equal(TileVisibility.Visible, state.Get(At(20, 20)));
        Assert.Single(state.VisibleIndexes);
    }

    [Fact]
    public void SourceMovesAway_DegradesToExplored_NeverHidden()
    {
        var state = NewState();

        Tick(state, (20, 20));
        Tick(state);   // source off / out of range

        Assert.Equal(TileVisibility.Explored, state.Get(At(20, 20)));
    }

    [Fact]
    public void Decay_OnlyHitsTilesNotReconfirmed()
    {
        var state = NewState();

        Tick(state, (5, 5), (6, 6));
        Tick(state, (5, 5));

        Assert.Equal(TileVisibility.Visible, state.Get(At(5, 5)));
        Assert.Equal(TileVisibility.Explored, state.Get(At(6, 6)));
        Assert.Single(state.VisibleIndexes);
    }

    [Fact]
    public void StationarySource_NoOscillation()
    {
        var state = NewState();

        for (int tick = 0; tick < 5; tick++)
            Tick(state, (20, 20));

        Assert.Equal(TileVisibility.Visible, state.Get(At(20, 20)));
        Assert.Single(state.VisibleIndexes);
    }

    [Fact]
    public void SourceMoves_OldTilesExplored_NewTilesVisible()
    {
        var state = NewState();

        Tick(state, (10, 10));
        Tick(state, (14, 10));

        Assert.Equal(TileVisibility.Explored, state.Get(At(10, 10)));
        Assert.Equal(TileVisibility.Visible, state.Get(At(14, 10)));
    }

    // ------------------------------------------------------------------
    // bounds
    // ------------------------------------------------------------------

    [Fact]
    public void OutOfBounds_GetReturnsHidden()
    {
        var state = NewState();

        Assert.Equal(TileVisibility.Hidden, state.Get(At(-1, 0)));
        Assert.Equal(TileVisibility.Hidden, state.Get(At(0, -1)));
        Assert.Equal(TileVisibility.Hidden, state.Get(At(40, 0)));
        Assert.Equal(TileVisibility.Hidden, state.Get(At(0, 40)));
    }

    [Fact]
    public void OutOfBounds_ProjectDoesNotThrowOrCorruptTheIndex()
    {
        var state = NewState();

        state.BeginProjection();
        state.Project(At(-1, 0));
        state.Project(At(0, -1));
        state.Project(At(40, 40));
        state.Project(At(int.MaxValue, int.MaxValue));
        state.Project(At(int.MinValue, int.MinValue));
        state.DegradeStale();
        state.EndProjection();

        Assert.Empty(state.VisibleIndexes);
        Assert.Equal(TileVisibility.Hidden, state.Get(At(0, 0)));
    }

    // ------------------------------------------------------------------
    // shape of the disc
    // ------------------------------------------------------------------

    [Fact]
    public void VisionIsRound_BoundingSquareCornersStayHidden()
    {
        var state = new FogOfWarState(41, 41);
        const int range = 4;
        const int center = 20;

        state.BeginProjection();
        foreach (var (dx, dy) in DiscProjector.Instance.Offsets(range))
            state.Project(At(center + dx, center + dy));
        state.DegradeStale();
        state.EndProjection();

        // on the axes, Chebyshev distance 4 is inside the disc
        Assert.Equal(TileVisibility.Visible, state.Get(At(center, center)));
        Assert.Equal(TileVisibility.Visible, state.Get(At(center + 4, center)));
        Assert.Equal(TileVisibility.Visible, state.Get(At(center, center + 4)));

        // square corners: euclidean 4*sqrt(2) ~ 5.66 > 4, outside
        Assert.Equal(TileVisibility.Hidden, state.Get(At(center + 4, center + 4)));
        Assert.Equal(TileVisibility.Hidden, state.Get(At(center - 4, center + 4)));
        Assert.Equal(TileVisibility.Hidden, state.Get(At(center + 4, center - 4)));
        Assert.Equal(TileVisibility.Hidden, state.Get(At(center - 4, center - 4)));
    }

    [Fact]
    public void Disc_Radius4_Has49Tiles_TheSquareHad81()
    {
        var state = new FogOfWarState(41, 41);

        state.BeginProjection();
        foreach (var (dx, dy) in DiscProjector.Instance.Offsets(4))
            state.Project(At(20 + dx, 20 + dy));
        state.DegradeStale();
        state.EndProjection();

        Assert.Equal(49, state.VisibleIndexes.Count);
    }

    [Fact]
    public void Disc_Radius5_Has81Tiles_TheSquareHad121()
    {
        var state = new FogOfWarState(41, 41);

        state.BeginProjection();
        foreach (var (dx, dy) in DiscProjector.Instance.Offsets(5))
            state.Project(At(20 + dx, 20 + dy));
        state.DegradeStale();
        state.EndProjection();

        Assert.Equal(81, state.VisibleIndexes.Count);
    }

    // ------------------------------------------------------------------
    // persistence
    // ------------------------------------------------------------------

    [Fact]
    public void RebuildVisibleIndex_RebuildsTheIndexFromTheBuffer()
    {
        var state = NewState(10);
        Tick(state, (3, 3), (7, 7));

        var rebuilt = new FogOfWarState(10, 10);
        for (int y = 0; y < 10; y++)
            for (int x = 0; x < 10; x++)
                rebuilt.Set(At(x, y), state.Get(At(x, y)));
        rebuilt.RebuildVisibleIndex();

        Assert.Equal(2, rebuilt.VisibleIndexes.Count);
        Assert.Equal(TileVisibility.Visible, rebuilt.Get(At(3, 3)));
        Assert.Equal(TileVisibility.Visible, rebuilt.Get(At(7, 7)));

        // and the index has to continue correctly from there
        rebuilt.BeginProjection();
        rebuilt.Project(At(3, 3));
        rebuilt.DegradeStale();
        rebuilt.EndProjection();

        Assert.Equal(TileVisibility.Visible, rebuilt.Get(At(3, 3)));
        Assert.Equal(TileVisibility.Explored, rebuilt.Get(At(7, 7)));
    }
}
