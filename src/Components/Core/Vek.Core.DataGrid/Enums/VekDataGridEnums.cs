namespace Vek.Core.DataGrid;

/// <summary>
/// Describes the direction applied to a grid sort definition.
/// </summary>
public enum VekSortDirection
{
    /// <summary>Sorts values from lowest to highest.</summary>
    Ascending,
    /// <summary>Sorts values from highest to lowest.</summary>
    Descending
}

/// <summary>
/// Determines how many grid columns may contribute to sorting.
/// </summary>
public enum VekDataGridSortMode
{
    /// <summary>Disables sorting for all columns.</summary>
    None,
    /// <summary>Sorts by one column at a time.</summary>
    Single,
    /// <summary>Allows multiple columns to contribute to the sort order.</summary>
    Multiple
}

/// <summary>
/// Describes a comparison used to filter a grid column.
/// </summary>
public enum VekDataGridFilterOperator
{
    /// <summary>Matches equal values.</summary>
    Equals,
    /// <summary>Matches unequal values.</summary>
    NotEquals,
    /// <summary>Matches strings containing a value.</summary>
    Contains,
    /// <summary>Matches strings beginning with a value.</summary>
    StartsWith,
    /// <summary>Matches strings ending with a value.</summary>
    EndsWith,
    /// <summary>Matches values greater than a value.</summary>
    GreaterThan,
    /// <summary>Matches values greater than or equal to a value.</summary>
    GreaterThanOrEqual,
    /// <summary>Matches values less than a value.</summary>
    LessThan,
    /// <summary>Matches values less than or equal to a value.</summary>
    LessThanOrEqual,
    /// <summary>Matches null or empty values.</summary>
    IsEmpty,
    /// <summary>Matches nonempty values.</summary>
    IsNotEmpty,
    /// <summary>Matches null values.</summary>
    IsNull,
    /// <summary>Matches non-null values.</summary>
    IsNotNull
}

/// <summary>
/// Controls whether the grid permits no, one, or multiple selected rows.
/// </summary>
public enum VekDataGridSelectionMode
{
    /// <summary>Row selection is disabled.</summary>
    None,
    /// <summary>Only one row can be selected at a time.</summary>
    Single,
    /// <summary>Multiple rows can be selected at a time.</summary>
    Multiple
}

/// <summary>
/// Controls selection retention when an active filter hides selected rows.
/// </summary>
public enum VekDataGridFilteredSelectionMode
{
    /// <summary>Keep selected items while they are filtered out.</summary>
    Keep,
    /// <summary>Remove selected items when a filter hides them.</summary>
    Clear
}

/// <summary>
/// Controls how grid rows expose their editable fields.
/// </summary>
public enum VekDataGridEditMode
{
    /// <summary>Editing is disabled.</summary>
    None,
    /// <summary>Edits one cell at a time.</summary>
    Cell,
    /// <summary>Edits all configured cells for one row at a time.</summary>
    Row,
    /// <summary>Edits one row through a form-oriented template.</summary>
    Form
}

/// <summary>
/// Defines the ordering applied to the values in a grouped column.
/// </summary>
public enum VekDataGridGroupSortOrder
{
    /// <summary>Orders group values from lowest to highest.</summary>
    Ascending,
    /// <summary>Orders group values from highest to lowest.</summary>
    Descending
}

/// <summary>
/// Chooses the calculation performed for an aggregate definition.
/// </summary>
public enum VekDataGridAggregateType
{
    /// <summary>Adds the non-null numeric values.</summary>
    Sum,
    /// <summary>Calculates the mean of non-null numeric values.</summary>
    Average,
    /// <summary>Counts non-null selected values.</summary>
    Count,
    /// <summary>Returns the least non-null selected value.</summary>
    Min,
    /// <summary>Returns the greatest non-null selected value.</summary>
    Max,
    /// <summary>Uses the aggregate delegate supplied by the caller.</summary>
    Custom
}

/// <summary>
/// Controls whether an aggregate appears in the overall footer, group headers, or both.
/// </summary>
public enum VekDataGridAggregateScope
{
    /// <summary>Shows the aggregate in the grid footer.</summary>
    Footer,
    /// <summary>Shows the aggregate in group headers.</summary>
    Group,
    /// <summary>Shows the aggregate in the footer and each group.</summary>
    Both
}
