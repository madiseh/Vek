// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

namespace Vek;

/// <summary>
/// Holds the editable form of a filter-builder group.
/// </summary>
public sealed class FilterBuilderDraftGroup
{
    /// <summary>
    /// Gets or sets the logic used to combine this group's children.
    /// </summary>
    public FilterGroupOperator LogicalOperator { get; set; } = FilterGroupOperator.And;

    /// <summary>
    /// Gets the nested conditions and groups in this group.
    /// </summary>
    public List<FilterBuilderDraftNode> Children { get; } = [];
}

/// <summary>
/// Holds one editable condition or nested group in the filter builder.
/// </summary>
public sealed class FilterBuilderDraftNode
{
    /// <summary>
    /// Gets or sets the nested group represented by this node, when present.
    /// </summary>
    public FilterBuilderDraftGroup? Group { get; set; }

    /// <summary>
    /// Gets or sets the selected column index for a condition.
    /// </summary>
    public int ColumnIndex { get; set; }

    /// <summary>
    /// Gets or sets the selected comparison operator.
    /// </summary>
    public string? Operator { get; set; }

    /// <summary>
    /// Gets or sets the value text entered for a condition.
    /// </summary>
    public string? Value { get; set; }
}

/// <summary>
/// Describes a field that can be selected in the visual filter builder.
/// </summary>
public sealed class FilterBuilderColumnOption
{
    /// <summary>
    /// Gets or sets the visible column name.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the operators supported by the column.
    /// </summary>
    public IReadOnlyList<string> Operators { get; set; } = [];

    /// <summary>
    /// Gets or sets the column value type used to validate entered filter values.
    /// </summary>
    public Type ValueType { get; set; } = typeof(string);

    /// <summary>
    /// Gets or sets the optional underlying column reference.
    /// </summary>
    public object? ColumnInstance { get; set; }
}
