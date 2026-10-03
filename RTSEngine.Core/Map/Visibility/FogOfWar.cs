using RTSEngine.Core.Map.Runtime;
using RTSEngine.Core.Settings;

namespace RTSEngine.Core.Map.Visibility;

/// <summary>
/// One FogOfWarState per player, created when the player joins and
/// filled from mode. MapVisibility.AllVisible allocates none.
/// </summary>
public sealed class FogOfWar
{
    private readonly int _width;
    private readonly int _height;
    private readonly Dictionary<int, FogOfWarState> _states = [];

    public MapVisibility Mode { get; }

    /// false = MapVisibility.AllVisible: no grid is allocated and
    /// every tile reads as TileVisibility.Visible.
    public bool IsEnabled { get; }

    private readonly TileVisibility _initialState;

    internal FogOfWar(int width, int height, MapVisibility mode)
    {
        _width = width;
        _height = height;
        Mode = mode;
        IsEnabled = mode != MapVisibility.AllVisible;
        _initialState = mode == MapVisibility.Explored
            ? TileVisibility.Explored
            : TileVisibility.Hidden;
    }

    internal void OnPlayerAdded(int playerId)
    {
        if (!IsEnabled)
            return;

        _states[playerId] = new FogOfWarState(_width, _height, _initialState);
    }

    internal FogOfWarState? GetState(int playerId)
        => _states.TryGetValue(playerId, out var state) ? state : null;

    /// Order by playerId before reading: dictionary order is not stable.
    internal IReadOnlyDictionary<int, FogOfWarState> States => _states;

    public TileVisibility Get(int playerId, GridPosition position)
    {
        if (!IsEnabled)
            return TileVisibility.Visible;

        return GetState(playerId)?.Get(position) ?? TileVisibility.Hidden;
    }

    public bool IsVisible(int playerId, GridPosition position)
        => Get(playerId, position) == TileVisibility.Visible;

    public bool IsSeen(int playerId, GridPosition position)
        => Get(playerId, position) != TileVisibility.Hidden;

    public VisionScope ScopeFor(int playerId) => new(playerId);

    /// <summary>
    /// Marks an area TileVisibility.Explored with no vision source behind
    /// it - a reveal trigger. Nothing calls it yet.
    ///
    /// Tiles already TileVisibility.Visible are skipped: writing them here
    /// would desync the incremental visible index.
    /// </summary>
    public void Reveal(int playerId, GridPosition center, int range)
    {
        if (!IsEnabled)
            return;

        var state = GetState(playerId);
        if (state == null)
            return;

        foreach (var (dx, dy) in DiscProjector.Instance.Offsets(range))
        {
            var tile = new GridPosition(center.X + dx, center.Y + dy);

            if (state.Get(tile) != TileVisibility.Hidden)
                continue;

            state.Set(tile, TileVisibility.Explored);
        }
    }
}
