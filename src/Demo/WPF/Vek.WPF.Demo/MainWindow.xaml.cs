namespace Vek.WPF.Demo;

public partial class MainWindow : Window
{
    public MainWindow() : this((Application.Current as App)?.ProductProvider)
    {
    }

    public MainWindow(IDemoProductProvider? productProvider)
    {
        InitializeComponent();
        DataContext = new ViewModels.MainWindowViewModel(productProvider);
    }
}
