using Vek.Core.DataGrid;

namespace Vek.WPF.Demo.Components.Pages.Docs.WPF.DataGrid.Filtering;

public partial class Filtering : UserControl
{
    private static readonly IReadOnlyList<CodeSample> Samples =
    [
        new("Filtering.xaml.cs", """
            FilterGrid.AddColumn(new VekDataGridColumn<DemoProduct, DataTemplate>(
                "Name", product => product.Name, title: "Name", filterable: true, searchable: true,
                cellTemplate: _ => (DataTemplate)FindResource("ProductNameCell")));
            FilterGrid.AddColumn(new VekDataGridColumn<DemoProduct, DataTemplate>(
                "Category", product => product.Category, title: "Category", filterable: true, searchable: true,
                cellTemplate: _ => (DataTemplate)FindResource("ProductCategoryCell")));

            private void GlobalSearchTextChanged(object sender, TextChangedEventArgs args)
                => FilterGrid.SetGlobalSearch(GlobalSearchBox.Text);
            """),
        new("Filtering.xaml", """
            <components:VekDataGrid
                ItemsSource="{Binding DataContext.BasicUsage.Products, RelativeSource={RelativeSource AncestorType={x:Type Window}}}"
                ShowFilterRow="True" AutoGenerateColumns="False">
            </components:VekDataGrid>
            """)
    ];

    public Filtering()
    {
        InitializeComponent();
        FilterGrid.AddColumn(new VekDataGridColumn<DemoProduct, DataTemplate>(
            "Name", product => product.Name, title: "Name", filterable: true, searchable: true,
            cellTemplate: _ => (DataTemplate)FindResource("ProductNameCell")));
        FilterGrid.AddColumn(new VekDataGridColumn<DemoProduct, DataTemplate>(
            "Category", product => product.Category, title: "Category", filterable: true, searchable: true,
            cellTemplate: _ => (DataTemplate)FindResource("ProductCategoryCell")));
    }

    public IReadOnlyList<CodeSample> CodeSamples => Samples;

    private void NavigateToSection(object sender, RoutedEventArgs args)
    {
        if (sender is Button { Tag: FrameworkElement target }) target.BringIntoView();
    }

    private void GlobalSearchTextChanged(object sender, TextChangedEventArgs args) =>
        FilterGrid.SetGlobalSearch(GlobalSearchBox.Text);
}
