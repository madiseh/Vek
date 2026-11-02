namespace Vek.WPF.Demo.Components.Pages.Docs.WPF.DataGrid.Filtering;

public partial class Filtering : UserControl
{
    private static readonly IReadOnlyList<CodeSample> Samples =
    [
        new("Filtering.xaml", """
            <components:VekDataGrid
                ItemsSource="{Binding DataContext.BasicUsage.Products, RelativeSource={RelativeSource AncestorType={x:Type Window}}}"
                ShowFilterRow="True" AutoGenerateColumns="False">
                <DataGrid.Columns>
                    <DataGridTextColumn Header="Name" Binding="{Binding Name}" SortMemberPath="Name" />
                    <DataGridTextColumn Header="Category" Binding="{Binding Category}" SortMemberPath="Category" />
                </DataGrid.Columns>
            </components:VekDataGrid>
            """)
    ];

    public Filtering() => InitializeComponent();

    public IReadOnlyList<CodeSample> CodeSamples => Samples;

    private void NavigateToSection(object sender, RoutedEventArgs args)
    {
        if (sender is Button { Tag: FrameworkElement target }) target.BringIntoView();
    }
}
