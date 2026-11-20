// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Vek;

/// <summary>
/// Specifies the display and selection format for hours in <see cref="VekTimePicker"/>.
/// </summary>
public enum TimeEditFormat
{
    /// <summary>
    /// 12-hour format with AM and PM markers (e.g., 01:00 PM to 12:00 AM).
    /// </summary>
    TwelveHour,

    /// <summary>
    /// 24-hour format (e.g., 00:00 to 23:59).
    /// </summary>
    TwentyFourHour
}
