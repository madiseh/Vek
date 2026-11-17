// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Vek.Extensions;
using Vek.Interfaces;
using Vek.Utilities;

namespace Vek;

/// <summary>
/// Represents an advanced filter row cell in a <see cref="VekDataGrid{T}"/> with operator selector and debounced input.
/// </summary>
/// <typeparam name="T">The type of item managed by the data grid.</typeparam>
public partial class DataGridFilterCell<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> : VekComponentBase
{
    private DataGridFilterOperator? _selectedOperator;

    /// <summary>
    /// The parent <see cref="VekDataGrid{T}"/> containing this filter cell.
    /// </summary>
    [CascadingParameter]
    public VekDataGrid<T> DataGrid { get; set; } = null!;

    /// <summary>
    /// The column associated with this filter cell.
    /// </summary>
    [Parameter, EditorRequired]
    [Category(CategoryTypes.DataGrid.Filtering)]
    public required Column<T> Column { get; set; }

    /// <summary>
    /// Custom template content rendered within this filter cell.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.DataGrid.Behavior)]
    public RenderFragment? ChildContent { get; set; }

    private string Classname =>
        new CssBuilder(Column?.HeaderClass)
            .AddClass(Column?.HeaderClassname)
            .AddClass(Class)
            .AddClass("vek-table-cell")
            .AddClass("filter-header-cell")
            .AddClass("vek-datagrid-filter-cell")
            .Build();

    private string Stylename =>
        new StyleBuilder()
            .AddStyle(Column?.HeaderStyle)
            .AddStyle("left", Column?.StickyLeftOffsetPx, when: Column?.StickyLeftOffsetPx is not null)
            .AddStyle("right", Column?.StickyRightOffsetPx, when: Column?.StickyRightOffsetPx is not null)
            .AddStyle(Style)
            .Build();

    private FieldType FieldType => FieldType.Identify(Column?.PropertyType);

    private double DebounceDelay => DataGrid is not null ? DataGrid.GetState(x => x.FilterRowInputDelay) : 300.0;

    private string FilterAriaLabel => $"Filter by {Column?.Title ?? "column"}";

    private DataGridFilterOperator CurrentOperator
    {
        get
        {
            if (_selectedOperator.HasValue)
            {
                return _selectedOperator.Value;
            }

            if (Column?.FilterContext.FilterDefinition?.Operator is { } opStr &&
                Enum.TryParse<DataGridFilterOperator>(opStr, true, out var parsed))
            {
                return parsed;
            }

            if (Column is not null)
            {
                var columnOp = Column.GetState(x => x.FilterOperator);
                if (columnOp != DataGridFilterOperator.Default)
                {
                    return columnOp;
                }
            }

            return DataGridFilterOperatorExtensions.GetDefaultOperator(FieldType);
        }
    }

    private string CurrentOperatorSymbol => CurrentOperator.ToSymbol();

    private IReadOnlyList<DataGridFilterOperator> SupportedOperators =>
        DataGridFilterOperatorExtensions.GetSupportedOperators(FieldType);

    private string? StringValue =>
        FieldType.IsString && Column?.FilterContext.FilterDefinition?.Value is not null
            ? Column.FilterContext.FilterDefinition.Value.ToString()
            : null;

    private double? NumberValue =>
        FieldType.IsNumber && Column?.FilterContext.FilterDefinition?.Value is not null
            ? Convert.ToDouble(Column.FilterContext.FilterDefinition.Value)
            : null;

    private bool? BoolValue =>
        FieldType.IsBoolean && Column?.FilterContext.FilterDefinition?.Value is not null
            ? (bool?)Column.FilterContext.FilterDefinition.Value
            : null;

    private Enum? EnumValue =>
        FieldType.IsEnum && Column?.FilterContext.FilterDefinition?.Value is not null
            ? (Enum)Column.FilterContext.FilterDefinition.Value
            : null;

    private DateTime? DateTimeValueForPicker =>
        FieldType.IsDateTime && Column?.FilterContext.FilterDefinition?.Value is not null
            ? (DateTime?)Column.FilterContext.FilterDefinition.Value
            : null;

    private DateTime? DateOnlyValueForPicker =>
        FieldType.IsDateOnly && Column?.FilterContext.FilterDefinition?.Value is not null
            ? ((DateOnly)Column.FilterContext.FilterDefinition.Value).ToDateTime(TimeOnly.MinValue)
            : null;

    private double? RangeFromNumber
    {
        get
        {
            if (Column?.FilterContext.FilterDefinition?.Value is DataGridFilterRange range && range.From is not null)
            {
                return Convert.ToDouble(range.From);
            }
            return null;
        }
    }

    private double? RangeToNumber
    {
        get
        {
            if (Column?.FilterContext.FilterDefinition?.Value is DataGridFilterRange range && range.To is not null)
            {
                return Convert.ToDouble(range.To);
            }
            return null;
        }
    }

    private DateTime? RangeFromDate
    {
        get
        {
            if (Column?.FilterContext.FilterDefinition?.Value is DataGridFilterRange range)
            {
                if (range.From is DateTime dt)
                {
                    return dt;
                }
                if (range.From is DateOnly d)
                {
                    return d.ToDateTime(TimeOnly.MinValue);
                }
            }
            return null;
        }
    }

    private DateTime? RangeToDate
    {
        get
        {
            if (Column?.FilterContext.FilterDefinition?.Value is DataGridFilterRange range)
            {
                if (range.To is DateTime dt)
                {
                    return dt;
                }
                if (range.To is DateOnly d)
                {
                    return d.ToDateTime(TimeOnly.MinValue);
                }
            }
            return null;
        }
    }

    private string GetChosenOperatorStyle(DataGridFilterOperator op)
    {
        return op == CurrentOperator
            ? "color:var(--vek-palette-primary-text);background-color:var(--vek-palette-primary)"
            : string.Empty;
    }

    private void EnsureFilterDefinition()
    {
        if (Column.FilterContext.FilterDefinition is null)
        {
            var filterDef = DataGrid.CreateFilterDefinitionInstance();
            filterDef.Column = Column;
            filterDef.Title = Column.Title;
            filterDef.Operator = CurrentOperator.ToString();
            Column.FilterContext.FilterDefinition = filterDef;
        }
        else
        {
            Column.FilterContext.FilterDefinition.Operator = CurrentOperator.ToString();
        }
    }

    private async Task OnOperatorSelectedAsync(DataGridFilterOperator op)
    {
        _selectedOperator = op;
        EnsureFilterDefinition();
        Column.FilterContext.FilterDefinition!.Operator = op.ToString();

        if (op is DataGridFilterOperator.IsNull or DataGridFilterOperator.IsNotNull)
        {
            Column.FilterContext.FilterDefinition.Value = null;
            await ApplyFilterAsync(Column.FilterContext.FilterDefinition);
        }
        else if (Column.FilterContext.FilterDefinition.Value is not null)
        {
            await ApplyFilterAsync(Column.FilterContext.FilterDefinition);
        }
    }

    private async Task OnStringValueChangedAsync(string? value)
    {
        EnsureFilterDefinition();
        Column.FilterContext.FilterDefinition!.Value = string.IsNullOrWhiteSpace(value) ? null : value;
        await ApplyFilterAsync(Column.FilterContext.FilterDefinition);
    }

    private async Task OnNumberValueChangedAsync(double? value)
    {
        EnsureFilterDefinition();
        Column.FilterContext.FilterDefinition!.Value = value;
        await ApplyFilterAsync(Column.FilterContext.FilterDefinition);
    }

    private async Task OnBoolValueChangedAsync(bool? value)
    {
        EnsureFilterDefinition();
        Column.FilterContext.FilterDefinition!.Value = value;
        await ApplyFilterAsync(Column.FilterContext.FilterDefinition);
    }

    private async Task OnEnumValueChangedAsync(Enum? value)
    {
        EnsureFilterDefinition();
        Column.FilterContext.FilterDefinition!.Value = value;
        await ApplyFilterAsync(Column.FilterContext.FilterDefinition);
    }

    private async Task OnDateTimeValueChangedAsync(DateTime? value)
    {
        EnsureFilterDefinition();
        Column.FilterContext.FilterDefinition!.Value = value;
        await ApplyFilterAsync(Column.FilterContext.FilterDefinition);
    }

    private async Task OnDateOnlyValueChangedAsync(DateTime? value)
    {
        EnsureFilterDefinition();
        Column.FilterContext.FilterDefinition!.Value = value.HasValue ? DateOnly.FromDateTime(value.Value) : null;
        await ApplyFilterAsync(Column.FilterContext.FilterDefinition);
    }

    private async Task OnRangeFromNumberChangedAsync(double? value)
    {
        EnsureFilterDefinition();
        var range = Column.FilterContext.FilterDefinition!.Value as DataGridFilterRange ?? new DataGridFilterRange();
        range.From = value;
        Column.FilterContext.FilterDefinition.Value = range.IsEmpty ? null : range;
        await ApplyFilterAsync(Column.FilterContext.FilterDefinition);
    }

    private async Task OnRangeToNumberChangedAsync(double? value)
    {
        EnsureFilterDefinition();
        var range = Column.FilterContext.FilterDefinition!.Value as DataGridFilterRange ?? new DataGridFilterRange();
        range.To = value;
        Column.FilterContext.FilterDefinition.Value = range.IsEmpty ? null : range;
        await ApplyFilterAsync(Column.FilterContext.FilterDefinition);
    }

    private async Task OnRangeFromDateOnlyChangedAsync(DateTime? value)
    {
        EnsureFilterDefinition();
        var range = Column.FilterContext.FilterDefinition!.Value as DataGridFilterRange ?? new DataGridFilterRange();
        range.From = value.HasValue ? DateOnly.FromDateTime(value.Value) : null;
        Column.FilterContext.FilterDefinition.Value = range.IsEmpty ? null : range;
        await ApplyFilterAsync(Column.FilterContext.FilterDefinition);
    }

    private async Task OnRangeToDateOnlyChangedAsync(DateTime? value)
    {
        EnsureFilterDefinition();
        var range = Column.FilterContext.FilterDefinition!.Value as DataGridFilterRange ?? new DataGridFilterRange();
        range.To = value.HasValue ? DateOnly.FromDateTime(value.Value) : null;
        Column.FilterContext.FilterDefinition.Value = range.IsEmpty ? null : range;
        await ApplyFilterAsync(Column.FilterContext.FilterDefinition);
    }

    private async Task OnRangeFromDateTimeChangedAsync(DateTime? value)
    {
        EnsureFilterDefinition();
        var range = Column.FilterContext.FilterDefinition!.Value as DataGridFilterRange ?? new DataGridFilterRange();
        range.From = value;
        Column.FilterContext.FilterDefinition.Value = range.IsEmpty ? null : range;
        await ApplyFilterAsync(Column.FilterContext.FilterDefinition);
    }

    private async Task OnRangeToDateTimeChangedAsync(DateTime? value)
    {
        EnsureFilterDefinition();
        var range = Column.FilterContext.FilterDefinition!.Value as DataGridFilterRange ?? new DataGridFilterRange();
        range.To = value;
        Column.FilterContext.FilterDefinition.Value = range.IsEmpty ? null : range;
        await ApplyFilterAsync(Column.FilterContext.FilterDefinition);
    }

    internal Task CallApplyFilterAsync(IFilterDefinition<T> filterDefinition) => ApplyFilterAsync(filterDefinition);

    internal Task CallClearFilterAsync() => ClearFilterAsync();

    private async Task ApplyFilterAsync(IFilterDefinition<T> filterDefinition)
    {
        if (DataGrid is null)
        {
            return;
        }

        if (DataGrid.FilterDefinitions.All(x => x.Id != filterDefinition.Id))
        {
            DataGrid.FilterDefinitions.Add(filterDefinition);
        }

        if (DataGrid.HasServerData)
        {
            await DataGrid.ReloadServerData();
        }

        DataGrid.GroupItems(noStateChange: true);
        DataGrid.DropContainerHasChanged();
        var filterChanged = DataGrid.NotifyFilterChangedAsync();
        if (!filterChanged.IsCompleted)
        {
            ((IVekStateHasChanged)DataGrid).StateHasChanged();
        }
        await filterChanged;
        ((IVekStateHasChanged)DataGrid).StateHasChanged();
    }

    private async Task ClearFilterAsync()
    {
        _selectedOperator = null;
        if (Column?.FilterContext.FilterDefinition is not null)
        {
            Column.FilterContext.FilterDefinition.Value = null;
            await DataGrid.RemoveFilterAsync(Column.FilterContext.FilterDefinition.Id);
        }
    }
}
