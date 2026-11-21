// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Globalization;

namespace Vek;

internal static class FilterBuilderValueParser
{
    internal static bool TryParse(string? value, Type propertyType, out object? parsedValue)
    {
        parsedValue = null;
        if (string.IsNullOrEmpty(value))
        {
            return true;
        }

        var targetType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        try
        {
            parsedValue = targetType == typeof(string) ? value
                : targetType == typeof(Guid) ? Guid.Parse(value)
                : targetType == typeof(DateOnly) ? DateOnly.Parse(value, CultureInfo.InvariantCulture)
                : targetType == typeof(DateTime) ? DateTime.Parse(value, CultureInfo.InvariantCulture)
                : targetType == typeof(DateTimeOffset) ? DateTimeOffset.Parse(value, CultureInfo.InvariantCulture)
                : targetType == typeof(bool) ? bool.Parse(value)
                : targetType.IsEnum ? Enum.Parse(targetType, value, ignoreCase: true)
                : Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
        catch (OverflowException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
