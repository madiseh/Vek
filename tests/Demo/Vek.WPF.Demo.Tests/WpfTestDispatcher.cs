namespace Vek.WPF.Demo.Tests;

internal static class WpfTestDispatcher
{
    private static readonly Dispatcher UiDispatcher = StartDispatcher();

    public static void ShowTestWindow(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);
        window.WindowStartupLocation = WindowStartupLocation.Manual;
        window.ShowActivated = false;
        window.ShowInTaskbar = false;
        window.Opacity = 0;
        window.Left = SystemParameters.VirtualScreenLeft - Math.Max(window.Width, 1) - 16;
        window.Top = SystemParameters.VirtualScreenTop - Math.Max(window.Height, 1) - 16;
        window.Show();
    }

    public static Task<T> RunAsync<T>(Func<T> action)
    {
        var completion = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        UiDispatcher.BeginInvoke(() =>
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
        return completion.Task;
    }

    private static Dispatcher StartDispatcher()
    {
        var completion = new TaskCompletionSource<Dispatcher>(TaskCreationOptions.RunContinuationsAsynchronously);
        var thread = new Thread(() =>
        {
            completion.SetResult(Dispatcher.CurrentDispatcher);
            Dispatcher.Run();
        })
        {
            IsBackground = true,
            Name = "WPF test dispatcher"
        };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        return completion.Task.GetAwaiter().GetResult();
    }
}
