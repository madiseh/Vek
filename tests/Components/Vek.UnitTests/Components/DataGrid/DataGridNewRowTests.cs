// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using NUnit.Framework;
using Vek.UnitTests;

namespace Vek.UnitTests.Components.DataGrid;

#nullable enable

[TestFixture]
public class DataGridNewRowTests : BunitTest
{
    public sealed class Item
    {
        public string Name { get; set; } = string.Empty;
    }

    private static RenderFragment Columns => builder =>
    {
        builder.OpenComponent<PropertyColumn<Item, string>>(0);
        builder.AddAttribute(1, nameof(PropertyColumn<Item, string>.Property), (Expression<Func<Item, string>>)(item => item.Name));
        builder.AddAttribute(2, nameof(PropertyColumn<Item, string>.Required), true);
        builder.CloseComponent();
    };

    private IRenderedComponent<VekDataGrid<Item>> RenderGrid(Action<ComponentParameterCollectionBuilder<VekDataGrid<Item>>>? configure = null, string newItemName = "New")
    {
        return Context.Render<VekDataGrid<Item>>(parameters =>
        {
            parameters
                .Add(item => item.Items, new[] { new Item { Name = "Existing" } })
                .Add(item => item.ReadOnly, false)
                .Add(item => item.Columns, Columns)
                .Add(item => item.NewItemFactory, () => new Item { Name = newItemName });
            configure?.Invoke(parameters);
        });
    }

    /// <summary>Starting a row renders editors before the data rows by default.</summary>
    [Test]
    public async Task StartNewRow_RendersAtTopByDefault()
    {
        var grid = RenderGrid();

        await grid.InvokeAsync(grid.Instance.StartNewRowAsync);

        grid.FindAll("tbody tr.vek-data-grid-new-row").Should().HaveCount(1);
        grid.Find("tbody tr.vek-data-grid-new-row").QuerySelector("input").Should().NotBeNull();
        grid.FindAll("tbody tr")[0].ClassList.Should().Contain("vek-data-grid-new-row");
    }

    /// <summary>Bottom insertion places the active new row after existing data rows.</summary>
    [Test]
    public async Task StartNewRow_BottomPositionRendersAfterDataRows()
    {
        var grid = RenderGrid(parameters => parameters.Add(item => item.NewRowPosition, NewRowPosition.Bottom));

        await grid.InvokeAsync(grid.Instance.StartNewRowAsync);

        grid.FindAll("tbody tr")[^1].ClassList.Should().Contain("vek-data-grid-new-row");
    }

    /// <summary>Creating invokes the callbacks with the factory item and closes the row.</summary>
    [Test]
    public async Task CommitNewRow_InvokesLifecycleCallbacks()
    {
        Item? creating = null;
        Item? created = null;
        var grid = RenderGrid(parameters => parameters
            .Add(item => item.OnRowCreating, value => creating = value)
            .Add(item => item.OnRowCreated, value => created = value));

        await grid.InvokeAsync(grid.Instance.StartNewRowAsync);
        await grid.InvokeAsync(grid.Instance.CommitNewRowAsync);

        creating.Should().NotBeNull();
        created.Should().BeSameAs(creating);
        grid.FindAll("tbody tr.vek-data-grid-new-row").Should().BeEmpty();
    }

    /// <summary>Canceling a new row discards it without invoking the create callback.</summary>
    [Test]
    public async Task CancelNewRow_DiscardsWithoutCreating()
    {
        var created = false;
        var grid = RenderGrid(parameters => parameters.Add(item => item.OnRowCreated, _ => created = true));

        await grid.InvokeAsync(grid.Instance.StartNewRowAsync);
        await grid.InvokeAsync(grid.Instance.CancelNewRowAsync);

        created.Should().BeFalse();
        grid.FindAll("tbody tr.vek-data-grid-new-row").Should().BeEmpty();
    }

    /// <summary>Required field errors keep the new row open and skip the create callback.</summary>
    [Test]
    public async Task CommitNewRow_ValidationFailureKeepsRowOpen()
    {
        var created = false;
        var grid = RenderGrid(newItemName: string.Empty, configure: parameters => parameters.Add(item => item.OnRowCreated, _ => created = true));

        await grid.InvokeAsync(grid.Instance.StartNewRowAsync);
        await grid.InvokeAsync(grid.Instance.CommitNewRowAsync);

        created.Should().BeFalse();
        grid.FindAll("tbody tr.vek-data-grid-new-row").Should().HaveCount(1);
    }
}
