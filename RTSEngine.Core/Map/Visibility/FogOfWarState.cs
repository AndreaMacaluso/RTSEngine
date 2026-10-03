using RTSEngine.Core.Map.Runtime;

namespace RTSEngine.Core.Map.Visibility;

/// <summary>
/// Fog state of one player. The update is incremental: only the tiles that were
/// or are visible get touched, never the whole grid every tick.
/// </summary>
internal sealed class FogOfWarState
{
    private const byte ExploredByte = (byte)TileVisibility.Explored;
    private const byte VisibleByte = (byte)TileVisibility.Visible;

    private readonly byte[] _tiles;

    private List<int> _visible;

    private List<int> _projected;

    /// O(1) dedup for _projected; cleared in O(|_projected|), never O(W*H).
    private readonly bool[] _inProjected;

    internal int Width { get; }
    internal int Height { get; }

    internal FogOfWarState(int width, int height, TileVisibility initialState = TileVisibility.Hidden)
    {
        Width = width;
        Height = height;

        _tiles = new byte[width * height];
        if (initialState != TileVisibility.Hidden)
            Array.Fill(_tiles, (byte)initialState);

        _visible = new List<int>();
        _projected = new List<int>();
        _inProjected = new bool[width * height];
    }

    internal TileVisibility Get(GridPosition position)
    {
        if (!InBounds(position))
            return TileVisibility.Hidden;

        return (TileVisibility)_tiles[position.Y * Width + position.X];
    }

    /// Writes _tiles only: _visible is left stale. Call RebuildVisibleIndex
    /// after, unless the tile was Hidden.
    internal void Set(GridPosition position, TileVisibility state)
    {
        if (!InBounds(position))
            return;

        _tiles[position.Y * Width + position.X] = (byte)state;
    }

    internal void BeginProjection() => _projected.Clear();

    internal void Project(GridPosition position)
    {
        if (!InBounds(position))
            return;

        int index = position.Y * Width + position.X;
        if (_inProjected[index])
            return;

        _inProjected[index] = true;
        _projected.Add(index);
        _tiles[index] = VisibleByte;
    }

    /// O(|previous|), not O(W*H).
    internal void DegradeStale()
    {
        foreach (int index in _visible)
        {
            if (!_inProjected[index])
                _tiles[index] = ExploredByte;
        }
    }

    /// Swaps the two lists: _visible ends up holding the new list and
    /// _projected becomes the empty buffer for the next tick.
    internal void EndProjection()
    {
        foreach (int index in _projected)
            _inProjected[index] = false;

        (_visible, _projected) = (_projected, _visible);
    }

    internal IReadOnlyList<int> VisibleIndexes => _visible;

    /// O(W*H), but load/resync only — never in the game loop.
    internal void RebuildVisibleIndex()
    {
        _visible.Clear();
        _projected.Clear();
        Array.Clear(_inProjected);

        for (int i = 0; i < _tiles.Length; i++)
        {
            if (_tiles[i] == VisibleByte)
                _visible.Add(i);
        }
    }

    internal int[] Flatten()
    {
        var result = new int[_tiles.Length];
        for (int i = 0; i < _tiles.Length; i++)
            result[i] = _tiles[i];
        return result;
    }

    private bool InBounds(GridPosition position)
        => (uint)position.X < (uint)Width && (uint)position.Y < (uint)Height;
}
