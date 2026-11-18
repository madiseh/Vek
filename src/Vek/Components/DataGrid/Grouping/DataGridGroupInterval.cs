// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Vek;

/// <summary>
/// Defines built-in ways to group values into intervals.
/// </summary>
public enum DataGridGroupInterval
{
    /// <summary>
    /// Groups values by their original value.
    /// </summary>
    None,

    /// <summary>
    /// Groups dates by year.
    /// </summary>
    DateYear,

    /// <summary>
    /// Groups dates by month and year.
    /// </summary>
    DateMonth,

    /// <summary>
    /// Groups dates by quarter and year.
    /// </summary>
    DateQuarter,

    /// <summary>
    /// Groups text values by their first letter.
    /// </summary>
    Alphabetical,

    /// <summary>
    /// Groups numeric values into equal sized ranges.
    /// </summary>
    NumericRange
}

/// <summary>
/// Calculates a group key from a column value.
/// </summary>
/// <param name="value">The original column value.</param>
/// <returns>The key used to group matching rows.</returns>
public delegate object? GroupIntervalCalculator(object? value);
