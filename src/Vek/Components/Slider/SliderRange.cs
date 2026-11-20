using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Vek;

/// <summary>
/// Represents a numeric range with start and end values for <see cref="VekRangeSlider{T}"/>.
/// </summary>
/// <typeparam name="T">The numeric value type.</typeparam>
public class SliderRange<T> : IEquatable<SliderRange<T>?> where T : struct, INumber<T>
{
    /// <summary>
    /// The lower or starting value of the range.
    /// </summary>
    public T Start { get; init; }

    /// <summary>
    /// The upper or ending value of the range.
    /// </summary>
    public T End { get; init; }

    /// <summary>
    /// The lower value of the range.
    /// </summary>
    /// <remarks>
    /// Alias for <see cref="Start"/>.
    /// </remarks>
    public T Lower => Start;

    /// <summary>
    /// The upper value of the range.
    /// </summary>
    /// <remarks>
    /// Alias for <see cref="End"/>.
    /// </remarks>
    public T Upper => End;

    /// <summary>
    /// Initializes a new instance with default values.
    /// </summary>
    public SliderRange() : this(T.Zero, T.Zero)
    {
    }

    /// <summary>
    /// Initializes a new instance with the specified start and end values.
    /// </summary>
    /// <param name="start">The start value.</param>
    /// <param name="end">The end value.</param>
    public SliderRange(T start, T end)
    {
        Start = start;
        End = end;
    }

    /// <summary>
    /// Deconstructs the range into start and end values.
    /// </summary>
    /// <param name="start">The start value.</param>
    /// <param name="end">The end value.</param>
    public void Deconstruct(out T start, out T end)
    {
        start = Start;
        end = End;
    }

    /// <inheritdoc />
    public bool Equals(SliderRange<T>? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Start.Equals(other.Start) && End.Equals(other.End);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is SliderRange<T> other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Start, End);

    /// <inheritdoc />
    public override string ToString() => $"[{Start}, {End}]";

    /// <summary>
    /// Converts a <see cref="SliderRange{T}"/> to a tuple of values.
    /// </summary>
    /// <param name="range">The slider range.</param>
    public static implicit operator (T Start, T End)(SliderRange<T> range) => (range.Start, range.End);

    /// <summary>
    /// Converts a tuple of values to a <see cref="SliderRange{T}"/>.
    /// </summary>
    /// <param name="tuple">The tuple of start and end values.</param>
    public static implicit operator SliderRange<T>((T Start, T End) tuple) => new(tuple.Start, tuple.End);
}
