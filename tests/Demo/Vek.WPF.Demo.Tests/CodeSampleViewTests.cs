namespace Vek.WPF.Demo.Tests;

public sealed class CodeSampleViewTests
{
    /// <summary>
    /// Basic Usage renders HTML, Java, C#, and SQL as distinct sections in the same order as Blazor.
    /// </summary>
    [Fact]
    public async Task CodeViewBasicUsageRendersFourSeparateSectionsInBlazorOrder()
    {
        await RunOnStaThreadAsync(() =>
        {
            var page = DemoPageCatalog.All.Single(page => page.Id == "vekcodeview/basic-usage");
            var view = new DocumentationPageView { Page = page, DataContext = page };
            var window = new Window { Content = view };
            try
            {
                window.Show();
                window.UpdateLayout();
                window.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
                var samples = Descendants<CodeSampleView>(window).Where(sample => sample.IsVisible).ToArray();
                Assert.Equal(4, samples.Length);
                Assert.Equal(new[] { "HTML", "Java", "C#", "SQL" }, samples.Select(sample => Assert.Single(sample.Samples).Title));
                Assert.All(Descendants<TextBox>(window), editor => Assert.True(editor.IsReadOnly));
                return true;
            }
            finally
            {
                window.Close();
            }
        });
    }

    /// <summary>
    /// Selecting another sample displays its source in the native read-only code surface.
    /// </summary>
    [Fact]
    public async Task MultiSampleDisplaysSelectedCodeInReadOnlyEditor()
    {
        var result = await RunOnStaThreadAsync(() =>
        {
            var view = new CodeSampleView
            {
                Samples =
                [
                    new CodeSample("HTML", "<p>Welcome</p>"),
                    new CodeSample("C#", "public static string Greeting() => \"Hello\";")
                ]
            };
            var window = new Window { Content = view };
            try
            {
                window.Show();
                window.UpdateLayout();
                window.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
                var tabs = Descendants<TabControl>(window).ToArray();
                Assert.True(tabs.Length > 0, DescribeWindowState(window));
                tabs[0].SelectedIndex = 1;
                window.UpdateLayout();
                window.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);

                var editor = Descendants<TextBox>(window).Single();
                return (editor.IsReadOnly, editor.Text);
            }
            finally
            {
                window.Close();
            }
        });

        Assert.True(result.IsReadOnly);
        Assert.Equal("public static string Greeting() => \"Hello\";", result.Text);
    }

    private static IEnumerable<T> Descendants<T>(DependencyObject parent) where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
        {
            var child = VisualTreeHelper.GetChild(parent, index);
            if (child is T match)
            {
                yield return match;
            }

            foreach (var descendant in Descendants<T>(child))
            {
                yield return descendant;
            }
        }
    }

    private static string DescribeWindowState(Window window) =>
        $"No TabControl. Content={window.Content?.GetType().FullName}; IsLoaded={window.IsLoaded}; " +
        $"App={Application.Current?.GetType().FullName ?? "<null>"}; " +
        $"AppDispatcherShutdown={Application.Current?.Dispatcher.HasShutdownStarted}; " +
        $"WindowDispatcherShutdown={window.Dispatcher.HasShutdownStarted}; Thread={Environment.CurrentManagedThreadId}.";

    private static Task<T> RunOnStaThreadAsync<T>(Func<T> action)
        => WpfTestDispatcher.RunAsync(action);
}
