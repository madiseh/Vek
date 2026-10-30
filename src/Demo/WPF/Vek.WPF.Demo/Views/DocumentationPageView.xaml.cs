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
            using System.Collections.ObjectModel;
            using Vek.Demo.Abstractions;

            public sealed class BasicUsageViewModel(IDemoProductProvider productProvider)
            {
                public ObservableCollection<DemoProduct> Products { get; } = [];

                public async Task LoadProductsAsync(CancellationToken cancellationToken = default)
                {
                    var products = await productProvider.GetProductsAsync(cancellationToken);
                    Products.Clear();
                    foreach (var product in products.Take(5))
                    {
                        Products.Add(product);
                    }
                }
            }
            """)
    ];

    private static readonly IReadOnlyList<CodeSample> CodeViewBasicSamplesData =
    [
        new("HTML", "<article class=\"card\">\n    <h2>Welcome</h2>\n    <p>Browse the latest updates.</p>\n</article>"),
        new("Java", "public class Greeting {\n    public static void main(String[] args) {\n        System.out.println(\"Hello, Vek!\");\n    }\n}"),
        new("C#", "public static class Greeting\n{\n    public static string Create(string name) => $\"Hello, {name}!\";\n}"),
        new("SQL", "SELECT name, created_at\nFROM users\nWHERE is_active = 1\nORDER BY created_at DESC;"),
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

    private static void OnPageContextChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args) =>
        ((DocumentationPageView)dependencyObject).RefreshPage();

    private void OnLoaded(object sender, RoutedEventArgs args) => RefreshPage();

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs args) =>
        RefreshPage();

    private void RefreshPage()
    {
        if (Article is null || (Page ?? DataContext as DemoPageDefinition) is not { } page || page.Id == "home") return;
        Article.Children.Clear();
        TableOfContents.Children.Clear();
        ArticleScroll.ScrollToTop();
        var content = DocumentationPageContent.For(page);
        Article.Children.Add(new TextBlock { Text = content.Title, FontSize = 36, FontWeight = FontWeights.Bold, TextWrapping = TextWrapping.Wrap });
        Article.Children.Add(Paragraph(content.Introduction));
        if (page.Section == "VekDataGrid")
        {
            Article.Children.Add(Notice("Blazor reference — the explanations below describe the Blazor component. " +
                (page.ImplementationStatus == DemoPageImplementationStatus.InDevelopment
                    ? "This topic's native WPF examples and APIs are in development."
                    : "The native WPF sample supports column sorting through ItemsSource. Pagination and the Blazor filter APIs are in development.")));
        }
        else if (page.Section == "VekCodeView")
        {
            Article.Children.Add(Notice("Native WPF samples use selectable, read-only text. Copy selected text with Ctrl+C. Syntax highlighting and a dedicated copy action are in development."));
        }

        if (content.Sections.Count > 0)
            TableOfContents.Children.Add(new TextBlock { Text = "On this page", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 12) });
        foreach (var section in content.Sections)
        {
            var panel = new StackPanel { Margin = new Thickness(0, 36, 0, 0), Focusable = true };
            panel.SetResourceReference(FocusVisualStyleProperty, "FocusStyle");
            AutomationProperties.SetAutomationId(panel, "section:" + section.Id);
            AutomationProperties.SetName(panel, section.Title);
            panel.Children.Add(new TextBlock { Text = section.Title, FontSize = 25, FontWeight = FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap });
            if (section.Role == DocumentationContentRole.InDevelopment)
                panel.Children.Add(Notice("In development — this native WPF example is being prepared. The following explanation describes the Blazor reference."));
            foreach (var paragraph in section.Paragraphs) panel.Children.Add(Paragraph(paragraph));
            switch (section.Role)
            {
                case DocumentationContentRole.NativeGrid:
                    panel.Children.Add(new ContentControl { ContentTemplate = (DataTemplate)Resources["NativeGridTemplate"] });
                    break;
                case DocumentationContentRole.NativeGridCode:
                    panel.Children.Add(Paragraph("WPF equivalent: bind ItemsSource in Sample.xaml and load products in BasicUsageViewModel.cs."));
                    panel.Children.Add(Sample(BasicUsageCodeSamples));
                    break;
                case DocumentationContentRole.CodeSample:
                    panel.Children.Add(Sample([CodeViewBasicSamples.Single(sample => sample.Title == section.Title)]));
                    break;
                case DocumentationContentRole.MultiSample:
                    panel.Children.Add(Sample(CodeViewMultiSamples));
                    break;
            }
            Article.Children.Add(panel);
            var link = new Button { Content = section.TocTitle ?? section.Title, HorizontalContentAlignment = HorizontalAlignment.Left, Tag = panel };
            link.SetResourceReference(StyleProperty, "TocLinkStyle");
            AutomationProperties.SetAutomationId(link, "toc:" + section.Id);
            link.Click += (_, _) =>
            {
                panel.Focus();
                ArticleScroll.ScrollToVerticalOffset(panel.TranslatePoint(new Point(), Article).Y);
            };
            TableOfContents.Children.Add(link);
        }
        LoadBasicUsageForPage(page);
    }

    private static CodeSampleView Sample(IReadOnlyList<CodeSample> samples) => new() { Samples = samples, Height = 300, Margin = new Thickness(0, 16, 0, 0) };

    private static TextBlock Paragraph(string text)
    {
        var block = new TextBlock { Text = text, TextWrapping = TextWrapping.Wrap, FontSize = 15, Margin = new Thickness(0, 16, 0, 0) };
        block.SetResourceReference(StyleProperty, "DescriptionStyle");
        return block;
    }

    private static Border Notice(string text)
    {
        var notice = new Border { Child = Paragraph(text), Padding = new Thickness(16, 0, 16, 16), Margin = new Thickness(0, 20, 0, 0), CornerRadius = new CornerRadius(6) };
        notice.SetResourceReference(BackgroundProperty, "SelectedBrush");
        return notice;
    }

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
