// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Vek;

#nullable enable

/// <summary>
/// Provides full-text search over a collection of items.
/// </summary>
internal interface ISearchService
{
    /// <summary>
    /// Searches a collection of items by iterating each item's keyword list,
    /// returning matching items ordered by relevance.
    /// Multiple keywords per item are supported; the highest score per item wins.
    /// </summary>
    IReadOnlyList<T> Search<T>(IEnumerable<T> items, Func<T, IEnumerable<string>> getKeywords, string query) where T : notnull;
}
