using Bunit;

namespace Vek.Components.Tests.DataGrid;

public sealed class SortingTests : BunitContext
{
    /// <summary>
    /// Verifies that clicking the column heading toggles its sort direction.
    /// </summary>
    [Fact]
    public void ClickingNameHeaderTogglesAscendingAndDescendingOrder()
    {
        var cut = DataGridTestHelpers.RenderGrid(this, pageSize: 3);

        cut.Find("thead button").Click();
        cut.WaitForAssertion(() =>
            Assert.Equal(
                new[] { "Desk Lamp", "Notebook Set", "Task Chair" },
                DataGridTestHelpers.GetRowNames(cut)));

        cut.Find("thead button").Click();
        cut.WaitForAssertion(() =>
            Assert.Equal(
                new[] { "Task Chair", "Notebook Set", "Desk Lamp" },
                DataGridTestHelpers.GetRowNames(cut)));
    }
}
