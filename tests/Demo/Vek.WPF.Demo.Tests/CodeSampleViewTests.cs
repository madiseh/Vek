namespace Vek.WPF.Demo.Tests;

public sealed class CodeSampleViewTests
{
    /// <summary>
    /// CodeView Basic Usage contains one native code sample while Multi-Sample owns the selectable sample set.
    /// </summary>
    [Fact]
    public async Task CodeViewBasicUsageDisplaysOneSample()
    {
        var sampleCount = await RunOnStaThreadAsync(() =>
        {
            var page = DemoPageCatalog.All.Single(page => page.Id == "vekcodeview/basic-usage");
            var view = new DocumentationPageView { Page = page, DataContext = page };
            var window = new Window { Content = view };
            try
            {
                window.Show();
                window.UpdateLayout();
                window.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
                var tabs = Descendants<TabControl>(window).ToArray();
                Assert.True(tabs.Length > 0, DescribeWindowState(window));
                return tabs[0].Items.Count;
            }
            finally
            {
                window.Close();
            }
        });

        Assert.Equal(1, sampleCount);
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
