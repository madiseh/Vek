// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Vek;

/// <summary>
/// Provides token parsing and row matching logic for searching rows in a <see cref="VekDataGrid{T}"/>.
/// </summary>
public static class DataGridSearchEngine
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo?> AllowSearchPropertyCache = new();

    /// <summary>
    /// Parses a search string into individual search tokens based on the specified parse mode.
    /// </summary>
    /// <param name="searchText">The raw search text entered by the user.</param>
    /// <param name="parseMode">The parse mode determining how the text is split into tokens.</param>
    /// <returns>A read-only list of parsed tokens, or an empty list if no valid tokens exist.</returns>
    public static IReadOnlyList<string> ParseTokens(string? searchText, DataGridSearchTextParseMode parseMode)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return Array.Empty<string>();
        }

        switch (parseMode)
        {
            case DataGridSearchTextParseMode.ExactMatch:
                {
                    var trimmed = searchText.Trim();
                    return trimmed.Length > 0 ? new[] { trimmed } : Array.Empty<string>();
                }

            case DataGridSearchTextParseMode.GroupWordsByAnd:
            case DataGridSearchTextParseMode.GroupWordsByOr:
                {
                    var tokens = new List<string>();
                    var current = new StringBuilder();
                    var inQuotes = false;

                    for (var i = 0; i < searchText.Length; i++)
                    {
                        var c = searchText[i];

                        if (c == '\\' && i + 1 < searchText.Length && (searchText[i + 1] == '"' || searchText[i + 1] == '\\'))
                        {
                            current.Append(searchText[i + 1]);
                            i++;
                            continue;
                        }

                        if (c == '"')
                        {
                            inQuotes = !inQuotes;
                            if (!inQuotes && current.Length > 0)
                            {
                                var token = current.ToString().Trim();
                                if (token.Length > 0)
                                {
                                    tokens.Add(token);
                                }
                                current.Clear();
                            }
                        }
                        else if (char.IsWhiteSpace(c) && !inQuotes)
                        {
                            if (current.Length > 0)
                            {
                                var token = current.ToString().Trim();
                                if (token.Length > 0)
                                {
                                    tokens.Add(token);
                                }
                                current.Clear();
                            }
                        }
                        else
                        {
                            current.Append(c);
                        }
                    }

                    if (current.Length > 0)
                    {
                        var token = current.ToString().Trim();
                        if (token.Length > 0)
                        {
                            tokens.Add(token);
                        }
                    }

                    return tokens;
                }

            default:
                throw new ArgumentOutOfRangeException(nameof(parseMode), parseMode, null);
        }
    }

    /// <summary>
    /// Determines whether the specified data item matches the search tokens across the searchable columns.
    /// </summary>
    /// <typeparam name="T">The type of item.</typeparam>
    /// <param name="item">The data item to evaluate.</param>
    /// <param name="columns">The columns to search within.</param>
    /// <param name="tokens">The search tokens.</param>
    /// <param name="parseMode">The parse mode determining how tokens are evaluated (AND, OR, or Exact).</param>
    /// <param name="culture">The optional culture to use for case-insensitive matching and formatting.</param>
    /// <returns><c>true</c> if the item satisfies the search criteria; otherwise, <c>false</c>.</returns>
    public static bool MatchesRow<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(
        T item,
        IEnumerable<Column<T>> columns,
        IReadOnlyList<string> tokens,
        DataGridSearchTextParseMode parseMode,
        CultureInfo? culture = null)
    {
        return MatchesRow(item, columns, tokens, parseMode, culture, isColumnSearchable: null);
    }

    /// <summary>
    /// Determines whether the specified data item matches the search tokens across the searchable columns.
    /// </summary>
    /// <typeparam name="T">The type of item.</typeparam>
    /// <param name="item">The data item to evaluate.</param>
    /// <param name="columns">The columns to search within.</param>
    /// <param name="tokens">The search tokens.</param>
    /// <param name="parseMode">The parse mode determining how tokens are evaluated (AND, OR, or Exact).</param>
    /// <param name="culture">The optional culture to use for case-insensitive matching and formatting.</param>
    /// <param name="isColumnSearchable">An optional delegate to determine whether a column is searchable. If <c>null</c>, reflection is used to check <c>AllowSearch</c>.</param>
    /// <returns><c>true</c> if the item satisfies the search criteria; otherwise, <c>false</c>.</returns>
    public static bool MatchesRow<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(
        T item,
        IEnumerable<Column<T>> columns,
        IReadOnlyList<string> tokens,
        DataGridSearchTextParseMode parseMode,
        CultureInfo? culture,
        Func<Column<T>, bool>? isColumnSearchable)
    {
        if (item is null || columns is null)
        {
            return false;
        }

        var validTokens = tokens?.Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
        if (validTokens is null || validTokens.Count == 0)
        {
            return true;
        }

        var searchableColumns = columns.Where(c => c is not null && IsColumnSearchable(c, isColumnSearchable)).ToList();
        if (searchableColumns.Count == 0)
        {
            return false;
        }

        var cellValues = new List<string>();
        foreach (var column in searchableColumns)
        {
            foreach (var str in GetCellStrings(column, item, culture))
            {
                cellValues.Add(str);
            }
        }

        if (cellValues.Count == 0)
        {
            return false;
        }

        return parseMode switch
        {
            DataGridSearchTextParseMode.GroupWordsByOr =>
                validTokens.Any(token => cellValues.Any(val => ContainsToken(val, token, culture))),
            DataGridSearchTextParseMode.GroupWordsByAnd or DataGridSearchTextParseMode.ExactMatch =>
                validTokens.All(token => cellValues.Any(val => ContainsToken(val, token, culture))),
            _ => throw new ArgumentOutOfRangeException(nameof(parseMode), parseMode, null)
        };
    }

    private static bool IsColumnSearchable<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(
        Column<T> column,
        Func<Column<T>, bool>? predicate)
    {
        if (predicate is not null)
        {
            return predicate(column);
        }

        var prop = AllowSearchPropertyCache.GetOrAdd(column.GetType(), GetAllowSearchProperty);

        if (prop is not null && prop.CanRead)
        {
            var value = prop.GetValue(column);
            if (value is bool b)
            {
                return b;
            }
        }

        return true;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "Reflection check for optional AllowSearch property on Column instances.")]
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Reflection check for optional AllowSearch property on Column instances.")]
    private static PropertyInfo? GetAllowSearchProperty(Type type)
    {
        return type.GetProperty("AllowSearch", BindingFlags.Public | BindingFlags.Instance);
    }

    private static IEnumerable<string> GetCellStrings<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(
        Column<T> column,
        T item,
        CultureInfo? culture)
    {
        object? value;
        try
        {
            value = column.GetCellContent(item);
        }
        catch
        {
            try
            {
                value = column.PropertyFunc(item);
            }
            catch
            {
                value = null;
            }
        }

        if (value is null)
        {
            yield break;
        }

        if (value is string s)
        {
            if (!string.IsNullOrEmpty(s))
            {
                yield return s;
            }
            yield break;
        }

        var effectiveCulture = culture ?? column.Culture ?? CultureInfo.CurrentCulture;
        string? formatted = null;

        if (value is IFormattable formattable && !string.IsNullOrEmpty(column.ContentFormat))
        {
            try
            {
                formatted = formattable.ToString(column.ContentFormat, effectiveCulture);
            }
            catch
            {
                // Fallback to unformatted value
            }
        }

        if (!string.IsNullOrEmpty(formatted))
        {
            yield return formatted;
        }

        string? raw = null;
        try
        {
            raw = value is IFormattable f
                ? f.ToString(null, effectiveCulture)
                : Convert.ToString(value, effectiveCulture) ?? value.ToString();
        }
        catch
        {
            // Ignore conversion error
        }

        if (!string.IsNullOrEmpty(raw) && !string.Equals(raw, formatted, StringComparison.Ordinal))
        {
            yield return raw;
        }
    }

    private static bool ContainsToken(string cellValue, string token, CultureInfo? culture)
    {
        if (culture is not null)
        {
            return culture.CompareInfo.IndexOf(cellValue, token, CompareOptions.IgnoreCase) >= 0;
        }

        return cellValue.Contains(token, StringComparison.OrdinalIgnoreCase) ||
               cellValue.Contains(token, StringComparison.CurrentCultureIgnoreCase);
    }
}
