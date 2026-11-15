// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Vek.Justification.StackedBars;

internal interface IStackedBarPositionStrategy
{
    double[] CalculatePositions(StackedBarContext ctx);
}

internal static class StackedBarStrategyFactory
{
    public static IStackedBarPositionStrategy GetStrategy(Justify justify) => justify switch
    {
        Justify.FlexStart => new FlexStartStrategy(),
        Justify.FlexEnd => new FlexEndStrategy(),
        Justify.Center => new CenterStrategy(),
        Justify.SpaceBetween => new SpaceBetweenStrategy(),
        Justify.SpaceAround => new SpaceAroundStrategy(),
        Justify.SpaceEvenly => new SpaceEvenlyStrategy(),
        _ => throw new NotSupportedException($"Unsupported Justification: {justify}")
    };
}

/// <summary>
/// Layout measurements passed to a stacked-bar positioning strategy to compute the horizontal placement of each bar column.
/// </summary>
public class StackedBarContext
{
    public int MaxColumns { get; init; }
    public double BarWidth { get; init; }
    public int SpaceBetweenBars { get; init; }
    public double HorizontalSpace { get; init; }
    public double HorizontalStartSpace { get; init; }
    public double HorizontalEndSpace { get; init; }
}
