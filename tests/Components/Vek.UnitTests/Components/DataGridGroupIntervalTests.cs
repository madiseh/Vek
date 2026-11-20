// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using AwesomeAssertions;
using NUnit.Framework;
using TestComponents.DataGrid;

namespace Vek.UnitTests.Components;

public class DataGridGroupIntervalTests : BunitTest
{
    /// <summary>
    /// Date intervals create readable group keys and combine rows from the same month.
    /// </summary>
    [Test]
    public void DateMonthIntervalGroupsRowsUnderFormattedMonth()
    {
        var component = Context.Render<DataGridGroupIntervalTest>();

        component.Markup.Should().Contain("Joined: March 2025");
        component.Markup.Split("Joined: March 2025").Should().HaveCount(2);
    }

    /// <summary>
    /// Changing an interval parameter rebuilds the group's visible key.
    /// </summary>
    [Test]
    public async Task ChangingDateIntervalRebuildsGroups()
    {
        var component = Context.Render<DataGridGroupIntervalTest>();

        await component.SetParametersAndRenderAsync(parameters => parameters
            .Add(x => x.Interval, DataGridGroupInterval.DateQuarter));

        component.Markup.Should().Contain("Joined: Q1, 2025");
    }

    /// <summary>
    /// A custom interval calculator replaces the built-in bucket and preserves the supplied group key.
    /// </summary>
    [Test]
    public void CustomCalculatorOverridesBuiltInInterval()
    {
        var component = Context.Render<DataGridGroupIntervalTest>(parameters => parameters
            .Add(x => x.Calculator, value => $"Month {((DateTime)value!).Month}"));

        component.Markup.Should().Contain("Joined: Month 3");
    }

    /// <summary>
    /// Numeric ranges use floor-based buckets so negative values fall into the expected lower interval.
    /// </summary>
    [Test]
    public void NumericRangeHandlesNegativeAndBoundaryValues()
    {
        DataGridGroupIntervalEvaluator.Evaluate(-1, DataGridGroupInterval.NumericRange, 10).Should().Be("-10–0");
        DataGridGroupIntervalEvaluator.Evaluate(10, DataGridGroupInterval.NumericRange, 10).Should().Be("10–20");
        var invalidInterval = () => DataGridGroupIntervalEvaluator.Evaluate(1, DataGridGroupInterval.NumericRange, 0);
        invalidInterval.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Alphabetical intervals ignore case and use a readable fallback for non-letter values.
    /// </summary>
    [Test]
    public void AlphabeticalIntervalUsesFirstLetterOrFallback()
    {
        DataGridGroupIntervalEvaluator.Evaluate("alpine", DataGridGroupInterval.Alphabetical, 10).Should().Be("A");
        DataGridGroupIntervalEvaluator.Evaluate("42", DataGridGroupInterval.Alphabetical, 10).Should().Be("#");
    }

    /// <summary>
    /// Date year and quarter intervals return concise, readable group labels.
    /// </summary>
    [Test]
    public void DateYearAndQuarterIntervalsFormatLabels()
    {
        var date = new DateTime(2025, 2, 10);

        DataGridGroupIntervalEvaluator.Evaluate(date, DataGridGroupInterval.DateYear, 10).Should().Be("2025");
        DataGridGroupIntervalEvaluator.Evaluate(date, DataGridGroupInterval.DateQuarter, 10).Should().Be("Q1, 2025");
    }
}
