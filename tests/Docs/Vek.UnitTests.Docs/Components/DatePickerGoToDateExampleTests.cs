using AwesomeAssertions;
using Bunit;
using NUnit.Framework;
using Vek.Docs.Examples;
using Vek.UnitTests.Shared.Extensions;

namespace Vek.UnitTests.Docs.Components;

[TestFixture]
public sealed class DatePickerGoToDateExampleTests : BunitTest
{
    /// <summary>
    /// Returns to today's date after navigating to the Vek release date.
    /// </summary>
    [Test]
    public async Task MoveToCurrentDate_ReturnsToTodayAfterVekReleaseDate()
    {
        var today = DateTime.Today;
        var timeProvider = Context.AddFakeTimeProvider();
        using var comp = Context.Render<DatePickerGoToDateExample>();

        await comp.FindAll("button").Single(button => button.TextContent.Trim() == "Move To Current Date").ClickAsync();
        comp.WaitForAssertion(() => comp.FindComponent<VekDatePicker>().Instance.Date.Should().Be(today));

        timeProvider.Advance(TimeSpan.FromSeconds(1));
        await comp.FindAll("button").Single(button => button.TextContent.Trim() == "When First Vek Released?").ClickAsync();
        comp.WaitForAssertion(() => comp.FindComponent<VekDatePicker>().Instance.Date.Should().Be(new DateTime(2020, 10, 18)));

        timeProvider.Advance(TimeSpan.FromSeconds(1));
        await comp.FindAll("button").Single(button => button.TextContent.Trim() == "Move To Current Date").ClickAsync();

        comp.WaitForAssertion(() =>
        {
            var datePicker = comp.FindComponent<VekDatePicker>().Instance;
            datePicker.PickerMonth.Should().Be(new DateTime(today.Year, today.Month, 1));
            datePicker.Date.Should().Be(today);
        });
    }
}
