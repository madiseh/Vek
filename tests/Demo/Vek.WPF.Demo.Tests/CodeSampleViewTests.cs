using Vek.WPF.Demo.Views;
using Vek.WPF.Demo.ViewModels;

namespace Vek.WPF.Demo.Tests;

public sealed class CodeSampleViewTests
{
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
                var tabs = Descendants<TabControl>(window).Single();
                tabs.SelectedIndex = 1;
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

    private static Task<T> RunOnStaThreadAsync<T>(Func<T> action)
    {
        var completion = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        var thread = new Thread(() =>
        {
            try
            {
                completion.SetResult(action());
            }
            catch (Exception exception)
            {
                completion.SetException(exception);
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        return completion.Task;
    }
}
