// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Vek;

/// <summary>
/// Defines one condition in a DataGrid filter builder expression.
/// </summary>
/// <typeparam name="T">The type of item being filtered.</typeparam>
public class FilterNode<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> : IFilterDefinition<T>
{
    /// <inheritdoc />
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the column evaluated by this condition.
    /// </summary>
    public Column<T>? Column { get; set; }

    /// <inheritdoc />
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the comparison operator used by this condition.
    /// </summary>
    public string? Operator { get; set; }

    /// <summary>
    /// Gets or sets the value compared with the selected column.
    /// </summary>
    public object? Value { get; set; }

    /// <inheritdoc />
    public FieldType FieldType => FieldType.Identify(Column?.PropertyType);

    /// <summary>
    /// Creates a LINQ expression for this condition.
    /// </summary>
    /// <param name="filterOptions">Options applied when generating the expression.</param>
    /// <returns>The condition expression, or an expression that matches every item when the condition is incomplete.</returns>
    public virtual Expression<Func<T, bool>> ToExpression(FilterOptions? filterOptions = null)
    {
        if (Column is null || string.IsNullOrWhiteSpace(Operator))
        {
            return _ => true;
        }

        return FilterExpressionGenerator.GenerateExpression(this, filterOptions);
    }

    /// <inheritdoc />
    public virtual Func<T, bool> GenerateFilterFunction(FilterOptions? filterOptions = null)
    {
        return ToExpression(filterOptions).Compile();
    }

    /// <inheritdoc />
    public virtual IFilterDefinition<T> Clone()
    {
        return new FilterNode<T>
        {
            Id = Id,
            Column = Column,
            Title = Title,
            Operator = Operator,
            Value = Value,
        };
    }
}
