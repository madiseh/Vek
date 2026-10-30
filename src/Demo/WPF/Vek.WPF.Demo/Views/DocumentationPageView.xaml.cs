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
        new("Greeting.cs", """
            public static class Greeting
            {
                public static string Create(string name) => $"Hello, {name}!";
            }
            """)
    ];

    private static readonly IReadOnlyList<CodeSample> CodeViewMultiSamplesData =
    [
        new("HTML", """
            <article class="card">
                <h2>Welcome</h2>
                <p>Browse the latest updates.</p>
            </article>
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

    private static readonly IReadOnlyList<CodeSample> GettingStartedCodeSamplesData =
    [
        new("WpfApp.csproj", """
            <PropertyGroup>
                <TargetFramework>net10.0-windows</TargetFramework>
                <UseWPF>true</UseWPF>
            </PropertyGroup>

            <ItemGroup>
                <!-- Replace x.y.z with the package version you want to use. -->
                <PackageReference Include="Vek.WPF.DataGrid" Version="x.y.z" />
            </ItemGroup>
            """),
        new("MainWindow.xaml", """
            <Window x:Class="MyApp.MainWindow"
                    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:components="clr-namespace:Vek.WPF.DataGrid.Components;assembly=Vek.WPF.DataGrid">
                <components:VekDataGrid ItemsSource="{Binding Products}"
                                        AutoGenerateColumns="True"
                                        IsReadOnly="True" />
            </Window>
            """)
    ];

    public static readonly DependencyProperty PageProperty = DependencyProperty.Register(
        nameof(Page), typeof(DemoPageDefinition), typeof(DocumentationPageView),
        new PropertyMetadata(null, OnPageContextChanged));

    public static readonly DependencyProperty BasicUsageProperty = DependencyProperty.Register(
        nameof(BasicUsage), typeof(BasicUsageViewModel), typeof(DocumentationPageView),
        new PropertyMetadata(null, OnPageContextChanged));

    private BasicUsageViewModel? _requestedBasicUsage;

    public DocumentationPageView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
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

    public IReadOnlyList<CodeSample> GettingStartedCodeSamples => GettingStartedCodeSamplesData;

    private static void OnPageContextChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args) =>
        ((DocumentationPageView)dependencyObject).LoadBasicUsageForCurrentPage();

    private void OnLoaded(object sender, RoutedEventArgs args) => LoadBasicUsageForCurrentPage();

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs args) =>
        LoadBasicUsageForPage(args.NewValue as DemoPageDefinition);

    private void LoadBasicUsageForCurrentPage() => LoadBasicUsageForPage(Page ?? DataContext as DemoPageDefinition);

    private void LoadBasicUsageForPage(DemoPageDefinition? page)
    {
        if (page?.Id != "vekdatagrid/basic-usage" || BasicUsage is not { } basicUsage ||
            ReferenceEquals(_requestedBasicUsage, basicUsage))
        {
            return;
        }

        _requestedBasicUsage = basicUsage;
        _ = basicUsage.LoadProductsAsync();
    }
}
