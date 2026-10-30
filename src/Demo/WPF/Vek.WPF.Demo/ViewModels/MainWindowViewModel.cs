using System.ComponentModel;
using Vek.WPF.Demo.Navigation;

namespace Vek.WPF.Demo.ViewModels;

public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    private DemoPageDefinition _selectedPage = DemoPageCatalog.All[0];

    public MainWindowViewModel(IDemoProductProvider? productProvider = null)
    {
        GetStartedCommand = new GetStartedNavigationCommand(this);
        BasicUsage = productProvider is null ? null : new BasicUsageViewModel(productProvider);
    }

    public ICommand GetStartedCommand { get; }

    public BasicUsageViewModel? BasicUsage { get; }

    public IReadOnlyList<DemoPageDefinition> Pages => DemoPageCatalog.All;

    public DemoPageDefinition SelectedPage
    {
        get => _selectedPage;
        set
        {
            if (_selectedPage == value)
            {
                return;
            }

            _selectedPage = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedPage)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void SelectPage(DemoPageDefinition page) => SelectedPage = page;

    private sealed class GetStartedNavigationCommand(MainWindowViewModel owner) : ICommand
    {
        public event EventHandler? CanExecuteChanged { add { } remove { } }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) =>
            owner.SelectPage(owner.Pages.Single(page => page.Id == "getting-started"));
    }
}
