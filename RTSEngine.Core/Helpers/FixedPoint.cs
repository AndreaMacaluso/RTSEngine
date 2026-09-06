using System;

namespace RTSEngine.Core.Helpers;

public readonly struct FixedPoint : IEquatable<FixedPoint>, IComparable<FixedPoint>
{
    private const int Scale = 1000;
    public int Raw { get; }

    public FixedPoint(int raw) => Raw = raw;

    public static FixedPoint FromFloat(float value) => new((int)(value * Scale));

    public static FixedPoint FromGrid(int coordinate) => new(coordinate * Scale + Scale / 2);

    public float ToFloat() => (float)Raw / Scale;

    public static FixedPoint operator +(FixedPoint a, FixedPoint b) => new(a.Raw + b.Raw);

    public static FixedPoint operator -(FixedPoint a, FixedPoint b) => new(a.Raw - b.Raw);

    public static FixedPoint operator *(FixedPoint a, FixedPoint b) => new(a.Raw * b.Raw / Scale);

    public static FixedPoint operator /(FixedPoint a, FixedPoint b)
    {
        if (b.Raw == 0) throw new DivideByZeroException("FixedPoint division by zero");
        return new FixedPoint(a.Raw * Scale / b.Raw);
    }

    public static FixedPoint operator -(FixedPoint a) => new(-a.Raw);

    public static bool operator >(FixedPoint a, FixedPoint b) => a.Raw > b.Raw;
    public static bool operator >=(FixedPoint a, FixedPoint b) => a.Raw >= b.Raw;
    public static bool operator <(FixedPoint a, FixedPoint b) => a.Raw < b.Raw;
    public static bool operator <=(FixedPoint a, FixedPoint b) => a.Raw <= b.Raw;

    public static bool operator ==(FixedPoint a, FixedPoint b) => a.Raw == b.Raw;
    public static bool operator !=(FixedPoint a, FixedPoint b) => a.Raw != b.Raw;

    public static int DistanceSquared(FixedPoint x1, FixedPoint y1, FixedPoint x2, FixedPoint y2)
    {
        int dx = x1.Raw - x2.Raw;
        int dy = y1.Raw - y2.Raw;
        return dx * dx + dy * dy;
    }

    public static int IntegerSqrt(int n)
    {
        if (n <= 0) return 0;
        int x = n;
        int y = (x + 1) / 2;
        while (y < x)
        {
            x = y;
            y = (x + n / x) / 2;
        }
        return x;
    }

    public bool Equals(FixedPoint other) => Raw == other.Raw;
    public override bool Equals(object? obj) => obj is FixedPoint other && Equals(other);
    public override int GetHashCode() => Raw;
    public int CompareTo(FixedPoint other) => Raw.CompareTo(other.Raw);
    public override string ToString() => ToFloat().ToString("F3");

    public static readonly FixedPoint Zero = new(0);
    public static readonly FixedPoint One = new(Scale);
    public static readonly FixedPoint Half = new(Scale / 2);

    public static implicit operator FixedPoint(int value) => new(value * Scale);
    public static implicit operator FixedPoint(float value) => FromFloat(value);
}
