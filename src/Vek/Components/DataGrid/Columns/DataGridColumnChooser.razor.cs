// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using Vek.Extensions;
using Vek.Interfaces;
using Vek.State;
using Vek.Utilities;

namespace Vek;

/// <summary>
/// A column chooser and visibility customization dialog for <see cref="VekDataGrid{T}"/>.
/// </summary>
/// <typeparam name="T">The type of item managed by the data grid.</typeparam>
public partial class DataGridColumnChooser<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> : VekComponentBase
{
    private readonly ParameterState<string> _titleState;
    private readonly ParameterState<bool> _allowFilteringState;
    private readonly ParameterState<bool> _allowReorderState;
    private readonly ParameterState<string?> _searchPlaceholderState;
    private readonly ParameterState<bool> _showActionsState;

    private string _searchText = string.Empty;

    protected string Classname => new CssBuilder("vek-column-chooser")
        .AddClass(Class)
        .Build();

    internal VekDataGrid<T>? ResolvedDataGrid => Context?.DataGrid ?? DataGrid ?? CascadedDataGrid;

    [CascadingParameter]
    private VekDataGrid<T>? CascadedDataGrid { get; set; }

    /// <summary>
    /// The target data grid whose columns are customized.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.DataGrid.Behavior)]
    public VekDataGrid<T>? DataGrid { get; set; }

    /// <summary>
    /// The column chooser context providing data grid and column metadata.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.DataGrid.Behavior)]
    public DataGridColumnChooserContext<T>? Context { get; set; }

    /// <summary>
    /// The title displayed in the column chooser header.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>"Columns"</c>.
    /// </remarks>
    [Parameter, ParameterState]
    [Category(CategoryTypes.DataGrid.Appearance)]
    public string Title { get; set; } = "Columns";

    /// <summary>
    /// Specifies whether the search filter input is displayed inside the chooser.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>true</c>.
    /// </remarks>
    [Parameter, ParameterState]
    [Category(CategoryTypes.DataGrid.Behavior)]
    public bool AllowFiltering { get; set; } = true;

    /// <summary>
    /// Specifies whether columns can be reordered in the chooser.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>true</c>.
    /// </remarks>
    [Parameter, ParameterState]
    [Category(CategoryTypes.DataGrid.Behavior)]
    public bool AllowReorder { get; set; } = true;

    /// <summary>
    /// The placeholder text for the search filter input.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>"Search columns..."</c>.
    /// </remarks>
    [Parameter, ParameterState]
    [Category(CategoryTypes.DataGrid.Appearance)]
    public string? SearchPlaceholder { get; set; } = "Search columns...";

    /// <summary>
    /// Specifies whether the action buttons (Show All, Hide All) are displayed in the footer.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>true</c>.
    /// </remarks>
    [Parameter, ParameterState]
    [Category(CategoryTypes.DataGrid.Appearance)]
    public bool ShowActions { get; set; } = true;

    /// <summary>
    /// Callback invoked when the column chooser is closed.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.DataGrid.Behavior)]
    public EventCallback OnClose { get; set; }

    public DataGridColumnChooser()
    {
        using var registerScope = CreateRegisterScope();
        _titleState = registerScope.RegisterParameter<string>(nameof(Title))
            .WithParameter(() => Title);
        _allowFilteringState = registerScope.RegisterParameter<bool>(nameof(AllowFiltering))
            .WithParameter(() => AllowFiltering);
        _allowReorderState = registerScope.RegisterParameter<bool>(nameof(AllowReorder))
            .WithParameter(() => AllowReorder);
        _searchPlaceholderState = registerScope.RegisterParameter<string?>(nameof(SearchPlaceholder))
            .WithParameter(() => SearchPlaceholder);
        _showActionsState = registerScope.RegisterParameter<bool>(nameof(ShowActions))
            .WithParameter(() => ShowActions);
    }

    private bool IsColumnVisible(Column<T> column) => !column.GetState(x => x.Hidden);

    private bool IsColumnAllowHiding(Column<T> column) => column.GetState(x => x.AllowHiding);

    private void OnSearchChanged(string text)
    {
        _searchText = text;
        StateHasChanged();
    }

    private IEnumerable<Column<T>> GetFilteredColumns()
    {
        var grid = ResolvedDataGrid;
        if (grid is null)
        {
            return [];
        }

        var columns = grid.RenderedColumns;
        if (string.IsNullOrWhiteSpace(_searchText))
        {
            return columns;
        }

        return columns.Where(c => GetColumnTitle(c).Contains(_searchText, StringComparison.OrdinalIgnoreCase));
    }

    private string GetColumnTitle(Column<T> column)
    {
        if (!string.IsNullOrWhiteSpace(column.Title))
        {
            return column.Title;
        }

        if (!string.IsNullOrWhiteSpace(column.PropertyName))
        {
            return column.PropertyName;
        }

        var index = ResolvedDataGrid?.RenderedColumns.IndexOf(column) ?? -1;
        return index >= 0 ? $"Column {index + 1}" : "Column";
    }

    private string GetItemClassname(Column<T> column) => new CssBuilder("vek-column-chooser__item")
        .AddClass("vek-column-chooser__item--disabled", !column.GetState(x => x.AllowHiding))
        .AddClass("vek-column-chooser__item--hidden", column.GetState(x => x.Hidden))
        .Build();

    private async Task OnColumnVisibilityChangedAsync(Column<T> column, bool isVisible)
    {
        if (!column.GetState(x => x.AllowHiding) && !isVisible)
        {
            return;
        }

        var currentlyHidden = column.GetState(x => x.Hidden);
        if (currentlyHidden == isVisible)
        {
            await column.HiddenState.SetValueAsync(!isVisible);
            if (ResolvedDataGrid is IVekStateHasChanged gridState)
            {
                gridState.StateHasChanged();
            }
            StateHasChanged();
        }
    }

    private void OnMoveUp(Column<T> column)
    {
        ResolvedDataGrid?.MoveColumnUp(column);
        StateHasChanged();
    }

    private void OnMoveDown(Column<T> column)
    {
        ResolvedDataGrid?.MoveColumnDown(column);
        StateHasChanged();
    }

    private async Task OnHideAllAsync()
    {
        var grid = ResolvedDataGrid;
        if (grid is null)
        {
            return;
        }

        foreach (var column in grid.RenderedColumns)
        {
            if (column.GetState(x => x.AllowHiding))
            {
                await column.HiddenState.SetValueAsync(true);
            }
        }

        grid.DropContainerHasChanged();
        ((IVekStateHasChanged)grid).StateHasChanged();
        StateHasChanged();
    }

    private async Task OnShowAllAsync()
    {
        var grid = ResolvedDataGrid;
        if (grid is null)
        {
            return;
        }

        foreach (var column in grid.RenderedColumns)
        {
            await column.HiddenState.SetValueAsync(false);
        }

        grid.DropContainerHasChanged();
        ((IVekStateHasChanged)grid).StateHasChanged();
        StateHasChanged();
    }

    private async Task OnCloseAsync()
    {
        if (OnClose.HasDelegate)
        {
            await OnClose.InvokeAsync();
        }
        else if (Context is not null)
        {
            await Context.CloseAsync();
        }
        else if (ResolvedDataGrid is not null)
        {
            await ResolvedDataGrid.HideColumnChooserAsync();
        }
    }
}
