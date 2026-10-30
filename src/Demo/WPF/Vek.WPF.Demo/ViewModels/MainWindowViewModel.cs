using System.ComponentModel;
using Vek.WPF.Demo.Navigation;

namespace Vek.WPF.Demo.ViewModels;

public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    private DemoPageDefinition _selectedPage = DemoPageCatalog.All[0];

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
}
