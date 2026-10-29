using Bunit;
using Microsoft.AspNetCore.Components.Web;
using System.Reflection;
using System.Text;
using Vek.Components.Components.CodeView;

namespace Vek.Components.Tests.CodeView;

public sealed class VekCodeViewMultiTests : BunitContext
{
    /// <summary>Verifies the tab strip scrolls horizontally without creating a vertical scrollbar.</summary>
    [Fact]
    public void TabStripAllowsHorizontalScrollingWithoutVerticalScrolling()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
            "Vek.Components.Tests.CodeView.VekCodeViewMulti.razor.css");
        Assert.NotNull(stream);
        using var reader = new StreamReader(stream!, Encoding.UTF8);
        var styles = reader.ReadToEnd();

        Assert.Contains("overflow-x: auto;", styles);
        Assert.Contains("overflow-y: hidden;", styles);
    }

    /// <summary>Verifies the first tab is selected and its sample is rendered initially.</summary>
    [Fact]
    public void FirstTabIsSelectedAndRendersItsCode()
    {
        var cut = RenderViewer(CreateTabs());

        Assert.Equal("true", cut.FindAll("[role='tab']")[0].GetAttribute("aria-selected"));
        Assert.Equal("false", cut.FindAll("[role='tab']")[1].GetAttribute("aria-selected"));
        Assert.Contains("HTML sample", cut.Find(".vek-code-view pre code").TextContent);
    }

    /// <summary>Verifies choosing a tab updates the selected tab and displayed sample.</summary>
    [Fact]
    public void ClickingTabShowsItsCodeAndUpdatesSelection()
    {
        var cut = RenderViewer(CreateTabs());

        cut.FindAll("[role='tab']")[1].Click();

        Assert.Equal("false", cut.FindAll("[role='tab']")[0].GetAttribute("aria-selected"));
        Assert.Equal("true", cut.FindAll("[role='tab']")[1].GetAttribute("aria-selected"));
        Assert.Contains("SELECT name", cut.Find(".vek-code-view pre code").TextContent);
    }

    /// <summary>Verifies arrow and boundary keys change selection without leaving the available tabs.</summary>
    [Fact]
    public void KeyboardNavigationSelectsTabsWithinBounds()
    {
        var cut = RenderViewer(CreateTabs());
        var tabs = cut.FindAll("[role='tab']");

        tabs[0].KeyDown(new KeyboardEventArgs { Key = "ArrowLeft" });
        Assert.Equal("true", cut.FindAll("[role='tab']")[0].GetAttribute("aria-selected"));

        cut.FindAll("[role='tab']")[0].KeyDown(new KeyboardEventArgs { Key = "End" });
        Assert.Equal("true", cut.FindAll("[role='tab']")[1].GetAttribute("aria-selected"));
        Assert.Contains("SELECT name", cut.Find(".vek-code-view pre code").TextContent);

        cut.FindAll("[role='tab']")[1].KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });
        Assert.Equal("true", cut.FindAll("[role='tab']")[1].GetAttribute("aria-selected"));

        cut.FindAll("[role='tab']")[1].KeyDown(new KeyboardEventArgs { Key = "Home" });
        Assert.Equal("true", cut.FindAll("[role='tab']")[0].GetAttribute("aria-selected"));
    }

    /// <summary>Verifies an empty tab collection renders no tab list or code panel.</summary>
    [Fact]
    public void EmptyTabsRenderNoPanel()
    {
        var cut = RenderViewer([]);

        Assert.Empty(cut.FindAll("[role='tablist']"));
        Assert.Empty(cut.FindAll(".vek-code-view"));
    }

    /// <summary>Verifies a one-tab collection renders as a usable single sample.</summary>
    [Fact]
    public void SingleTabRendersNormally()
    {
        var cut = RenderViewer([new VekCodeViewTab("One", VekCodeLanguage.CSharp, "class One {}")]);

        Assert.Equal("One", cut.Find("[role='tab']").TextContent);
        Assert.Equal("true", cut.Find("[role='tab']").GetAttribute("aria-selected"));
        Assert.Contains("class One", cut.Find(".vek-code-view pre code").TextContent);
    }

    /// <summary>Verifies titles are encoded and every tab has a unique, correctly linked panel across viewer instances.</summary>
    [Fact]
    public void TitlesAreEncodedAndInstancesHaveUniqueTabAndPanelIds()
    {
        var tabs = CreateTabs("<HTML>");
        var first = RenderViewer(tabs);
        var second = RenderViewer(tabs);
        var firstTab = first.Find("[role='tab']");
        var firstPanel = first.Find("[role='tabpanel']");
        var firstTabs = first.FindAll("[role='tab']");
        var firstPanels = first.FindAll("[role='tabpanel']");
        var secondTab = second.Find("[role='tab']");
        var secondPanel = second.Find("[role='tabpanel']");

        Assert.Equal("<HTML>", firstTab.TextContent);
        Assert.Equal(2, firstPanels.Count);
        Assert.Equal(firstPanel.Id, firstTab.GetAttribute("aria-controls"));
        Assert.Equal(firstTab.Id, firstPanel.GetAttribute("aria-labelledby"));
        for (var index = 0; index < firstTabs.Count; index++)
        {
            Assert.Equal(firstPanels[index].Id, firstTabs[index].GetAttribute("aria-controls"));
            Assert.Equal(firstTabs[index].Id, firstPanels[index].GetAttribute("aria-labelledby"));
            Assert.Equal(index != 0, firstPanels[index].HasAttribute("hidden"));
        }

        Assert.NotEqual(firstTab.Id, secondTab.Id);
        Assert.NotEqual(firstPanel.Id, secondPanel.Id);
    }

    private IRenderedComponent<VekCodeViewMulti> RenderViewer(IReadOnlyList<VekCodeViewTab> tabs) =>
        Render<VekCodeViewMulti>(parameters => parameters.Add(viewer => viewer.Tabs, tabs));

    private static IReadOnlyList<VekCodeViewTab> CreateTabs(string firstTitle = "HTML") =>
    [
        new(firstTitle, VekCodeLanguage.Html, "<p>HTML sample</p>"),
        new("SQL", VekCodeLanguage.Sql, "SELECT name FROM users;")
    ];
}
