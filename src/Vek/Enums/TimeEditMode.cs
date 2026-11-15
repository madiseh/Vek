// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Vek;

/// <summary>
/// Indicates the editable values of a <see cref="VekTimePicker"/>.
/// </summary>
public enum TimeEditMode
{
    /// <summary>
    /// Hours and minutes can be edited.
    /// </summary>
    Normal,

    /// <summary>
    /// Only minutes can be edited.
    /// </summary>
    OnlyMinutes,

    /// <summary>
    /// Only hours can be edited.
    /// </summary>
    OnlyHours
}
