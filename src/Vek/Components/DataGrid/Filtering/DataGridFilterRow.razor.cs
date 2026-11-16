// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using Vek.Utilities;

namespace Vek;

/// <summary>
/// Represents the dedicated filter row displayed below column headers in a <see cref="VekDataGrid{T}"/>.
/// </summary>
/// <typeparam name="T">The type of item managed by the data grid.</typeparam>
public partial class DataGridFilterRow<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> : VekComponentBase
{
    /// <summary>
    /// The parent <see cref="VekDataGrid{T}"/> containing this filter row.
    /// </summary>
    [CascadingParameter]
    public VekDataGrid<T> DataGrid { get; set; } = null!;

    private string Classname =>
        new CssBuilder("vek-table-row")
            .AddClass("vek-datagrid-filter-row")
            .AddClass(Class)
            .Build();

    private string Stylename =>
        new StyleBuilder()
            .AddStyle(Style)
            .Build();
}
