namespace RTSEngine.Core.Map.Visibility;

/// <summary>
/// What a tile is to a given player: Hidden never seen,
/// Explored seen at least once but not in vision right now,
/// Visible inside a vision source this tick.
///
/// Hidden = 0 is load-bearing: a fresh FogOfWarState is a
/// zeroed byte[] and reads zero as "never seen".
/// </summary>
public enum TileVisibility : byte
{
    Hidden = 0,
    Explored = 1,
    Visible = 2
}
