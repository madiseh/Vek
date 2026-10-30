namespace Vek.WPF.Demo.Components.Pages.Docs.WPF.DataGrid.BasicUsage;

public partial class BasicUsage : UserControl
{
    private static readonly IReadOnlyList<CodeSample> Samples =
    [
        new("Sample.xaml", """
            <components:VekDataGrid ItemsSource="{Binding DataContext.BasicUsage.Products, RelativeSource={RelativeSource AncestorType={x:Type Window}}}"
                                    AutoGenerateColumns="False" IsReadOnly="True">
                <DataGrid.Columns>
                    <DataGridTextColumn Header="Name" Binding="{Binding Name}" />
                    <DataGridTextColumn Header="Category" Binding="{Binding Category}" />
                    <DataGridTextColumn Header="Price" Binding="{Binding Price, StringFormat=C2}" />
                    <DataGridTextColumn Header="Product ID" Binding="{Binding Id}" />
                </DataGrid.Columns>
            </components:VekDataGrid>
            """),
        new("Sample.xaml.cs", """
            public sealed class BasicUsageViewModel(IDemoProductProvider productProvider)
            {
                public ObservableCollection<DemoProduct> Products { get; } = [];

                public async Task LoadProductsAsync(CancellationToken cancellationToken = default)
                {
                    var products = await productProvider.GetProductsAsync(cancellationToken);
                    Products.Clear();
                    foreach (var product in products.Take(5)) Products.Add(product);
                }
            }
            """)
    ];

    public BasicUsage() => InitializeComponent();

    public IReadOnlyList<CodeSample> CodeSamples => Samples;

    private void NavigateToSection(object sender, RoutedEventArgs args)
    {
        if (sender is Button { Tag: FrameworkElement target }) target.BringIntoView();
    }
}
