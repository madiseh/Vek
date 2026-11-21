// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Vek.Interfaces;
using Vek.State;
using Vek.Utilities;

namespace Vek;

/// <summary>
/// A visual filter builder component allowing users to construct multi-level boolean expressions with nested AND/OR groups for <see cref="VekDataGrid{T}"/>.
/// </summary>
/// <typeparam name="T">The type of item managed by the data grid.</typeparam>
public partial class VekFilterBuilder<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> : VekComponentBase
{
    private readonly ParameterState<bool> _visibleState;
    private readonly ParameterState<string> _titleState;
    private readonly ParameterState<bool> _asDialogState;

    private FilterBuilderDraftGroup _rootGroup = new();
    private string? _validationMessage;
    private bool _isInitialized;

    protected string Classname => new CssBuilder("vek-filter-builder")
        .AddClass(Class)
        .Build();

    internal VekDataGrid<T>? ResolvedDataGrid => DataGrid ?? CascadedDataGrid;

    [CascadingParameter]
    private VekDataGrid<T>? CascadedDataGrid { get; set; }

    /// <summary>
    /// The target data grid whose columns are filtered.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.DataGrid.Behavior)]
    public VekDataGrid<T>? DataGrid { get; set; }

    /// <summary>
    /// Controls whether the filter builder is displayed.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>true</c>.
    /// </remarks>
    [Parameter, ParameterState]
    [Category(CategoryTypes.DataGrid.Behavior)]
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Notifies when visibility changes.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.DataGrid.Behavior)]
    public EventCallback<bool> VisibleChanged { get; set; }

    /// <summary>
    /// The title displayed in the filter builder header.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>"Filter builder"</c>.
    /// </remarks>
    [Parameter, ParameterState]
    [Category(CategoryTypes.DataGrid.Appearance)]
    public string Title { get; set; } = "Filter builder";

    /// <summary>
    /// Specifies whether to render a dialog overlay and centered modal container.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c>.
    /// </remarks>
    [Parameter, ParameterState]
    [Category(CategoryTypes.DataGrid.Appearance)]
    public bool AsDialog { get; set; }

    /// <summary>
    /// The column options available for filter conditions.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.DataGrid.Filtering)]
    public IReadOnlyList<FilterBuilderColumnOption>? Columns { get; set; }

    /// <summary>
    /// The initial filter group node to load into the builder.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.DataGrid.Filtering)]
    public FilterGroupNode<T>? RootGroup { get; set; }

    /// <summary>
    /// Occurs when the user applies the configured filter tree.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.DataGrid.Filtering)]
    public EventCallback<FilterGroupNode<T>> FilterApplied { get; set; }

    /// <summary>
    /// Occurs when the user cancels or closes the filter builder.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.DataGrid.Behavior)]
    public EventCallback OnClose { get; set; }

    public VekFilterBuilder()
    {
        using var registerScope = CreateRegisterScope();
        _visibleState = registerScope.RegisterParameter<bool>(nameof(Visible))
            .WithParameter(() => Visible)
            .WithEventCallback(() => VisibleChanged);
        _titleState = registerScope.RegisterParameter<string>(nameof(Title))
            .WithParameter(() => Title);
        _asDialogState = registerScope.RegisterParameter<bool>(nameof(AsDialog))
            .WithParameter(() => AsDialog);
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (!_isInitialized)
        {
            InitializeDraft();
            _isInitialized = true;
        }
    }

    private void InitializeDraft()
    {
        if (RootGroup is not null)
        {
            _rootGroup = ConvertToDraftGroup(RootGroup);
            return;
        }

        if (ResolvedDataGrid != null)
        {
            var existing = ResolvedDataGrid.FilterDefinitions.OfType<FilterGroupNode<T>>().FirstOrDefault();
            if (existing is not null)
            {
                _rootGroup = ConvertToDraftGroup(existing);
                return;
            }
        }

        var columns = GetEffectiveColumns();
        if (_rootGroup.Children.Count == 0 && columns.Count > 0)
        {
            _rootGroup.Children.Add(new FilterBuilderDraftNode
            {
                ColumnIndex = 0,
                Operator = columns[0].Operators.FirstOrDefault()
            });
        }
    }

    /// <summary>
    /// Opens the visual filter builder dialog.
    /// </summary>
    public Task ShowFilterBuilderAsync() => _visibleState.SetValueAsync(true);

    /// <summary>
    /// Closes the visual filter builder dialog.
    /// </summary>
    public Task HideFilterBuilderAsync() => _visibleState.SetValueAsync(false);

    /// <summary>
    /// Converts the current draft tree to a strongly typed <see cref="FilterGroupNode{T}"/>.
    /// </summary>
    /// <returns>A filter group node representing the configured filter conditions.</returns>
    public FilterGroupNode<T> ToFilterGroupNode()
    {
        return BuildGroupNode(_rootGroup);
    }

    /// <summary>
    /// Generates a LINQ expression representing the configured filter tree.
    /// </summary>
    /// <param name="filterOptions">Optional filter evaluation options.</param>
    /// <returns>A LINQ filter expression.</returns>
    public Expression<Func<T, bool>> ToExpression(FilterOptions? filterOptions = null)
    {
        return ToFilterGroupNode().ToExpression(filterOptions);
    }

    internal IReadOnlyList<FilterBuilderColumnOption> GetEffectiveColumns()
    {
        if (Columns != null)
        {
            return Columns;
        }

        if (ResolvedDataGrid != null)
        {
            var list = new List<FilterBuilderColumnOption>();
            foreach (var col in ResolvedDataGrid.RenderedColumns.Where(c => c.Filterable == true))
            {
                var fieldType = FieldType.Identify(col.PropertyType);
                list.Add(new FilterBuilderColumnOption
                {
                    Title = col.Title ?? col.PropertyName ?? "Column",
                    Operators = col.GetFilterOperators(fieldType).ToList(),
                    ValueType = col.PropertyType ?? typeof(string),
                    ColumnInstance = col
                });
            }
            return list;
        }

        return [];
    }

    private FilterGroupNode<T> BuildGroupNode(FilterBuilderDraftGroup draftGroup)
    {
        var groupNode = new FilterGroupNode<T>
        {
            LogicalOperator = draftGroup.LogicalOperator
        };

        var columns = GetEffectiveColumns();

        foreach (var draftNode in draftGroup.Children)
        {
            if (draftNode.Group is not null)
            {
                groupNode.Children.Add(BuildGroupNode(draftNode.Group));
            }
            else if (draftNode.ColumnIndex >= 0 && draftNode.ColumnIndex < columns.Count)
            {
                var columnOption = columns[draftNode.ColumnIndex];
                FilterBuilderValueParser.TryParse(draftNode.Value, columnOption.ValueType, out var parsedValue);

                Column<T>? targetColumn = columnOption.ColumnInstance as Column<T>;
                if (targetColumn is null && ResolvedDataGrid != null)
                {
                    targetColumn = ResolvedDataGrid.RenderedColumns.FirstOrDefault(c => (c.Title ?? c.PropertyName) == columnOption.Title)
                        ?? (draftNode.ColumnIndex < ResolvedDataGrid.RenderedColumns.Count ? ResolvedDataGrid.RenderedColumns[draftNode.ColumnIndex] : null);
                }

                var conditionNode = new FilterNode<T>
                {
                    Column = targetColumn,
                    Title = columnOption.Title,
                    Operator = draftNode.Operator,
                    Value = parsedValue
                };
                groupNode.Children.Add(conditionNode);
            }
        }

        return groupNode;
    }

    private FilterBuilderDraftGroup ConvertToDraftGroup(FilterGroupNode<T> groupNode)
    {
        var draftGroup = new FilterBuilderDraftGroup
        {
            LogicalOperator = groupNode.LogicalOperator
        };

        var columns = GetEffectiveColumns();

        foreach (var child in groupNode.Children)
        {
            if (child is FilterGroupNode<T> childGroup)
            {
                draftGroup.Children.Add(new FilterBuilderDraftNode
                {
                    Group = ConvertToDraftGroup(childGroup)
                });
            }
            else
            {
                var columnIndex = 0;
                if (child.Column != null)
                {
                    var idx = -1;
                    for (var i = 0; i < columns.Count; i++)
                    {
                        if (ReferenceEquals(columns[i].ColumnInstance, child.Column) ||
                            string.Equals(columns[i].Title, child.Column.Title ?? child.Column.PropertyName, StringComparison.OrdinalIgnoreCase))
                        {
                            idx = i;
                            break;
                        }
                    }
                    if (idx >= 0)
                    {
                        columnIndex = idx;
                    }
                }

                draftGroup.Children.Add(new FilterBuilderDraftNode
                {
                    ColumnIndex = columnIndex,
                    Operator = child.Operator,
                    Value = child.Value?.ToString()
                });
            }
        }

        return draftGroup;
    }

    private Task OnChangedAsync()
    {
        _validationMessage = null;
        return InvokeAsync(StateHasChanged);
    }

    private async Task OnCloseAsync()
    {
        await _visibleState.SetValueAsync(false);
        if (OnClose.HasDelegate)
        {
            await OnClose.InvokeAsync();
        }
        else if (ResolvedDataGrid is not null)
        {
            await ResolvedDataGrid.HideFilterBuilderAsync();
        }
    }

    private async Task OnClearAsync()
    {
        _rootGroup.Children.Clear();
        _validationMessage = null;

        if (ResolvedDataGrid != null)
        {
            ResolvedDataGrid.FilterDefinitions.RemoveAll(f => f is FilterGroupNode<T>);
            await ResolvedDataGrid.NotifyFilterChangedAsync();
            if (ResolvedDataGrid is IVekStateHasChanged gridState)
            {
                gridState.StateHasChanged();
            }
        }

        await FilterApplied.InvokeAsync(ToFilterGroupNode());
        await InvokeAsync(StateHasChanged);
    }

    private async Task OnApplyAsync()
    {
        if (!ValidateGroup(_rootGroup))
        {
            return;
        }

        var groupNode = ToFilterGroupNode();

        if (ResolvedDataGrid != null)
        {
            ResolvedDataGrid.FilterDefinitions.RemoveAll(f => f is FilterGroupNode<T>);
            if (groupNode.Children.Count > 0)
            {
                ResolvedDataGrid.FilterDefinitions.Add(groupNode);
            }
            await ResolvedDataGrid.NotifyFilterChangedAsync();
            if (ResolvedDataGrid is IVekStateHasChanged gridState)
            {
                gridState.StateHasChanged();
            }
        }

        await FilterApplied.InvokeAsync(groupNode);
        await OnCloseAsync();
    }

    private bool ValidateGroup(FilterBuilderDraftGroup group)
    {
        var columns = GetEffectiveColumns();
        foreach (var node in group.Children)
        {
            if (node.Group is not null)
            {
                if (!ValidateGroup(node.Group))
                {
                    return false;
                }
                continue;
            }

            if (node.ColumnIndex < 0 || node.ColumnIndex >= columns.Count)
            {
                _validationMessage = "Choose a valid field for each condition.";
                return false;
            }

            var op = node.Operator?.ToLowerInvariant() ?? string.Empty;
            if (op.Contains("empty") || op.Contains("null"))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(node.Value))
            {
                _validationMessage = $"Enter a value for {columns[node.ColumnIndex].Title}.";
                return false;
            }

            if (!FilterBuilderValueParser.TryParse(node.Value, columns[node.ColumnIndex].ValueType, out _))
            {
                _validationMessage = $"Enter a valid value for {columns[node.ColumnIndex].Title}.";
                return false;
            }
        }

        return true;
    }

    private Task OnKeyDownAsync(KeyboardEventArgs args)
    {
        return args.Key == "Escape" ? OnCloseAsync() : Task.CompletedTask;
    }
}
