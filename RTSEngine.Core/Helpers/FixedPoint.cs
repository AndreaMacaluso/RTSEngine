using System;
using System.Text.Json.Serialization;

namespace RTSEngine.Core.Helpers;

/// <summary>
/// Fixed-point arithmetic type for deterministic cross-platform calculations.
///
/// Replaces float/double to guarantee identical results across different CPUs,
/// which is essential for lockstep multiplayer and replay determinism.
///
/// Representation: integer value * 1000 (Scale = 1000).
/// Example: 1.5 → Raw = 1500, 0.25 → Raw = 250
///
/// Precision: 3 decimal places (1/1000 = 0.001).
/// Range: ±2,147,483 (int.MaxValue / 1000).
///
/// Usage:
///   FixedPoint speed = FixedPoint.FromFloat(0.25f);
///   FixedPoint one = FixedPoint.One;           // 1.0
///   FixedPoint distance = a + b;
///   bool greater = a > b;
///
/// JSON serialization: handled by FixedPointJsonConverter (stores as float).
/// </summary>
[JsonConverter(typeof(FixedPointJsonConverter))]
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
    public static explicit operator FixedPoint(float value) => FromFloat(value);
}
