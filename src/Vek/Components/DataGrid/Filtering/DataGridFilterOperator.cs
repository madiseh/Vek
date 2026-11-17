// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

namespace Vek;

/// <summary>
/// Specifies the comparison operators available for filtering rows in a <see cref="VekDataGrid{T}"/>.
/// </summary>
public enum DataGridFilterOperator
{
    /// <summary>
    /// The default operator based on the column's data type (Contains for text, Equals for others).
    /// </summary>
    Default = 0,

    /// <summary>
    /// Matches items where the column value equals the filter value.
    /// </summary>
    Equals,

    /// <summary>
    /// Matches items where the column value does not equal the filter value.
    /// </summary>
    NotEquals,

    /// <summary>
    /// Matches string column values that contain the filter value.
    /// </summary>
    Contains,

    /// <summary>
    /// Matches string column values that start with the filter value.
    /// </summary>
    StartsWith,

    /// <summary>
    /// Matches string column values that end with the filter value.
    /// </summary>
    EndsWith,

    /// <summary>
    /// Matches column values greater than the filter value.
    /// </summary>
    GreaterThan,

    /// <summary>
    /// Matches column values greater than or equal to the filter value.
    /// </summary>
    GreaterThanOrEqual,

    /// <summary>
    /// Matches column values less than the filter value.
    /// </summary>
    LessThan,

    /// <summary>
    /// Matches column values less than or equal to the filter value.
    /// </summary>
    LessThanOrEqual,

    /// <summary>
    /// Matches column values that fall between two specified values (inclusive range).
    /// </summary>
    Between,

    /// <summary>
    /// Matches column values that are null or empty.
    /// </summary>
    IsNull,

    /// <summary>
    /// Matches column values that are not null and not empty.
    /// </summary>
    IsNotNull,
}

/// <summary>
/// Provides extension methods and utilities for <see cref="DataGridFilterOperator"/>.
/// </summary>
public static class DataGridFilterOperatorExtensions
{
    /// <summary>
    /// Returns a concise, human-readable display label for the filter operator.
    /// </summary>
    public static string ToDisplayString(this DataGridFilterOperator op) => op switch
    {
        DataGridFilterOperator.Equals => "Equals",
        DataGridFilterOperator.NotEquals => "Does Not Equal",
        DataGridFilterOperator.Contains => "Contains",
        DataGridFilterOperator.StartsWith => "Starts With",
        DataGridFilterOperator.EndsWith => "Ends With",
        DataGridFilterOperator.GreaterThan => "Greater Than",
        DataGridFilterOperator.GreaterThanOrEqual => "Greater Than or Equal",
        DataGridFilterOperator.LessThan => "Less Than",
        DataGridFilterOperator.LessThanOrEqual => "Less Than or Equal",
        DataGridFilterOperator.Between => "Between",
        DataGridFilterOperator.IsNull => "Is Null",
        DataGridFilterOperator.IsNotNull => "Is Not Null",
        _ => "Default",
    };

    /// <summary>
    /// Returns a short visual symbol representing the filter operator.
    /// </summary>
    public static string ToSymbol(this DataGridFilterOperator op) => op switch
    {
        DataGridFilterOperator.Equals => "=",
        DataGridFilterOperator.NotEquals => "≠",
        DataGridFilterOperator.Contains => "∋",
        DataGridFilterOperator.StartsWith => "a…",
        DataGridFilterOperator.EndsWith => "…a",
        DataGridFilterOperator.GreaterThan => ">",
        DataGridFilterOperator.GreaterThanOrEqual => "≥",
        DataGridFilterOperator.LessThan => "<",
        DataGridFilterOperator.LessThanOrEqual => "≤",
        DataGridFilterOperator.Between => "↔",
        DataGridFilterOperator.IsNull => "∅",
        DataGridFilterOperator.IsNotNull => "!∅",
        _ => "=",
    };

    /// <summary>
    /// Returns the default <see cref="DataGridFilterOperator"/> for a given <see cref="FieldType"/>.
    /// </summary>
    public static DataGridFilterOperator GetDefaultOperator(FieldType fieldType)
    {
        if (fieldType.IsString)
        {
            return DataGridFilterOperator.Contains;
        }

        return DataGridFilterOperator.Equals;
    }

    /// <summary>
    /// Returns the supported <see cref="DataGridFilterOperator"/> options for a given <see cref="FieldType"/>.
    /// </summary>
    public static IReadOnlyList<DataGridFilterOperator> GetSupportedOperators(FieldType fieldType)
    {
        if (fieldType.IsString)
        {
            return
            [
                DataGridFilterOperator.Contains,
                DataGridFilterOperator.Equals,
                DataGridFilterOperator.NotEquals,
                DataGridFilterOperator.StartsWith,
                DataGridFilterOperator.EndsWith,
                DataGridFilterOperator.IsNull,
                DataGridFilterOperator.IsNotNull,
            ];
        }

        if (fieldType.IsNumber || fieldType.IsDateTime || fieldType.IsDateOnly)
        {
            return
            [
                DataGridFilterOperator.Equals,
                DataGridFilterOperator.NotEquals,
                DataGridFilterOperator.GreaterThan,
                DataGridFilterOperator.GreaterThanOrEqual,
                DataGridFilterOperator.LessThan,
                DataGridFilterOperator.LessThanOrEqual,
                DataGridFilterOperator.Between,
                DataGridFilterOperator.IsNull,
                DataGridFilterOperator.IsNotNull,
            ];
        }

        if (fieldType.IsBoolean || fieldType.IsEnum || fieldType.IsGuid)
        {
            return
            [
                DataGridFilterOperator.Equals,
                DataGridFilterOperator.NotEquals,
                DataGridFilterOperator.IsNull,
                DataGridFilterOperator.IsNotNull,
            ];
        }

        return
        [
            DataGridFilterOperator.Equals,
            DataGridFilterOperator.NotEquals,
            DataGridFilterOperator.IsNull,
            DataGridFilterOperator.IsNotNull,
        ];
    }
}
