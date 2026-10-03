using System.Collections.Concurrent;

namespace RTSEngine.Core.Map.Visibility;

internal sealed class DiscProjector
{
    internal static readonly DiscProjector Instance = new();

    private readonly ConcurrentDictionary<int, (int Dx, int Dy)[]> _cache = new();

    public IReadOnlyList<(int Dx, int Dy)> Offsets(int range)
    {
        // Deliberate cap: it bounds the cache and the largest disc (129x129).
        // A SightRange above it sees exactly the same; a negative one clamps to
        // 0 and sees only its own tile, instead of becoming the full disc.
        range = Math.Clamp(range, 0, 64);

        if (_cache.TryGetValue(range, out var cached))
            return cached;

        return _cache.GetOrAdd(range, BuildDisc);
    }

    private static (int Dx, int Dy)[] BuildDisc(int range)
    {
        long radiusSquared = (long)range * range;
        var offsets = new List<(int Dx, int Dy)>((2 * range + 1) * (2 * range + 1));

        for (int dy = -range; dy <= range; dy++)
        {
            for (int dx = -range; dx <= range; dx++)
            {
                if ((long)dx * dx + (long)dy * dy <= radiusSquared)
                    offsets.Add((dx, dy));
            }
        }

        return offsets.ToArray();
    }
}
