namespace Vek.WPF.Demo.Views;

public partial class DocumentationPageView : UserControl
{
    private static readonly IReadOnlyList<CodeSample> BasicUsageCodeSamplesData =
    [
        new("Sample.xaml", """
            <components:VekDataGrid ItemsSource="{Binding Products}"
                                    AutoGenerateColumns="False"
                                    IsReadOnly="True">
                <DataGrid.Columns>
                    <DataGridTextColumn Header="Name" Binding="{Binding Name}" />
                    <DataGridTextColumn Header="Category" Binding="{Binding Category}" />
                    <DataGridTextColumn Header="Price" Binding="{Binding Price, StringFormat=C2}" />
                    <DataGridTextColumn Header="Product ID" Binding="{Binding Id}" />
                </DataGrid.Columns>
            </components:VekDataGrid>
            """),
        new("BasicUsageViewModel.cs", """
            public async Task LoadProductsAsync(CancellationToken cancellationToken = default)
            {
                var products = await ProductProvider.GetProductsAsync(cancellationToken);
                foreach (var product in products.Take(5))
                {
                    Products.Add(product);
                }
            }
            """)
    ];

    private static readonly IReadOnlyList<CodeSample> CodeViewBasicSamplesData =
    [
        new("HTML", """
            <article class="card">
                <h2>Welcome</h2>
                <p>Browse the latest updates.</p>
            </article>
            """),
        new("Java", """
            public class Greeting {
                public static void main(String[] args) {
                    System.out.println("Hello, Vek!");
                }
            }
            """),
        new("C#", """
            public static class Greeting
            {
                public static string Create(string name) => $"Hello, {name}!";
            }
            """),
        new("SQL", """
            SELECT name, created_at
            FROM users
            WHERE is_active = 1
            ORDER BY created_at DESC;
            """)
    ];

    private static readonly IReadOnlyList<CodeSample> CodeViewMultiSamplesData =
    [
        CodeViewBasicSamplesData[0],
        CodeViewBasicSamplesData[2],
        CodeViewBasicSamplesData[3]
    ];

    public static readonly DependencyProperty PageProperty = DependencyProperty.Register(
        nameof(Page), typeof(DemoPageDefinition), typeof(DocumentationPageView));

    public static readonly DependencyProperty BasicUsageProperty = DependencyProperty.Register(
        nameof(BasicUsage), typeof(BasicUsageViewModel), typeof(DocumentationPageView));

    private bool _hasRequestedProducts;

    public DocumentationPageView()
    {
        InitializeComponent();
    }

    public DemoPageDefinition? Page
    {
        get => (DemoPageDefinition?)GetValue(PageProperty);
        set => SetValue(PageProperty, value);
    }

    public BasicUsageViewModel? BasicUsage
    {
        get => (BasicUsageViewModel?)GetValue(BasicUsageProperty);
        set => SetValue(BasicUsageProperty, value);
    }

    public IReadOnlyList<CodeSample> BasicUsageCodeSamples => BasicUsageCodeSamplesData;

    public IReadOnlyList<CodeSample> CodeViewBasicSamples => CodeViewBasicSamplesData;

    public IReadOnlyList<CodeSample> CodeViewMultiSamples => CodeViewMultiSamplesData;

    private async void OnLoaded(object sender, RoutedEventArgs args)
    {
        if (_hasRequestedProducts || Page?.Id != "vekdatagrid/basic-usage" || BasicUsage is null)
        {
            return;
        }

        _hasRequestedProducts = true;
        try
        {
            await BasicUsage.LoadProductsAsync();
        }
        catch (Exception exception)
        {
            LoadErrorText.Text = $"Sample products could not be loaded: {exception.Message}";
            LoadErrorText.Visibility = Visibility.Visible;
        }
    }
}
