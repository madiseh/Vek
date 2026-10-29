using Bunit;

namespace Vek.Components.Tests.DataGrid;

public sealed class PaginationTests : BunitContext
{
    /// <summary>
    /// Verifies that the pager navigates forward and backward between result pages.
    /// </summary>
    [Fact]
    public void NextAndPreviousButtonsNavigateBetweenPages()
    {
        var cut = DataGridTestHelpers.RenderGrid(this, pageSize: 2);

        cut.WaitForAssertion(() =>
            Assert.Equal(
                new[] { "Task Chair", "Desk Lamp" },
                DataGridTestHelpers.GetRowNames(cut)));

        cut.Find(".pager button:last-child").Click();
        cut.WaitForAssertion(() =>
        {
            Assert.Equal(
                new[] { "Notebook Set" },
                DataGridTestHelpers.GetRowNames(cut));
            Assert.Contains("Page 2 of 2", cut.Markup);
        });

        cut.Find(".pager button:first-child").Click();
        cut.WaitForAssertion(() =>
        {
            Assert.Equal(
                new[] { "Task Chair", "Desk Lamp" },
                DataGridTestHelpers.GetRowNames(cut));
            Assert.Contains("Page 1 of 2", cut.Markup);
        });
    }
}
