// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Vek.Charts;

namespace Vek;

/// <summary>
/// Options specific to pie charts, extending <see cref="DefaultRadialChartOptions"/>.
/// </summary>
public class PieChartOptions : DefaultRadialChartOptions, IHasValueLabelOptions
{
    /// <summary>
    /// Whether values should be displayed within the chart.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c>.
    /// </remarks>
    public bool ShowValues { get; set; } = false;

    public static implicit operator PieChartOptions(ChartOptions options) => new()
    {
        ShowLegend = options.ShowLegend,
        ShowToolTips = options.ShowToolTips,
        TooltipTitleFormat = options.TooltipTitleFormat,
        TooltipSubtitleFormat = options.TooltipSubtitleFormat,
        ChartPalette = options.ChartPalette,
    };
}
