using Bunit;
using Microsoft.AspNetCore.Components;
using System.Collections.ObjectModel;
using Vek.Blazor.DataGrid.Components.DataGrid;
using Vek.Components.Components.DataGrid;

namespace Vek.Components.Tests.DataGrid;

public sealed class ObservabilityTests : BunitContext
{
    /// <summary>Verifies observable item changes refresh rows and replacing the source detaches the old collection.</summary>
    [Fact]
    public void ObservableCollectionChangesRefreshRows()
    {
        var original = new ObservableCollection<TestProduct>
        {
            new("Desk Lamp", 49.99m, "Lighting")
        };
        var selectionChanges = new List<IReadOnlyList<TestProduct>>();
        var cut = Render<VekDataGrid<TestProduct>>(parameters => parameters
            .Add(grid => grid.Items, original)
            .Add(grid => grid.PageSize, 10)
            .Add(grid => grid.SelectionMode, VekDataGridSelectionMode.Multiple)
            .Add(grid => grid.SelectedItems, [original[0]])
            .Add(grid => grid.SelectedItemsChanged, EventCallback.Factory.Create<IReadOnlyList<TestProduct>>(this, selectionChanges.Add))
            .AddChildContent(NameColumn()));

        original.Add(new TestProduct("Task Chair", 199.00m, "Furniture"));
        cut.WaitForAssertion(() => Assert.Contains("Task Chair", cut.Find("tbody").TextContent));
        original.RemoveAt(0);
        cut.WaitForAssertion(() => Assert.Empty(cut.Instance.SelectedItems));
        Assert.Single(selectionChanges);

        var replacement = new ObservableCollection<TestProduct>
        {
            new("Notebook Set", 12.90m, "Stationery")
        };
        cut.Render(parameters => parameters.Add(grid => grid.Items, replacement));
        original.Add(new TestProduct("Removed Source", 1m, "Old"));
        replacement.Add(new TestProduct("Pencil Case", 8m, "Stationery"));

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Pencil Case", cut.Find("tbody").TextContent);
            Assert.DoesNotContain("Removed Source", cut.Find("tbody").TextContent);
        });

        cut.Dispose();
        replacement.Add(new TestProduct("After disposal", 3m, "Old"));
    }

    private static RenderFragment NameColumn() => builder =>
    {
        builder.OpenComponent<PropertyColumn<TestProduct, string>>(0);
        builder.AddAttribute(1, nameof(PropertyColumn<TestProduct, string>.Property),
            (System.Linq.Expressions.Expression<Func<TestProduct, string>>)(product => product.Name));
        builder.AddAttribute(2, nameof(PropertyColumn<TestProduct, string>.Title), "Name");
        builder.CloseComponent();
    };
}
