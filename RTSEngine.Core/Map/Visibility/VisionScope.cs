namespace RTSEngine.Core.Map.Visibility;

/// Who counts as friendly to the viewer - today only the viewer themselves.
/// Not a single choke point: PlayerView compares OwnerId directly.
public readonly struct VisionScope
{
    public int ViewerId { get; }

    public VisionScope(int viewerId)
    {
        ViewerId = viewerId;
    }

    public bool Includes(int ownerId) => ownerId == ViewerId;
}
