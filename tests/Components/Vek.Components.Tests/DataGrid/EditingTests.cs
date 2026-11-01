using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Vek.Components.Components.DataGrid;

namespace Vek.Components.Tests.DataGrid;

public sealed class EditingTests : BunitContext
{
    /// <summary>Verifies a cell edit commits the updated draft through the grid callback.</summary>
    [Fact]
    public void CellEditCommitsValue()
    {
        TestProduct? committed = null;
        var cut = RenderEditingGrid(VekDataGridEditMode.Cell, item =>
        {
            committed = item;
            return Task.FromResult(true);
        });

        cut.Find("button[aria-label='Edit Name']").Click();
        cut.Find("button[data-action='change']").Click();
        cut.Find("button[data-action='commit']").Click();

        Assert.Equal("Edited", committed?.Name);
        Assert.Equal("Task Chair", DataGridTestHelpers.Products[0].Name);
    }

    /// <summary>Verifies row editing allows only one row to be edited at a time.</summary>
    [Fact]
    public void InlineEditAllowsOnlyOneRow()
    {
        var cut = RenderEditingGrid(VekDataGridEditMode.Row, _ => Task.FromResult(true));

        cut.Find("button[aria-label='Edit row Task Chair']").Click();
        cut.Find("button[aria-label='Edit row Desk Lamp']").Click();

        Assert.Contains("Draft: Task Chair", cut.Find("tbody").TextContent);
        Assert.DoesNotContain("Draft: Desk Lamp", cut.Find("tbody").TextContent);
    }

    /// <summary>Verifies a rejected form commit leaves the edit draft open for correction.</summary>
    [Fact]
    public void FormEditCanKeepOpenAfterRejectedCommit()
    {
        var cut = RenderEditingGrid(VekDataGridEditMode.Form, _ => Task.FromResult(false));

        cut.Find("button[aria-label='Edit row Task Chair']").Click();
        cut.Find("button[data-action='commit']").Click();

        Assert.Contains("Draft: Task Chair", cut.Find("tbody").TextContent);
    }

    /// <summary>Verifies cancel discards the copied draft and leaves the source item unchanged.</summary>
    [Fact]
    public void CancelRestoresOriginalItem()
    {
        var cut = RenderEditingGrid(VekDataGridEditMode.Row, _ => Task.FromResult(true));

        cut.Find("button[aria-label='Edit row Task Chair']").Click();
        cut.Find("button[data-action='change']").Click();
        cut.Find("button[data-action='cancel']").Click();

        Assert.Equal("Task Chair", DataGridTestHelpers.Products[0].Name);
        Assert.Contains("Task Chair", cut.Find("tbody").TextContent);
        Assert.DoesNotContain("Draft:", cut.Find("tbody").TextContent);
    }

    /// <summary>Verifies validation messages prevent the edit commit callback from running.</summary>
    [Fact]
    public void ValidationFailurePreventsCommit()
    {
        var committed = false;
        var cut = RenderEditingGrid(VekDataGridEditMode.Cell, _ =>
        {
            committed = true;
            return Task.FromResult(true);
        }, context =>
        {
            var messages = new ValidationMessageStore(context.EditContext);
            messages.Add(new FieldIdentifier(context.Item, nameof(TestProduct.Name)), "Name is required.");
            context.EditContext.NotifyValidationStateChanged();
        });

        cut.Find("button[aria-label='Edit Name']").Click();
        cut.Find("button[data-action='invalidate']").Click();
        cut.Find("button[data-action='commit']").Click();

        Assert.False(committed);
        Assert.Contains("Draft: Task Chair", cut.Find("tbody").TextContent);
    }

    private IRenderedComponent<VekDataGrid<TestProduct>> RenderEditingGrid(
        VekDataGridEditMode editMode,
        Func<TestProduct, Task<bool>> commit,
        Action<VekDataGridEditContext<TestProduct>>? invalidate = null) => Render<VekDataGrid<TestProduct>>(parameters => parameters
            .Add(grid => grid.Items, DataGridTestHelpers.Products)
            .Add(grid => grid.PageSize, 10)
            .Add(grid => grid.EditMode, editMode)
            .Add(grid => grid.CommitEdit, commit)
            .AddChildContent(builder =>
            {
                builder.OpenComponent<PropertyColumn<TestProduct, string>>(0);
                builder.AddAttribute(1, nameof(PropertyColumn<TestProduct, string>.Property), (System.Linq.Expressions.Expression<Func<TestProduct, string>>)(product => product.Name));
                builder.AddAttribute(2, nameof(PropertyColumn<TestProduct, string>.Title), "Name");
                builder.AddAttribute(3, nameof(PropertyColumn<TestProduct, string>.Editable), true);
                builder.AddAttribute(4, nameof(PropertyColumn<TestProduct, string>.EditTemplate), (RenderFragment<VekDataGridEditContext<TestProduct>>)(context => content =>
                {
                    content.OpenElement(0, "span");
                    content.AddContent(1, $"Draft: {context.Item.Name}");
                    content.CloseElement();
                    content.OpenElement(2, "button");
                    content.AddAttribute(3, "data-action", "change");
                    content.AddAttribute(4, "onclick", EventCallback.Factory.Create(this, () => context.UpdateItem(context.Item with { Name = "Edited" })));
                    content.AddContent(5, "Change value");
                    content.CloseElement();
                    content.OpenElement(6, "button");
                    content.AddAttribute(7, "data-action", "invalidate");
                    content.AddAttribute(8, "onclick", EventCallback.Factory.Create(this, () => invalidate?.Invoke(context)));
                    content.AddContent(9, "Invalidate");
                    content.CloseElement();
                    content.OpenElement(10, "button");
                    content.AddAttribute(11, "data-action", "commit");
                    content.AddAttribute(12, "onclick", EventCallback.Factory.Create(this, context.CommitAsync));
                    content.AddContent(13, "Save");
                    content.CloseElement();
                    content.OpenElement(14, "button");
                    content.AddAttribute(15, "data-action", "cancel");
                    content.AddAttribute(16, "onclick", EventCallback.Factory.Create(this, context.CancelAsync));
                    content.AddContent(17, "Cancel");
                    content.CloseElement();
                }));
                builder.CloseComponent();
            }));
}
