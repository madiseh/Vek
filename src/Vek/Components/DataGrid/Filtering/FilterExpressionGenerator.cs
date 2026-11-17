// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Vek;

/// <summary>
/// Builds LINQ filter expressions from <see cref="VekDataGrid{T}"/> filter definitions and operators for in-memory or <see cref="IQueryable{T}"/> querying.
/// </summary>
public static class FilterExpressionGenerator
{
    /// <summary>
    /// Creates a C# expression for the specified filter and options.
    /// </summary>
    /// <typeparam name="T">The kind of object to filter.</typeparam>
    /// <param name="filter">The filter definition used to generate the expression.</param>
    /// <param name="filterOptions">Any options to apply such as case sensitivity.</param>
    /// <returns>An expression which can be executed to perform a filter.</returns>
    public static Expression<Func<T, bool>> GenerateExpression<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(IFilterDefinition<T> filter, FilterOptions? filterOptions)
    {
        filterOptions ??= FilterOptions.Default; // Default if null
        var propertyExpression = filter.Column?.PropertyExpression;

        if (propertyExpression is null)
        {
            return x => true;
        }

        var fieldType = filter.FieldType;
        var op = NormalizeOperator(filter.Operator);

        if (fieldType.IsString)
        {
            if (op == "empty")
            {
                return propertyExpression.Modify<T>((Expression<Func<string?, bool>>)(x => string.IsNullOrWhiteSpace(x)));
            }

            if (op == "notempty")
            {
                return propertyExpression.Modify<T>((Expression<Func<string?, bool>>)(x => !string.IsNullOrWhiteSpace(x)));
            }

            var value = filter.Value?.ToString();

            if (value is null)
            {
                return x => true;
            }

            if (filterOptions.FilterCaseSensitivity == DataGridFilterCaseSensitivity.Ignore)
            {
                return op switch
                {
                    "contains" =>
                        propertyExpression.Modify<T>((Expression<Func<string?, bool>>)(x => x != null && value != null && x.Contains(value))),
                    "notcontains" =>
                        propertyExpression.Modify<T>((Expression<Func<string?, bool>>)(x => x != null && value != null && !x.Contains(value))),
                    "equal" =>
                        propertyExpression.Modify<T>((Expression<Func<string?, bool>>)(x => x != null && x.Equals(value))),
                    "notequal" =>
                        propertyExpression.Modify<T>((Expression<Func<string?, bool>>)(x => x != null && !x.Equals(value))),
                    "startswith" =>
                        propertyExpression.Modify<T>((Expression<Func<string?, bool>>)(x => x != null && value != null && x.StartsWith(value))),
                    "endswith" =>
                        propertyExpression.Modify<T>((Expression<Func<string?, bool>>)(x => x != null && value != null && x.EndsWith(value))),
                    _ => x => true
                };
            }

            var stringComparer = filterOptions.FilterCaseSensitivity == DataGridFilterCaseSensitivity.Default ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            return op switch
            {
                "contains" =>
                    propertyExpression.Modify<T>((Expression<Func<string?, bool>>)(x => x != null && value != null && x.Contains(value, stringComparer))),
                "notcontains" =>
                    propertyExpression.Modify<T>((Expression<Func<string?, bool>>)(x => x != null && value != null && !x.Contains(value, stringComparer))),
                "equal" =>
                    propertyExpression.Modify<T>((Expression<Func<string?, bool>>)(x => x != null && x.Equals(value, stringComparer))),
                "notequal" =>
                    propertyExpression.Modify<T>((Expression<Func<string?, bool>>)(x => x != null && !x.Equals(value, stringComparer))),
                "startswith" =>
                    propertyExpression.Modify<T>((Expression<Func<string?, bool>>)(x => x != null && value != null && x.StartsWith(value, stringComparer))),
                "endswith" =>
                    propertyExpression.Modify<T>((Expression<Func<string?, bool>>)(x => x != null && value != null && x.EndsWith(value, stringComparer))),
                _ => x => true
            };
        }

        if (fieldType.IsNumber)
        {
            if (op == "empty")
            {
                return propertyExpression.GenerateBinary<T>(ExpressionType.Equal, null);
            }

            if (op == "notempty")
            {
                return propertyExpression.GenerateBinary<T>(ExpressionType.NotEqual, null);
            }

            if (op == "between")
            {
                var (from, to) = ExtractRange(filter.Value);
                return GenerateRangeExpression<T>(propertyExpression, from, to);
            }

            if (filter.Value is null)
            {
                return x => true;
            }

            return op switch
            {
                "equal" => propertyExpression.GenerateBinary<T>(ExpressionType.Equal, filter.Value),
                "notequal" => propertyExpression.GenerateBinary<T>(ExpressionType.NotEqual, filter.Value),
                "greaterthan" => propertyExpression.GenerateBinary<T>(ExpressionType.GreaterThan, filter.Value),
                "greaterthanorequal" => propertyExpression.GenerateBinary<T>(ExpressionType.GreaterThanOrEqual, filter.Value),
                "lessthan" => propertyExpression.GenerateBinary<T>(ExpressionType.LessThan, filter.Value),
                "lessthanorequal" => propertyExpression.GenerateBinary<T>(ExpressionType.LessThanOrEqual, filter.Value),
                _ => x => true
            };
        }

        if (fieldType.IsDateOnly)
        {
            if (op == "empty")
            {
                return propertyExpression.GenerateBinary<T>(ExpressionType.Equal, null);
            }

            if (op == "notempty")
            {
                return propertyExpression.GenerateBinary<T>(ExpressionType.NotEqual, null);
            }

            if (op == "between")
            {
                var (from, to) = ExtractRange(filter.Value);
                return GenerateRangeExpression<T>(propertyExpression, from, to);
            }

            if (filter.Value is null)
            {
                return x => true;
            }

            return op switch
            {
                "equal" => propertyExpression.GenerateBinary<T>(ExpressionType.Equal, filter.Value),
                "notequal" => propertyExpression.GenerateBinary<T>(ExpressionType.NotEqual, filter.Value),
                "greaterthan" => propertyExpression.GenerateBinary<T>(ExpressionType.GreaterThan, filter.Value),
                "greaterthanorequal" => propertyExpression.GenerateBinary<T>(ExpressionType.GreaterThanOrEqual, filter.Value),
                "lessthan" => propertyExpression.GenerateBinary<T>(ExpressionType.LessThan, filter.Value),
                "lessthanorequal" => propertyExpression.GenerateBinary<T>(ExpressionType.LessThanOrEqual, filter.Value),
                _ => x => true
            };
        }

        if (fieldType.IsDateTime)
        {
            if (op == "empty")
            {
                return propertyExpression.GenerateBinary<T>(ExpressionType.Equal, null);
            }

            if (op == "notempty")
            {
                return propertyExpression.GenerateBinary<T>(ExpressionType.NotEqual, null);
            }

            if (op == "between")
            {
                var (from, to) = ExtractRange(filter.Value);
                return GenerateRangeExpression<T>(propertyExpression, from, to);
            }

            if (filter.Value is null)
            {
                return x => true;
            }

            return op switch
            {
                "equal" => propertyExpression.GenerateBinary<T>(ExpressionType.Equal, filter.Value),
                "notequal" => propertyExpression.GenerateBinary<T>(ExpressionType.NotEqual, filter.Value),
                "greaterthan" => propertyExpression.GenerateBinary<T>(ExpressionType.GreaterThan, filter.Value),
                "greaterthanorequal" => propertyExpression.GenerateBinary<T>(ExpressionType.GreaterThanOrEqual, filter.Value),
                "lessthan" => propertyExpression.GenerateBinary<T>(ExpressionType.LessThan, filter.Value),
                "lessthanorequal" => propertyExpression.GenerateBinary<T>(ExpressionType.LessThanOrEqual, filter.Value),
                _ => x => true
            };
        }

        if (fieldType.IsBoolean)
        {
            if (op == "empty")
            {
                return propertyExpression.GenerateBinary<T>(ExpressionType.Equal, null);
            }

            if (op == "notempty")
            {
                return propertyExpression.GenerateBinary<T>(ExpressionType.NotEqual, null);
            }

            if (filter.Value is null)
            {
                return x => true;
            }

            return op switch
            {
                "equal" => propertyExpression.GenerateBinary<T>(ExpressionType.Equal, filter.Value),
                "notequal" => propertyExpression.GenerateBinary<T>(ExpressionType.NotEqual, filter.Value),
                _ => x => true
            };
        }

        if (fieldType.IsEnum)
        {
            if (op == "empty")
            {
                return propertyExpression.GenerateBinary<T>(ExpressionType.Equal, null);
            }

            if (op == "notempty")
            {
                return propertyExpression.GenerateBinary<T>(ExpressionType.NotEqual, null);
            }

            if (filter.Value is null && op != "empty" && op != "notempty")
            {
                return x => true;
            }

            return op switch
            {
                "equal" => propertyExpression.GenerateBinary<T>(ExpressionType.Equal, filter.Value),
                "notequal" => propertyExpression.GenerateBinary<T>(ExpressionType.NotEqual, filter.Value),
                "isanyof" => propertyExpression.GenerateAnyOf<T>(filter.Value as IEnumerable ?? new[] { filter.Value }),
                _ => x => true
            };
        }

        if (fieldType.IsGuid)
        {
            if (op == "empty")
            {
                return propertyExpression.GenerateBinary<T>(ExpressionType.Equal, null);
            }

            if (op == "notempty")
            {
                return propertyExpression.GenerateBinary<T>(ExpressionType.NotEqual, null);
            }

            return op switch
            {
                "equal" => propertyExpression.GenerateBinary<T>(ExpressionType.Equal, filter.Value),
                "notequal" => propertyExpression.GenerateBinary<T>(ExpressionType.NotEqual, filter.Value),
                _ => x => true
            };
        }

        return x => true;
    }

    private static string NormalizeOperator(string? op)
    {
        if (string.IsNullOrWhiteSpace(op))
        {
            return string.Empty;
        }

        return op.Trim().ToLowerInvariant() switch
        {
            "=" or "equals" or "equal" or "is" => "equal",
            "!=" or "<>" or "not equals" or "not equal" or "is not" or "isnot" or "notequals" => "notequal",
            "contains" => "contains",
            "not contains" or "notcontains" or "does not contain" => "notcontains",
            "starts with" or "startswith" => "startswith",
            "ends with" or "endswith" => "endswith",
            ">" or "greater than" or "greaterthan" or "after" or "is after" or "isafter" => "greaterthan",
            ">=" or "greater than or equal" or "greaterthanorequal" or "on or after" or "onorafter" or "is on or after" or "isonorafter" => "greaterthanorequal",
            "<" or "less than" or "lessthan" or "before" or "is before" or "isbefore" => "lessthan",
            "<=" or "less than or equal" or "lessthanorequal" or "on or before" or "onorbefore" or "is on or before" or "isonorbefore" => "lessthanorequal",
            "between" or "in range" or "inrange" => "between",
            "is empty" or "isempty" or "empty" or "is null" or "isnull" => "empty",
            "is not empty" or "isnotempty" or "not empty" or "notempty" or "is not null" or "isnotnull" => "notempty",
            "is any of" or "isanyof" => "isanyof",
            _ => op.ToLowerInvariant()
        };
    }

    private static (object? From, object? To) ExtractRange(object? value)
    {
        if (value is DataGridFilterRange range)
        {
            return (range.From, range.To);
        }

        if (value is ValueTuple<object?, object?> vt)
        {
            return (vt.Item1, vt.Item2);
        }

        if (value is Tuple<object?, object?> t)
        {
            return (t.Item1, t.Item2);
        }

        if (value is object?[] arr && arr.Length >= 2)
        {
            return (arr[0], arr[1]);
        }

        if (value is string s && s.Contains(".."))
        {
            var parts = s.Split("..", 2);
            var from = string.IsNullOrWhiteSpace(parts[0]) ? null : parts[0].Trim();
            var to = string.IsNullOrWhiteSpace(parts[1]) ? null : parts[1].Trim();
            return (from, to);
        }

        return (value, null);
    }

    private static Expression<Func<T, bool>> GenerateRangeExpression<T>(LambdaExpression propertyExpression, object? from, object? to)
    {
        if (from is null && to is null)
        {
            return x => true;
        }

        if (from is not null && to is not null)
        {
            var greater = propertyExpression.GenerateBinary<T>(ExpressionType.GreaterThanOrEqual, from);
            var lesser = propertyExpression.GenerateBinary<T>(ExpressionType.LessThanOrEqual, to);
            return CombineAnd(greater, lesser);
        }

        if (from is not null)
        {
            return propertyExpression.GenerateBinary<T>(ExpressionType.GreaterThanOrEqual, from);
        }

        return propertyExpression.GenerateBinary<T>(ExpressionType.LessThanOrEqual, to);
    }

    private static Expression<Func<T, bool>> CombineAnd<T>(Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var firstBody = new ExpressionReplacer(first.Parameters[0], parameter).Visit(first.Body);
        var secondBody = new ExpressionReplacer(second.Parameters[0], parameter).Visit(second.Body);
        return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(firstBody, secondBody), parameter);
    }
}
