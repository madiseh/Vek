// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Components;
using Vek.Docs.Models;

namespace Vek.Docs.Components;

#nullable enable

/// <summary>
/// Displays the see-also links for a component or type.
/// </summary>
public partial class ApiSeeAlsoLinks
{
    private DocumentedType? _type;

    /// <summary>
    /// The type to display members for.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public DocumentedType? Type
    {
        get => _type;
        set
        {
            _type = value;
            Table?.ReloadServerData();
        }
    }

    /// <summary>
    /// The table of see-also links.
    /// </summary>
    public VekTable<DocumentedLink>? Table { get; set; }

    /// <summary>
    /// Requests data for the table.
    /// </summary>
    /// <param name="state">The current table state.</param>
    /// <param name="token">A <see cref="CancellationToken"/> for aborting ongoing requests.</param>
    /// <returns></returns>
    public Task<TableData<DocumentedLink>> GetData(TableState state, CancellationToken token)
    {
        if (_type == null)
        {
            return Task.FromResult<TableData<DocumentedLink>>(new() { Items = [] });
        }

        return Task.FromResult(new TableData<DocumentedLink>()
        {
            Items = _type.Links,
            TotalItems = _type.Links.Count,
        });
    }
}
