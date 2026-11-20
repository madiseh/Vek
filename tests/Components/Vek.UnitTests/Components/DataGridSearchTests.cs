// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Linq.Expressions;
using AngleSharp.Dom;
using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using NUnit.Framework;

namespace Vek.UnitTests.Components;

#nullable enable

[TestFixture]
public class DataGridSearchTests : BunitTest
{
    public record Person(string Name, string Role, string Department);

    private static readonly Person[] SamplePeople =
    [
        new("Alice Johnson", "Engineer", "R&D"),
        new("Bob Smith", "Designer", "Product"),
        new("Charlie Brown", "Manager", "R&D"),
        new("David Lee", "Engineer", "Operations"),
    ];

    private static void RenderDefaultColumns(RenderTreeBuilder builder)
    {
        builder.OpenComponent<PropertyColumn<Person, string>>(0);
        builder.AddAttribute(1, nameof(PropertyColumn<Person, string>.Property), (Expression<Func<Person, string>>)(x => x.Name));
        builder.CloseComponent();

        builder.OpenComponent<PropertyColumn<Person, string>>(2);
        builder.AddAttribute(3, nameof(PropertyColumn<Person, string>.Property), (Expression<Func<Person, string>>)(x => x.Role));
        builder.CloseComponent();

        builder.OpenComponent<PropertyColumn<Person, string>>(4);
        builder.AddAttribute(5, nameof(PropertyColumn<Person, string>.Property), (Expression<Func<Person, string>>)(x => x.Department));
        builder.CloseComponent();
    }

    [Test]
    public void DataGrid_ShowSearchBox_FalseByDefault_DoesNotRenderSearchInput()
    {
        var comp = Context.Render<VekDataGrid<Person>>(parameters => parameters
            .Add(p => p.Items, SamplePeople)
            .Add(p => p.Columns, RenderDefaultColumns));

        comp.FindAll(".vek-data-grid-search-input").Should().BeEmpty();
    }

    [Test]
    public void DataGrid_ShowSearchBox_True_RendersSearchInputWithDefaultPlaceholder()
    {
        var comp = Context.Render<VekDataGrid<Person>>(parameters => parameters
            .Add(p => p.Items, SamplePeople)
            .Add(p => p.ShowSearchBox, true)
            .Add(p => p.Columns, RenderDefaultColumns));

        var input = comp.Find(".vek-data-grid-search-input input");
        input.Should().NotBeNull();
        input.GetAttribute("placeholder").Should().Be("Search...");
    }

    [Test]
    public void DataGrid_SearchBoxPlaceholder_Custom_RendersCustomPlaceholder()
    {
        var comp = Context.Render<VekDataGrid<Person>>(parameters => parameters
            .Add(p => p.Items, SamplePeople)
            .Add(p => p.ShowSearchBox, true)
            .Add(p => p.SearchBoxPlaceholder, "Filter people...")
            .Add(p => p.Columns, RenderDefaultColumns));

        var input = comp.Find(".vek-data-grid-search-input input");
        input.GetAttribute("placeholder").Should().Be("Filter people...");
    }

    [Test]
    public async Task DataGrid_SetSearchTextAsync_FiltersRows()
    {
        var comp = Context.Render<VekDataGrid<Person>>(parameters => parameters
            .Add(p => p.Items, SamplePeople)
            .Add(p => p.ShowSearchBox, true)
            .Add(p => p.Columns, RenderDefaultColumns));

        // Initial rows: 4 people
        comp.Instance.FilteredItems.Should().HaveCount(4);

        // Search for "Engineer"
        await comp.InvokeAsync(() => comp.Instance.SetSearchTextAsync("Engineer"));

        comp.Instance.FilteredItems.Should().HaveCount(2);
        comp.Instance.FilteredItems.Select(p => p.Name).Should().BeEquivalentTo(["Alice Johnson", "David Lee"]);

        // Clear search
        await comp.InvokeAsync(() => comp.Instance.SetSearchTextAsync(null));

        comp.Instance.FilteredItems.Should().HaveCount(4);
    }

    [Test]
    public async Task DataGrid_SearchTextChanged_EventCallbackFires()
    {
        string? notifiedText = null;

        var comp = Context.Render<VekDataGrid<Person>>(parameters => parameters
            .Add(p => p.Items, SamplePeople)
            .Add(p => p.ShowSearchBox, true)
            .Add(p => p.SearchTextChanged, EventCallback.Factory.Create<string?>(this, text => notifiedText = text))
            .Add(p => p.Columns, RenderDefaultColumns));

        await comp.InvokeAsync(() => comp.Instance.SetSearchTextAsync("Operations"));

        notifiedText.Should().Be("Operations");
        comp.Instance.FilteredItems.Should().HaveCount(1);
        comp.Instance.FilteredItems.First().Name.Should().Be("David Lee");
    }

    [Test]
    public async Task DataGrid_Column_AllowSearch_False_ExcludesColumnFromSearch()
    {
        void RenderColumnsWithExclusion(RenderTreeBuilder builder)
        {
            builder.OpenComponent<PropertyColumn<Person, string>>(0);
            builder.AddAttribute(1, nameof(PropertyColumn<Person, string>.Property), (Expression<Func<Person, string>>)(x => x.Name));
            builder.CloseComponent();

            builder.OpenComponent<PropertyColumn<Person, string>>(2);
            builder.AddAttribute(3, nameof(PropertyColumn<Person, string>.Property), (Expression<Func<Person, string>>)(x => x.Role));
            builder.CloseComponent();

            builder.OpenComponent<PropertyColumn<Person, string>>(4);
            builder.AddAttribute(5, nameof(PropertyColumn<Person, string>.Property), (Expression<Func<Person, string>>)(x => x.Department));
            builder.AddAttribute(6, nameof(PropertyColumn<Person, string>.AllowSearch), false);
            builder.CloseComponent();
        }

        var comp = Context.Render<VekDataGrid<Person>>(parameters => parameters
            .Add(p => p.Items, SamplePeople)
            .Add(p => p.ShowSearchBox, true)
            .Add(p => p.Columns, (RenderFragment)RenderColumnsWithExclusion));

        // Searching for "R&D" should yield 0 rows because Department column has AllowSearch = false
        await comp.InvokeAsync(() => comp.Instance.SetSearchTextAsync("R&D"));
        comp.Instance.FilteredItems.Should().BeEmpty();

        // Searching for "Alice" should yield Alice because Name has AllowSearch = true (default)
        await comp.InvokeAsync(() => comp.Instance.SetSearchTextAsync("Alice"));
        comp.Instance.FilteredItems.Should().HaveCount(1);
        comp.Instance.FilteredItems.First().Name.Should().Be("Alice Johnson");
    }

    [Test]
    public async Task DataGrid_CustomSearchBoxTemplate_RendersAndInteractsCorrectly()
    {
        DataGridSearchBoxContext<Person>? capturedContext = null;

        RenderFragment<DataGridSearchBoxContext<Person>> template = ctx => builder =>
        {
            capturedContext = ctx;
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "custom-search-container");
            builder.AddContent(2, $"Search is: {ctx.SearchText ?? "empty"}");
            builder.CloseElement();
        };

        var comp = Context.Render<VekDataGrid<Person>>(parameters => parameters
            .Add(p => p.Items, SamplePeople)
            .Add(p => p.SearchBoxTemplate, template)
            .Add(p => p.Columns, RenderDefaultColumns));

        comp.Find(".custom-search-container").TextContent.Should().Be("Search is: empty");
        capturedContext.Should().NotBeNull();

        // Update via context
        await comp.InvokeAsync(() => capturedContext!.UpdateSearchTextAsync("Bob"));
        comp.Instance.FilteredItems.Should().HaveCount(1);
        comp.Instance.FilteredItems.First().Name.Should().Be("Bob Smith");
        capturedContext!.SearchText.Should().Be("Bob");

        // Clear via context
        await comp.InvokeAsync(() => capturedContext!.ClearAsync());
        comp.Instance.FilteredItems.Should().HaveCount(4);
    }

    [Test]
    public async Task DataGrid_SearchTextParseMode_ExactMatch_RequiresExactPhrase()
    {
        var comp = Context.Render<VekDataGrid<Person>>(parameters => parameters
            .Add(p => p.Items, SamplePeople)
            .Add(p => p.SearchTextParseMode, DataGridSearchTextParseMode.ExactMatch)
            .Add(p => p.Columns, RenderDefaultColumns));

        // "Alice Johnson" exact match
        await comp.InvokeAsync(() => comp.Instance.SetSearchTextAsync("Alice Johnson"));
        comp.Instance.FilteredItems.Should().HaveCount(1);

        // "Johnson Alice" with ExactMatch does NOT match
        await comp.InvokeAsync(() => comp.Instance.SetSearchTextAsync("Johnson Alice"));
        comp.Instance.FilteredItems.Should().BeEmpty();
    }

    [Test]
    public async Task DataGrid_SearchCombinedWithQuickFilter_AppliesBoth()
    {
        var comp = Context.Render<VekDataGrid<Person>>(parameters => parameters
            .Add(p => p.Items, SamplePeople)
            .Add(p => p.QuickFilter, (Person p) => p.Department == "R&D")
            .Add(p => p.Columns, RenderDefaultColumns));

        // QuickFilter alone: Alice and Charlie
        comp.Instance.FilteredItems.Should().HaveCount(2);

        // Combined with search "Manager": only Charlie
        await comp.InvokeAsync(() => comp.Instance.SetSearchTextAsync("Manager"));
        comp.Instance.FilteredItems.Should().HaveCount(1);
        comp.Instance.FilteredItems.First().Name.Should().Be("Charlie Brown");

        // Search for "Bob" (who is in Product): matches search, but rejected by QuickFilter -> 0 results
        await comp.InvokeAsync(() => comp.Instance.SetSearchTextAsync("Bob"));
        comp.Instance.FilteredItems.Should().BeEmpty();
    }
    [Test]
    public async Task DataGrid_HighlightSearchResults_True_RendersMarkAroundMatchedTokens()
    {
        var comp = Context.Render<VekDataGrid<Person>>(parameters => parameters
            .Add(p => p.Items, SamplePeople)
            .Add(p => p.HighlightSearchResults, true)
            .Add(p => p.Columns, RenderDefaultColumns));

        await comp.InvokeAsync(() => comp.Instance.SetSearchTextAsync("Alice"));

        var marks = comp.FindAll("mark.vek-table-cell-highlight");
        marks.Should().NotBeEmpty();
        marks.Should().ContainSingle(m => m.TextContent == "Alice");
        marks.First().ParentElement?.TextContent.Trim().Should().Be("Alice Johnson");
    }

    [Test]
    public async Task DataGrid_HighlightSearchResults_MultipleTokens_HighlightsAllMatches()
    {
        var comp = Context.Render<VekDataGrid<Person>>(parameters => parameters
            .Add(p => p.Items, SamplePeople)
            .Add(p => p.HighlightSearchResults, true)
            .Add(p => p.Columns, RenderDefaultColumns));

        await comp.InvokeAsync(() => comp.Instance.SetSearchTextAsync("Alice Engineer"));

        var marks = comp.FindAll("mark.vek-table-cell-highlight");
        marks.Should().HaveCount(2);
        marks.Select(m => m.TextContent).Should().BeEquivalentTo(["Alice", "Engineer"]);
    }

    [Test]
    public async Task DataGrid_Column_AllowSearch_False_DoesNotHighlightCellValues()
    {
        void RenderColumns(RenderTreeBuilder builder)
        {
            builder.OpenComponent<PropertyColumn<Person, string>>(0);
            builder.AddAttribute(1, nameof(PropertyColumn<Person, string>.Property), (Expression<Func<Person, string>>)(x => x.Name));
            builder.CloseComponent();

            builder.OpenComponent<PropertyColumn<Person, string>>(2);
            builder.AddAttribute(3, nameof(PropertyColumn<Person, string>.Property), (Expression<Func<Person, string>>)(x => x.Role));
            builder.AddAttribute(4, nameof(PropertyColumn<Person, string>.AllowSearch), false);
            builder.CloseComponent();

            builder.OpenComponent<PropertyColumn<Person, string>>(5);
            builder.AddAttribute(6, nameof(PropertyColumn<Person, string>.Property), (Expression<Func<Person, string>>)(x => x.Department));
            builder.CloseComponent();
        }

        var customPeople = new Person[]
        {
            new("Alice Engineer", "Engineer", "R&D")
        };

        var comp = Context.Render<VekDataGrid<Person>>(parameters => parameters
            .Add(p => p.Items, customPeople)
            .Add(p => p.HighlightSearchResults, true)
            .Add(p => p.Columns, (RenderFragment)RenderColumns));

        await comp.InvokeAsync(() => comp.Instance.SetSearchTextAsync("Engineer"));

        // Name column has AllowSearch = true, so "Engineer" in Name is highlighted.
        // Role column has AllowSearch = false, so "Engineer" in Role is NOT highlighted.
        var marks = comp.FindAll("mark.vek-table-cell-highlight");
        marks.Should().HaveCount(1);
        marks.First().ParentElement?.TextContent.Trim().Should().Be("Alice Engineer");
    }

    [Test]
    public async Task DataGrid_HighlightSearchResults_False_DoesNotRenderMarks()
    {
        var comp = Context.Render<VekDataGrid<Person>>(parameters => parameters
            .Add(p => p.Items, SamplePeople)
            .Add(p => p.HighlightSearchResults, false)
            .Add(p => p.Columns, RenderDefaultColumns));

        await comp.InvokeAsync(() => comp.Instance.SetSearchTextAsync("Alice"));

        comp.Instance.FilteredItems.Should().HaveCount(1);
        comp.FindAll("mark").Should().BeEmpty();
        comp.FindAll(".vek-table-cell-highlight").Should().BeEmpty();
    }

    [Test]
    public async Task DataGrid_SetSearchTextAsync_ClearingSearchText_RestoresAllRows()
    {
        var comp = Context.Render<VekDataGrid<Person>>(parameters => parameters
            .Add(p => p.Items, SamplePeople)
            .Add(p => p.Columns, RenderDefaultColumns));

        comp.Instance.FilteredItems.Should().HaveCount(4);

        await comp.InvokeAsync(() => comp.Instance.SetSearchTextAsync("Alice"));
        comp.Instance.FilteredItems.Should().HaveCount(1);

        await comp.InvokeAsync(() => comp.Instance.SetSearchTextAsync(string.Empty));
        comp.Instance.FilteredItems.Should().HaveCount(4);
    }

    [Test]
    public async Task DataGrid_SearchTextParseMode_GroupWordsByOr_MatchesAnyWord()
    {
        var comp = Context.Render<VekDataGrid<Person>>(parameters => parameters
            .Add(p => p.Items, SamplePeople)
            .Add(p => p.SearchTextParseMode, DataGridSearchTextParseMode.GroupWordsByOr)
            .Add(p => p.Columns, RenderDefaultColumns));

        // "Alice Bob" in GroupWordsByOr should match both Alice and Bob rows
        await comp.InvokeAsync(() => comp.Instance.SetSearchTextAsync("Alice Bob"));

        comp.Instance.FilteredItems.Should().HaveCount(2);
        comp.Instance.FilteredItems.Select(p => p.Name).Should().BeEquivalentTo(["Alice Johnson", "Bob Smith"]);
    }

    [Test]
    public async Task DataGrid_SearchTextParseMode_ExactMatch_MatchesFullPhraseOnly()
    {
        var comp = Context.Render<VekDataGrid<Person>>(parameters => parameters
            .Add(p => p.Items, SamplePeople)
            .Add(p => p.SearchTextParseMode, DataGridSearchTextParseMode.ExactMatch)
            .Add(p => p.Columns, RenderDefaultColumns));

        // "Alice Johnson" exact phrase matches
        await comp.InvokeAsync(() => comp.Instance.SetSearchTextAsync("Alice Johnson"));
        comp.Instance.FilteredItems.Should().HaveCount(1);
        comp.Instance.FilteredItems.First().Name.Should().Be("Alice Johnson");

        // Words across columns ("Alice Engineer") should NOT match under ExactMatch
        await comp.InvokeAsync(() => comp.Instance.SetSearchTextAsync("Alice Engineer"));
        comp.Instance.FilteredItems.Should().BeEmpty();
    }

    [Test]
    public async Task DataGrid_SearchTextParseMode_DynamicChange_RefiltersRows()
    {
        var comp = Context.Render<VekDataGrid<Person>>(parameters => parameters
            .Add(p => p.Items, SamplePeople)
            .Add(p => p.SearchText, "Alice Bob")
            .Add(p => p.SearchTextParseMode, DataGridSearchTextParseMode.GroupWordsByAnd)
            .Add(p => p.Columns, RenderDefaultColumns));

        // Under GroupWordsByAnd, no single person row contains both "Alice" and "Bob"
        comp.Instance.FilteredItems.Should().BeEmpty();

        // Switch to GroupWordsByOr dynamically
        await comp.SetParametersAndRenderAsync(parameters => parameters
            .Add(p => p.SearchTextParseMode, DataGridSearchTextParseMode.GroupWordsByOr));

        comp.Instance.FilteredItems.Should().HaveCount(2);
        comp.Instance.FilteredItems.Select(p => p.Name).Should().BeEquivalentTo(["Alice Johnson", "Bob Smith"]);
    }

    [Test]
    public async Task DataGrid_HighlightSearchResults_DynamicToggle_TogglesMarks()
    {
        var comp = Context.Render<VekDataGrid<Person>>(parameters => parameters
            .Add(p => p.Items, SamplePeople)
            .Add(p => p.SearchText, "Alice")
            .Add(p => p.HighlightSearchResults, true)
            .Add(p => p.Columns, RenderDefaultColumns));

        comp.FindAll("mark.vek-table-cell-highlight").Should().NotBeEmpty();

        // Toggle highlighting off
        await comp.SetParametersAndRenderAsync(parameters => parameters
            .Add(p => p.HighlightSearchResults, false));

        comp.FindAll("mark.vek-table-cell-highlight").Should().BeEmpty();

        // Toggle highlighting back on
        await comp.SetParametersAndRenderAsync(parameters => parameters
            .Add(p => p.HighlightSearchResults, true));

        comp.FindAll("mark.vek-table-cell-highlight").Should().NotBeEmpty();
    }

    [Test]
    public async Task DataGrid_SearchBoxContext_ClearAsync_ClearsSearchTextAndRestoresRows()
    {
        DataGridSearchBoxContext<Person>? context = null;

        var comp = Context.Render<VekDataGrid<Person>>(parameters => parameters
            .Add(p => p.Items, SamplePeople)
            .Add(p => p.ShowSearchBox, true)
            .Add(p => p.SearchText, "Alice")
            .Add(p => p.SearchBoxTemplate, ctx => builder =>
            {
                context = ctx;
                builder.AddContent(0, "custom search");
            })
            .Add(p => p.Columns, RenderDefaultColumns));

        comp.Instance.FilteredItems.Should().HaveCount(1);
        context.Should().NotBeNull();
        context!.SearchText.Should().Be("Alice");

        await comp.InvokeAsync(() => context.ClearAsync());

        comp.Instance.FilteredItems.Should().HaveCount(4);
        context.SearchText.Should().BeEmpty();
    }
}
