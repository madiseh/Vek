using System.Globalization;
using AngleSharp.Dom;
using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using NUnit.Framework;

namespace Vek.UnitTests.Components.DatePicker;

#nullable enable
[TestFixture]
public class VekCalendarTests : BunitTest
{
    [Test]
    public void DefaultState_ShouldRenderInlineCalendarWithoutPopovers()
    {
        var comp = Context.Render<VekCalendar>();
        var calendar = comp.Instance;

        calendar.Date.Should().BeNull();
        calendar.SelectedDates.Should().BeNull();
        calendar.MultiSelection.Should().BeFalse();
        calendar.ShowWeekNumbers.Should().BeFalse();
        calendar.ShowToolbar.Should().BeTrue();
        calendar.OpenTo.Should().Be(OpenTo.Date);
        calendar.PickerVariant.Should().Be(PickerVariant.Static);
        calendar.Disabled.Should().BeFalse();
        calendar.ReadOnly.Should().BeFalse();

        // Must be embedded inline directly without popovers or inputs
        comp.Find(".vek-calendar").Should().NotBeNull();
        comp.Find(".vek-picker-static").Should().NotBeNull();
        comp.FindAll(".vek-popover").Should().BeEmpty();
        comp.FindAll("input").Should().BeEmpty();
    }

    [Test]
    public void SingleSelection_ShouldSelectDateOnClick()
    {
        DateTime? selectedDate = null;
        var initialMonth = new DateTime(2026, 5, 1);

        var comp = Context.Render<VekCalendar>(parameters => parameters
            .Add(p => p.PickerMonth, initialMonth)
            .Add(p => p.Date, selectedDate)
            .Add(p => p.DateChanged, EventCallback.Factory.Create<DateTime?>(this, d => selectedDate = d)));

        // Find day button for 15th
        var day15Button = comp.FindAll("button.vek-picker-calendar-day")
            .First(b => b.TextContent.Trim() == "15");

        day15Button.Click();

        selectedDate.Should().Be(new DateTime(2026, 5, 15));
        comp.Instance.SelectedDate.Should().Be(new DateTime(2026, 5, 15));

        // Re-rendered day button should have selected class
        var updatedDay15 = comp.FindAll("button.vek-picker-calendar-day")
            .First(b => b.TextContent.Trim() == "15");
        updatedDay15.ClassList.Should().Contain("vek-selected");
    }

    [Test]
    public void MultiSelection_ShouldToggleMultipleDates()
    {
        IReadOnlyCollection<DateTime>? selectedDates = null;
        var initialMonth = new DateTime(2026, 5, 1);

        var comp = Context.Render<VekCalendar>(parameters => parameters
            .Add(p => p.MultiSelection, true)
            .Add(p => p.PickerMonth, initialMonth)
            .Add(p => p.SelectedDates, selectedDates)
            .Add(p => p.SelectedDatesChanged, EventCallback.Factory.Create<IReadOnlyCollection<DateTime>?>(this, d => selectedDates = d)));

        var day10Button = comp.FindAll("button.vek-picker-calendar-day")
            .First(b => b.TextContent.Trim() == "10");
        day10Button.Click();

        selectedDates.Should().NotBeNull();
        selectedDates!.Count.Should().Be(1);
        selectedDates.First().Should().Be(new DateTime(2026, 5, 10));

        var day12Button = comp.FindAll("button.vek-picker-calendar-day")
            .First(b => b.TextContent.Trim() == "12");
        day12Button.Click();

        selectedDates.Count.Should().Be(2);
        selectedDates.Should().Contain(new DateTime(2026, 5, 10));
        selectedDates.Should().Contain(new DateTime(2026, 5, 12));

        // Toggle day 10 off
        day10Button = comp.FindAll("button.vek-picker-calendar-day")
            .First(b => b.TextContent.Trim() == "10");
        day10Button.Click();

        selectedDates.Count.Should().Be(1);
        selectedDates.First().Should().Be(new DateTime(2026, 5, 12));
    }

    [Test]
    public void ShowWeekNumbers_ShouldDisplayWeekNumberColumn()
    {
        var comp = Context.Render<VekCalendar>(parameters => parameters
            .Add(p => p.PickerMonth, new DateTime(2026, 5, 1))
            .Add(p => p.ShowWeekNumbers, true));

        var weekElements = comp.FindAll(".vek-picker-calendar-week-text");
        weekElements.Should().NotBeEmpty();
        weekElements.Any(w => !string.IsNullOrWhiteSpace(w.TextContent)).Should().BeTrue();
    }

    [Test]
    public void Navigation_PreviousAndNextMonthButtons()
    {
        var comp = Context.Render<VekCalendar>(parameters => parameters
            .Add(p => p.Culture, CultureInfo.InvariantCulture)
            .Add(p => p.PickerMonth, new DateTime(2026, 5, 1)));

        var monthBtn = comp.Find("button.vek-button-month");
        monthBtn.TextContent.Should().Contain("May");
        monthBtn.TextContent.Should().Contain("2026");

        // Next month
        var nextButton = comp.Find(".vek-picker-nav-button-next");
        nextButton.Click();
        monthBtn = comp.Find("button.vek-button-month");
        monthBtn.TextContent.Should().Contain("June");
        monthBtn.TextContent.Should().Contain("2026");

        // Previous month twice
        var prevButton = comp.Find(".vek-picker-nav-button-prev");
        prevButton.Click();
        prevButton.Click();
        monthBtn = comp.Find("button.vek-button-month");
        monthBtn.TextContent.Should().Contain("April");
        monthBtn.TextContent.Should().Contain("2026");
    }

    [Test]
    public void HeaderNavigation_SwitchToMonthAndYearViews()
    {
        var comp = Context.Render<VekCalendar>(parameters => parameters
            .Add(p => p.Culture, CultureInfo.InvariantCulture)
            .Add(p => p.PickerMonth, new DateTime(2026, 5, 1)));

        // Click month title to open month view
        comp.Find("button.vek-button-month").Click();
        comp.FindAll(".vek-picker-month").Count.Should().Be(12);

        // Click September
        var sepButton = comp.FindAll(".vek-picker-month")
            .First(b => b.TextContent.Trim().StartsWith("Sep"));
        sepButton.Click();

        // Should return to date view showing September 2026
        var monthBtn = comp.Find("button.vek-button-month");
        monthBtn.TextContent.Should().Contain("September");
        monthBtn.TextContent.Should().Contain("2026");

        // Click year in toolbar to switch to year view
        comp.Find(".vek-button-year").Click();
        comp.Find(".vek-picker-year-container").Should().NotBeNull();

        // Click year 2028
        var year2028 = comp.FindAll(".vek-picker-year")
            .First(y => y.TextContent.Trim() == "2028");
        year2028.Click();

        // Should return to date view for 2028
        comp.Find("button.vek-button-month").TextContent.Should().Contain("2028");
    }

    [Test]
    public void MinMaxDate_ShouldDisableOutOfRangeDays()
    {
        var minDate = new DateTime(2026, 5, 10);
        var maxDate = new DateTime(2026, 5, 20);

        var comp = Context.Render<VekCalendar>(parameters => parameters
            .Add(p => p.PickerMonth, new DateTime(2026, 5, 1))
            .Add(p => p.MinDate, minDate)
            .Add(p => p.MaxDate, maxDate));

        var day5 = comp.FindAll("button.vek-picker-calendar-day")
            .First(b => b.TextContent.Trim() == "5");
        day5.Attributes.GetNamedItem("disabled").Should().NotBeNull();
        day5.ClassList.Should().Contain("vek-disabled");

        var day15 = comp.FindAll("button.vek-picker-calendar-day")
            .First(b => b.TextContent.Trim() == "15");
        day15.Attributes.GetNamedItem("disabled").Should().BeNull();
        day15.ClassList.Should().NotContain("vek-disabled");

        var day25 = comp.FindAll("button.vek-picker-calendar-day")
            .First(b => b.TextContent.Trim() == "25");
        day25.Attributes.GetNamedItem("disabled").Should().NotBeNull();
        day25.ClassList.Should().Contain("vek-disabled");
    }

    [Test]
    public void DisabledState_ShouldPreventAllInteractions()
    {
        DateTime? selectedDate = null;
        var comp = Context.Render<VekCalendar>(parameters => parameters
            .Add(p => p.Disabled, true)
            .Add(p => p.PickerMonth, new DateTime(2026, 5, 1))
            .Add(p => p.DateChanged, EventCallback.Factory.Create<DateTime?>(this, d => selectedDate = d)));

        comp.Find(".vek-calendar").ClassList.Should().Contain("vek-calendar-disabled");

        var day15 = comp.FindAll("button.vek-picker-calendar-day")
            .First(b => b.TextContent.Trim() == "15");
        day15.Attributes.GetNamedItem("disabled").Should().NotBeNull();
        day15.Click();

        selectedDate.Should().BeNull();
    }

    [Test]
    public void KeyboardNavigation_ArrowKeysAndEnterSelection()
    {
        DateTime? selectedDate = null;
        var comp = Context.Render<VekCalendar>(parameters => parameters
            .Add(p => p.Date, new DateTime(2026, 5, 15))
            .Add(p => p.DateChanged, EventCallback.Factory.Create<DateTime?>(this, d => selectedDate = d)));

        var calendarRoot = comp.Find(".vek-calendar");

        // ArrowRight moves focus to 16th
        calendarRoot.KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });

        // Enter selects the focused date
        calendarRoot.KeyDown(new KeyboardEventArgs { Key = "Enter" });

        selectedDate.Should().Be(new DateTime(2026, 5, 16));
    }

    [Test]
    public void CustomDayCellTemplate_ShouldRenderCustomMarkup()
    {
        var comp = Context.Render<VekCalendar>(parameters => parameters
            .Add(p => p.PickerMonth, new DateTime(2026, 5, 1))
            .Add(p => p.DayCellTemplate, day => builder =>
            {
                builder.OpenElement(0, "span");
                builder.AddAttribute(1, "class", "custom-day-cell");
                builder.AddContent(2, $"D-{day.Day}");
                builder.CloseElement();
            }));

        var customCells = comp.FindAll(".custom-day-cell");
        customCells.Should().NotBeEmpty();
        customCells.First().TextContent.Should().StartWith("D-");
    }

    [Test]
    public void Appearance_OutlinedAndElevation()
    {
        var comp = Context.Render<VekCalendar>(parameters => parameters
            .Add(p => p.Outlined, true)
            .Add(p => p.Elevation, 4)
            .Add(p => p.Square, true));

        var root = comp.Find(".vek-calendar");
        root.ClassList.Should().Contain("vek-paper-outlined");
        root.ClassList.Should().NotContain("vek-rounded");
    }

    [Test]
    public void GoToToday_ShouldSelectCurrentDate()
    {
        DateTime? selected = null;
        var comp = Context.Render<VekCalendar>(parameters => parameters
            .Add(p => p.ShowTodayButton, true)
            .Add(p => p.DateChanged, EventCallback.Factory.Create<DateTime?>(this, d => selected = d)));

        var todayButton = comp.Find(".vek-calendar-actions button");
        todayButton.Click();

        selected.Should().NotBeNull();
        selected!.Value.Date.Should().Be(DateTime.Today);
    }
}
