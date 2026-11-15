// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;
using Vek.Extensions;

namespace Vek;

/// <summary>
/// Context passed to a <see cref="VekDataGrid{T}"/> search box template, exposing search state and operations.
/// </summary>
/// <typeparam name="T">The kind of item managed by the data grid.</typeparam>
public class DataGridSearchBoxContext<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>
{
    /// <summary>
    /// Gets the data grid instance associated with this search box context.
    /// </summary>
    public VekDataGrid<T> DataGrid { get; }

    /// <summary>
    /// Gets the current search text of the data grid.
    /// </summary>
    public string? SearchText => DataGrid.GetState(x => x.SearchText);

    /// <summary>
    /// Updates the search text of the data grid asynchronously.
    /// </summary>
    /// <param name="text">The new search text to apply.</param>
    public Task UpdateSearchTextAsync(string? text) => DataGrid.SetSearchTextAsync(text);

    /// <summary>
    /// Clears the search text of the data grid asynchronously.
    /// </summary>
    public Task ClearAsync() => DataGrid.SetSearchTextAsync(string.Empty);

    /// <summary>
    /// Initializes a new instance of the <see cref="DataGridSearchBoxContext{T}"/> class.
    /// </summary>
    /// <param name="dataGrid">The data grid instance.</param>
    public DataGridSearchBoxContext(VekDataGrid<T> dataGrid) => DataGrid = dataGrid;
}
