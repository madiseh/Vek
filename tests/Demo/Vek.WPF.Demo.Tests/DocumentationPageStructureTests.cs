namespace Vek.WPF.Demo.Tests;

public sealed class DocumentationPageStructureTests
{
    /// <summary>
    /// Documentation retains Blazor's collapsed component hierarchy, route order, and a separate Home action.
    /// </summary>
    [Fact]
    public async Task DocumentationNavigationMatchesBlazorHierarchy()
    {
        await WpfTestDispatcher.RunAsync(() =>
        {
            var model = new MainWindowViewModel();
            model.GetStartedCommand.Execute(null);
            WithWindow(new DocumentationView { DataContext = model }, window =>
            {
                var tree = Assert.Single(Descendants<TreeView>(window));
                Assert.Equal(2, tree.Items.Count);
                var gettingStarted = Assert.IsType<TreeViewItem>(tree.Items[0]);
                Assert.Equal("Getting Started", gettingStarted.Header);
                var components = Assert.IsType<TreeViewItem>(tree.Items[1]);
                Assert.Equal("Components", components.Header);
                Assert.False(components.IsExpanded);
                Assert.Equal(new[] { "VekDataGrid", "VekCodeView" }, components.Items.Cast<TreeViewItem>().Select(item => (string)item.Header));
                var links = components.Items.Cast<TreeViewItem>().SelectMany(group => group.Items.Cast<TreeViewItem>()).ToArray();
                var source = File.ReadAllText(Path.Combine(RepositoryRoot(), "src/Demo/Vek.Demo/Components/Layout/DocumentationLayout.razor"));
                var expected = Regex.Matches(source, "href=\"/docs/components/([^\"]+)\"").Select(match => match.Groups[1].Value);
                Assert.Equal(expected, links.Select(item => Assert.IsType<DemoPageDefinition>(item.Tag).Id));
                var home = Descendants<Button>(window).Single(button => (string?)button.Content == "Home");
                home.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Assert.Equal("home", model.SelectedPage.Id);
            });
            return true;
        });
    }

    /// <summary>
    /// Every rendered article keeps the source title, introduction, ordered section anchors, and local TOC.
    /// </summary>
    [Fact]
    public async Task EveryDocumentationArticleMatchesBlazorSectionOrder()
    {
        await WpfTestDispatcher.RunAsync(() =>
        {
            var sources = Directory.GetFiles(Path.Combine(RepositoryRoot(), "src/Demo/Vek.Demo/Components/Pages/Docs/Blazor"), "*.razor", SearchOption.AllDirectories);
            foreach (var page in DemoPageCatalog.All.Where(page => page.Id != "home"))
            {
                var route = page.Id == "getting-started" ? "/docs/getting-started" : "/docs/components/" + page.Id;
                var source = sources.Select(File.ReadAllText).Single(text => text.Contains("@page \"" + route + "\""));
                var title = Plain(Regex.Match(source, "<h1>(.*?)</h1>", RegexOptions.Singleline).Groups[1].Value);
                var introduction = Plain(Regex.Match(source, "<p class=\"docs-intro\">(.*?)</p>", RegexOptions.Singleline).Groups[1].Value);
                var sections = Regex.Matches(source, "<section([^>]*)>\\s*<h2[^>]*>(.*?)</h2>", RegexOptions.Singleline)
                    .Select(match => (Id: SectionId(match.Groups[1].Value), Title: Plain(match.Groups[2].Value))).ToArray();
                var sourceToc = Regex.Matches(source, "<a href=\"[^\"]+#([^\"]+)\">(.*?)</a>", RegexOptions.Singleline)
                    .Select(match => (Id: match.Groups[1].Value, Title: Plain(match.Groups[2].Value))).ToArray();
                WithWindow(new DocumentationPageView { Page = page }, window =>
                {
                    Assert.Contains(Descendants<TextBlock>(window), block => block.Text == title);
                    Assert.Contains(Descendants<TextBlock>(window), block => block.Text == introduction);
                    var rendered = Descendants<StackPanel>(window).Where(panel => AutomationProperties.GetAutomationId(panel).StartsWith("section:")).ToArray();
                    Assert.Equal(sections.Select(section => "section:" + section.Id), rendered.Select(AutomationProperties.GetAutomationId));
                    Assert.Equal(sections.Select(section => section.Title), rendered.Select(panel => panel.Children.OfType<TextBlock>().First().Text));
                    var toc = Descendants<Button>(window).Where(button => AutomationProperties.GetAutomationId(button).StartsWith("toc:")).ToArray();
                    Assert.Equal(sections.Select(section => "toc:" + section.Id), toc.Select(AutomationProperties.GetAutomationId));
                    Assert.Equal((sourceToc.Length > 0 ? sourceToc : sections).Select(section => section.Title), toc.Select(button => (string)button.Content));
                    if (page.ImplementationStatus == DemoPageImplementationStatus.InDevelopment)
                    {
                        Assert.Empty(Descendants<VekDataGrid>(window));
                        Assert.Empty(Descendants<CodeSampleView>(window));
                        foreach (var slot in rendered.Where(panel => AutomationProperties.GetAutomationId(panel) is "section:live-example" or "section:code" or "section:client-virtualization" or "section:server-virtualization"))
                            Assert.Contains(Descendants<TextBlock>(slot), block => block.Text.StartsWith("In development"));
                    }
                });
            }
            return true;
        });
    }

    /// <summary>
    /// Local TOC activation scrolls the article to the requested section instead of leaving a decorative link.
    /// </summary>
    [Fact]
    public async Task TableOfContentsScrollsToItsSection()
    {
        await WpfTestDispatcher.RunAsync(() =>
        {
            var page = DemoPageCatalog.All.Single(page => page.Id == "vekdatagrid/filtering");
            WithWindow(new DocumentationPageView { Page = page }, window =>
            {
                var scroll = Descendants<ScrollViewer>(window).Single(view => view.Name == "ArticleScroll");
                var link = Descendants<Button>(window).Single(button => AutomationProperties.GetAutomationId(button) == "toc:filter-options");
                link.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                window.UpdateLayout();
                window.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
                Assert.True(scroll.VerticalOffset > 0);
                var target = Descendants<StackPanel>(window).Single(panel => AutomationProperties.GetAutomationId(panel) == "section:filter-options");
                Assert.InRange(target.TranslatePoint(new Point(), scroll).Y, 0, scroll.ViewportHeight);
            }, 420);
            return true;
        });
    }

    internal static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Vek.slnx"))) directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Cannot find repository root.");
    }

    private static string Plain(string text) => Regex.Replace(System.Net.WebUtility.HtmlDecode(Regex.Replace(text, "<[^>]+>", "")), "\\s+", " ").Trim();

    private static string SectionId(string attributes)
    {
        var id = Regex.Match(attributes, "(?:^|\\s)id=\"([^\"]+)\"");
        return id.Success ? id.Groups[1].Value : Regex.Match(attributes, "aria-labelledby=\"([^\"]+)\"").Groups[1].Value;
    }

    internal static void WithWindow(UserControl view, Action<Window> assertion, double height = 720)
    {
        var window = new Window { Content = view, Width = 1250, Height = height };
        window.Resources.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri("/Vek.WPF.Demo;component/Resources/Theme.xaml", UriKind.Relative)
        });
        try
        {
            window.Show();
            window.UpdateLayout();
            window.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
            assertion(window);
        }
        finally { window.Close(); }
    }

    internal static IEnumerable<T> Descendants<T>(DependencyObject parent) where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
        {
            var child = VisualTreeHelper.GetChild(parent, index);
            if (child is T match) yield return match;
            foreach (var descendant in Descendants<T>(child)) yield return descendant;
        }
    }
}
