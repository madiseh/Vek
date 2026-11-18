// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;

namespace Vek;

internal static class DataGridGroupIntervalEvaluator
{
    internal static object? Evaluate(object? value, DataGridGroupInterval interval, decimal intervalSize)
    {
        if (value is null || interval == DataGridGroupInterval.None)
        {
            return value;
        }

        return interval switch
        {
            DataGridGroupInterval.DateYear => GetDate(value).Year.ToString(CultureInfo.CurrentCulture),
            DataGridGroupInterval.DateMonth => GetDate(value).ToString("MMMM yyyy", CultureInfo.CurrentCulture),
            DataGridGroupInterval.DateQuarter => GetQuarter(value),
            DataGridGroupInterval.Alphabetical => GetAlphabeticalKey(value),
            DataGridGroupInterval.NumericRange => GetNumericRange(value, intervalSize),
            _ => value
        };
    }

    private static DateTime GetDate(object value)
    {
        return value switch
        {
            DateTime dateTime => dateTime,
            DateTimeOffset dateTimeOffset => dateTimeOffset.DateTime,
            DateOnly dateOnly => dateOnly.ToDateTime(TimeOnly.MinValue),
            _ => throw new ArgumentException("Date group intervals require a date value.", nameof(value))
        };
    }

    private static string GetAlphabeticalKey(object value)
    {
        var text = Convert.ToString(value, CultureInfo.CurrentCulture);
        if (string.IsNullOrEmpty(text))
        {
            return "#";
        }

        var firstCharacter = text[0];
        return char.IsLetter(firstCharacter) ? char.ToUpper(firstCharacter, CultureInfo.CurrentCulture).ToString() : "#";
    }

    private static string GetQuarter(object value)
    {
        var date = GetDate(value);
        return $"Q{((date.Month - 1) / 3) + 1}, {date.Year}";
    }

    private static string GetNumericRange(object value, decimal intervalSize)
    {
        if (intervalSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(intervalSize), "The numeric group interval must be greater than zero.");
        }

        var number = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        var start = decimal.Floor(number / intervalSize) * intervalSize;
        var end = start + intervalSize;
        return string.Create(CultureInfo.CurrentCulture, $"{start:G}–{end:G}");
    }
}
