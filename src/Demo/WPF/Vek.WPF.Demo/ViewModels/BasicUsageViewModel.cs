using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Threading;

namespace Vek.WPF.Demo.ViewModels;

public sealed class BasicUsageViewModel(IDemoProductProvider productProvider) : INotifyPropertyChanged
{
    private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;
    private Task? _loadTask;
    private string? _loadErrorMessage;

    public ObservableCollection<DemoProduct> Products { get; } = [];

    public string? LoadErrorMessage
    {
        get => _loadErrorMessage;
        private set
        {
            if (_loadErrorMessage == value)
            {
                return;
            }

            _loadErrorMessage = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LoadErrorMessage)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public Task LoadProductsAsync(CancellationToken cancellationToken = default) =>
        _loadTask ??= LoadProductsCoreAsync(cancellationToken);

    private async Task LoadProductsCoreAsync(CancellationToken cancellationToken)
    {
        try
        {
            var products = await productProvider.GetProductsAsync(cancellationToken);
            await UpdateOnDispatcherAsync(() =>
            {
                Products.Clear();
                foreach (var product in products.Take(5))
                {
                    Products.Add(product);
                }
            });
        }
        catch (Exception exception)
        {
            await UpdateOnDispatcherAsync(() =>
                LoadErrorMessage = $"Sample products could not be loaded: {exception.Message}");
        }
    }

    private Task UpdateOnDispatcherAsync(Action update)
    {
        if (_dispatcher.CheckAccess())
        {
            update();
            return Task.CompletedTask;
        }

        return _dispatcher.InvokeAsync(update).Task;
    }
}
