// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Vek;

/// <summary>
/// Combines filter conditions and nested groups with AND or OR logic.
/// </summary>
/// <typeparam name="T">The type of item being filtered.</typeparam>
public sealed class FilterGroupNode<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> : FilterNode<T>
{
    /// <summary>
    /// Gets or sets the logical operator used to combine this group's children.
    /// </summary>
    public FilterGroupOperator LogicalOperator { get; set; } = FilterGroupOperator.And;

    /// <summary>
    /// Gets the conditions and nested groups in this group.
    /// </summary>
    public List<FilterNode<T>> Children { get; } = [];

    /// <summary>
    /// Creates a LINQ expression for this group and its nested children.
    /// </summary>
    /// <param name="filterOptions">Options applied when generating leaf expressions.</param>
    /// <returns>An expression that evaluates the complete group.</returns>
    public override Expression<Func<T, bool>> ToExpression(FilterOptions? filterOptions = null)
    {
        var parameter = Expression.Parameter(typeof(T), "item");
        Expression? body = null;

        foreach (var child in Children)
        {
            var childExpression = child.ToExpression(filterOptions);
            var rewrittenBody = new ParameterReplacer(childExpression.Parameters[0], parameter).Visit(childExpression.Body)!;
            body = body is null
                ? rewrittenBody
                : LogicalOperator == FilterGroupOperator.And
                    ? Expression.AndAlso(body, rewrittenBody)
                    : Expression.OrElse(body, rewrittenBody);
        }

        return Expression.Lambda<Func<T, bool>>(body ?? Expression.Constant(true), parameter);
    }

    /// <inheritdoc />
    public override Func<T, bool> GenerateFilterFunction(FilterOptions? filterOptions = null)
    {
        return ToExpression(filterOptions).Compile();
    }

    /// <inheritdoc />
    public override IFilterDefinition<T> Clone()
    {
        var clone = new FilterGroupNode<T>
        {
            Id = Id,
            LogicalOperator = LogicalOperator,
            Title = Title,
        };

        foreach (var child in Children)
        {
            clone.Children.Add((FilterNode<T>)child.Clone());
        }

        return clone;
    }

    private sealed class ParameterReplacer(ParameterExpression source, ParameterExpression target) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == source ? target : base.VisitParameter(node);
        }
    }
}

/// <summary>
/// Specifies how conditions within a filter group are combined.
/// </summary>
public enum FilterGroupOperator
{
    /// <summary>
    /// Every child condition must match.
    /// </summary>
    And,

    /// <summary>
    /// At least one child condition must match.
    /// </summary>
    Or,
}
