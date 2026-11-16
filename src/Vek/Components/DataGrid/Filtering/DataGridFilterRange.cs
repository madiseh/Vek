// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Vek;

/// <summary>
/// Represents a numeric, date, or comparable range for range-based filtering operations such as <see cref="DataGridFilterOperator.Between"/>.
/// </summary>
public sealed class DataGridFilterRange
{
    /// <summary>
    /// The inclusive lower bound of the filter range.
    /// </summary>
    public object? From { get; set; }

    /// <summary>
    /// The inclusive upper bound of the filter range.
    /// </summary>
    public object? To { get; set; }

    public DataGridFilterRange()
    {
    }

    public DataGridFilterRange(object? from, object? to)
    {
        From = from;
        To = to;
    }

    /// <summary>
    /// Indicates whether neither bound is specified.
    /// </summary>
    public bool IsEmpty => From is null && To is null;

    /// <inheritdoc />
    public override string ToString()
    {
        if (From is not null && To is not null)
        {
            return $"{From}..{To}";
        }

        if (From is not null)
        {
            return $">={From}";
        }

        if (To is not null)
        {
            return $"<={To}";
        }

        return string.Empty;
    }
}
