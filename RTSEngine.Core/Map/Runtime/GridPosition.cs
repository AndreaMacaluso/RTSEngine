namespace RTSEngine.Core.Map.Runtime;

public struct GridPosition : IEquatable<GridPosition>
{
    public int X { get; set; }

    public int Y { get; set; }

    public GridPosition(int x, int y)
    {
        X = x;
        Y = y;
    }

    public bool Equals(GridPosition other)
    {
        return X == other.X
            && Y == other.Y;
    }

    public override bool Equals(object? obj)
    {
        return obj is GridPosition other
            && Equals(other);
    }

    // Deterministic hash: HashCode.Combine uses a randomized seed per process,
    // which breaks cross-run determinism needed for multiplayer.
    // This manual hash guarantees same input → same output, always.
    public override int GetHashCode()
    {
        return (X * 397) ^ Y;
    }

    public static bool operator ==(
        GridPosition left,
        GridPosition right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(
        GridPosition left,
        GridPosition right)
    {
        return !left.Equals(right);
    }

    public override string ToString()
    {
        return $"({X}, {Y})";
    }
    
}