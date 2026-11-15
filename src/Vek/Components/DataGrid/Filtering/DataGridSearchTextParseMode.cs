// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Vek;

/// <summary>
/// Specifies how search text is parsed into tokens and evaluated when searching rows in a <see cref="VekDataGrid{T}"/>.
/// </summary>
public enum DataGridSearchTextParseMode
{
    /// <summary>
    /// Search text is split by whitespace into tokens, and a row matches when all tokens are found in the row's searchable columns.
    /// </summary>
    GroupWordsByAnd,

    /// <summary>
    /// Search text is split by whitespace into tokens, and a row matches when at least one token is found in the row's searchable columns.
    /// </summary>
    GroupWordsByOr,

    /// <summary>
    /// Search text is treated as a single token without splitting by whitespace, and a row matches when the exact phrase is found in the row's searchable columns.
    /// </summary>
    ExactMatch,
}
