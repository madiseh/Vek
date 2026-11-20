using System;
using System.Text.RegularExpressions;
using Vek.Charts;

namespace Vek.UnitTests.Docs.Generator;

public partial class TestsForApiPages
{
    /// <summary>
    /// The current production links to API documentation.
    /// </summary>
    private readonly string[] _legacyApiAddresses = [
        // available from the main menu in "API" group
        "api/alert",
        "api/appbar",
        "api/autocomplete",
        "api/avatar",
        "api/badge",
        "api/barchart",
        "api/breadcrumbs",
        "api/breakpointprovider",
        "api/button",
        "api/buttonfab",
        "api/buttongroup",
        "api/card",
        "api/carousel",
        "api/checkbox",
        "api/chips",
        "api/chipset",
        "api/colorpicker",
        "api/container",
        "api/datagrid",
        "api/datepicker",
        "api/dialog",
        "api/divider",
        "api/donutchart",
        "api/drawer",
        "api/element",
        "api/expansionpanels",
        "api/field",
        "api/fileuploader",
        "api/focustrap",
        "api/form",
        "api/grid",
        "api/hidden",
        "api/highlighter",
        "api/iconbutton",
        "api/icons",
        "api/linechart",
        "api/link",
        "api/list",
        "api/menu",
        "api/messagebox",
        "api/navmenu",
        "api/numericfield",
        "api/overlay",
        "api/pagination",
        "api/paper",
        "api/piechart",
        "api/popover",
        "api/progress",
        "api/radio",
        "api/rating",
        "api/scrolltotop",
        "api/select",
        "api/simpletable",
        "api/skeleton",
        "api/slider",
        "api/snackbar",
        "api/swipearea",
        "api/switch",
        "api/table",
        "api/tabs",
        "api/textfield",
        "api/timepicker",
        "api/timeline",
        "api/toggleiconbutton",
        "api/toolbar",
        "api/tooltip",
        "api/treeview",
        "api/typography",

        // subelements - available from components/* pages
        "api/drawerheader",
        "api/drawercontainer",
        "api/navlink",
        "api/navgroup",
        "api/item",
        "api/dynamictabs",
        "api/expansionpanel",
        "api/timelineitem",
        "api/cardactions",
        "api/cardcontent",
        "api/cardheader",
        "api/cardmedia",
        "api/treeviewitem",
        "api/treeviewitemtogglebutton",
        "api/listitem",
        "api/listsubheader",
        "api/carouselitem",
        "api/dialoginstance",
        "api/dialogprovider",
        "api/avatargroup",
        "api/menuitem",
        "api/radiogroup",
        "api/selectitem",
        "api/ratingitem",

        // API pages not linked from the documentation web site, but still available through the URL
        "api/THeadRow",
        "api/TFootRow",
        "api/Tr",
        "api/Th",
        "api/Td",
        "api/TableGroupRow",
        "api/TableSortLabel",
        "api/TablePager",
        "api/InputLabel",
        "api/InputControl",
        "api/Input",
        "api/RangeInput",
        "api/MainContent",
        "api/DateRangePicker",
        "api/Collapse",
        "api/PageContentNavigation",
        "api/RTLProvider",
        "api/SnackbarElement",
        "api/SparkLine",
    ];

    /// <summary>
    /// A list of types which have an example page ("/component/*") to link to.
    /// </summary>
    /// <remarks>
    /// This list should match the types mentioned in the <c>MenuService</c> class in Vek.Docs.
    /// </remarks>
    public List<Type> TypesWithExamples =
    [
        typeof(VekContainer), typeof(VekGrid), typeof(VekItem), typeof(VekHidden),  typeof(VekBreakpointProvider), typeof(VekChip<object>), typeof(VekChipSet<object>),
        typeof(VekBadge), typeof(VekAppBar), typeof(VekDrawer), typeof(VekDrawerHeader), typeof(VekDrawerContainer), typeof(VekDropZone<object>), typeof(VekDropContainer<object>),
        typeof(VekDynamicDropItem<object>), typeof(VekLink), typeof(VekMenu), typeof(VekMenuItem), typeof(VekMessageBox), typeof(VekNavMenu), typeof(VekNavLink), typeof(VekNavGroup),
        typeof(VekTabs), typeof(VekTabPanel), typeof(VekDynamicTabs), typeof(VekProgressCircular), typeof(VekProgressLinear), typeof(VekDialog), typeof(VekDialogContainer),
        typeof(VekDialogProvider), typeof(SnackbarService), typeof(VekSnackbarProvider), typeof(VekSnackbarElement), typeof(VekAvatar), typeof(VekAvatarGroup),
        typeof(VekAlert), typeof(VekCard), typeof(VekCardActions), typeof(VekCardContent), typeof(VekCardHeader), typeof(VekCardMedia), typeof(VekDivider),
        typeof(VekExpansionPanels), typeof(VekExpansionPanel), typeof(VekImage), typeof(VekIcon), typeof(VekList<object>), typeof(VekListItem<object>), typeof(VekListSubheader),
        typeof(VekPaper), typeof(VekRating), typeof(VekRatingItem), typeof(VekSkeleton), typeof(VekTableBase), typeof(VekTable<object>), typeof(VekTablePager),
        typeof(VekTableGroupRow<object>), typeof(VekTableSortLabel<object>), typeof(VekTd), typeof(VekTh), typeof(VekTr), typeof(VekTFootRow), typeof(VekTHeadRow),
        typeof(VekDataGrid<object>), typeof(Column<object>), typeof(FilterHeaderCell<object>), typeof(FooterCell<object>), typeof(HeaderCell<object>), typeof(HierarchyColumn<object>), typeof(VekDataGridPager<object>),
        typeof(TemplateColumn<object>), typeof(VekSimpleTable), typeof(VekTooltip), typeof(VekText), typeof(VekOverlay), typeof(VekHighlighter), typeof(VekElement),
        typeof(VekFocusTrap), typeof(VekTreeView<object>), typeof(VekTreeViewItem<object>), typeof(VekTreeViewItemToggleButton),  typeof(VekBreadcrumbs), typeof(VekScrollToTop),
        typeof(VekPopover),  typeof(VekSwipeArea), typeof(VekToolBar), typeof(VekCarousel<object>), typeof(VekCarouselItem), typeof(VekTimeline), typeof(VekTimelineItem),
        typeof(VekPagination), typeof(VekStack), typeof(VekSpacer), typeof(VekCollapse), typeof(VekStepper), typeof(VekStep), typeof(VekRadio<object>), typeof(VekRadioGroup<object>),
        typeof(VekCheckBox<object>), typeof(VekSelect<object>), typeof(VekSelectItem<object>), typeof(VekSlider<int>), typeof(VekSwitch<object>), typeof(VekTextField<object>),
        typeof(VekNumericField<object>), typeof(VekForm), typeof(VekAutocomplete<object>), typeof(VekField), typeof(VekFileUpload<object>), typeof(VekToggleGroup<object>), typeof(VekToggleItem<object>),
        typeof(VekDatePicker), typeof(VekDateRangePicker), typeof(VekTimePicker), typeof(VekColorPicker),  typeof(VekButton),  typeof(VekButtonGroup), typeof(VekIconButton),
        typeof(VekToggleIconButton), typeof(VekFab), typeof(ChartOptions), typeof(Donut<>), typeof(Line<>), typeof(Legend<>), typeof(Pie<>), typeof(Bar<>), typeof(HeatMap<>),typeof(StackedBar<>),
        typeof(TimeSeries<>), typeof(Radar<>), typeof(Rose<>)
    ];

    /// <summary>
    /// Ensures that an API page is available for each Vek component.
    /// </summary>
    public bool Execute()
    {
        var success = true;
        try
        {
            Directory.CreateDirectory(Paths.TestDirPath);

            var currentCode = string.Empty;
            if (File.Exists(Paths.ApiPageTestsFilePath))
            {
                currentCode = File.ReadAllText(Paths.ApiPageTestsFilePath);
            }

            var cb = new CodeBuilder();

            cb.AddHeader();
            cb.AddLine("using System.Collections.Generic;");
            cb.AddLine("using Bunit;");
            cb.AddLine("using AwesomeAssertions;");
            cb.AddLine("using Microsoft.Extensions.DependencyInjection;");
            cb.AddLine("using Vek.Docs.Pages.Api;");
            cb.AddLine("using Vek.Docs.Services;");
            cb.AddLine("using NUnit.Framework;");
            cb.AddLine();
            cb.AddLine("namespace Vek.UnitTests.Docs.Generated");
            cb.AddLine("{");
            cb.IndentLevel++;
            cb.AddLine("// These tests just check all the API pages to see if they throw any exceptions");
            cb.AddLine("[System.CodeDom.Compiler.GeneratedCodeAttribute(\"Vek.Docs.Compiler\", \"0.0.0.0\")]");
            cb.AddLine("public partial class ApiDocsTests");
            cb.AddLine("{");
            cb.IndentLevel++;

            WritePublicTypeCases(cb);
            WriteLegacyApiCases(cb);
            WritePublicTypeTest(cb);
            WriteLegacyApiTest(cb);

            cb.IndentLevel--;
            cb.AddLine("}");
            cb.IndentLevel--;
            cb.AddLine("}");

            if (currentCode != cb.ToString())
            {
                File.WriteAllText(Paths.ApiPageTestsFilePath, cb.ToString());
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($@"Error generating {Paths.ApiPageTestsFilePath} : {e.Message}");
            success = false;
        }

        return success;
    }

    /// <summary>
    /// Creates the ApiCases method that yields TestCaseData for all public Vek types.
    /// </summary>
    public void WritePublicTypeCases(CodeBuilder cb)
    {
        cb.AddLine("public static IEnumerable<TestCaseData> ApiCases()");
        cb.AddLine("{");
        cb.IndentLevel++;

        var vekBlazorAssembly = typeof(_Imports).Assembly;
        var vekBlazorComponents = vekBlazorAssembly.GetTypes()
            .Where(type =>
                // Include public types
                type.IsPublic
                // ... which aren't excluded
                && !IsExcluded(type)
                // ... which aren't interfaces
                && !type.IsInterface
                && !ImplementsConverterInterface(type)
                // ... which aren't source generators
                && !type.Name.Contains("SourceGenerator")
                // ... which aren't extension classes
                && !type.Name.Contains("Extensions")
                // ... which aren't clone strategies
                && !type.Name.Contains("SystemTextJson"))
            .ToList();

        foreach (var type in vekBlazorComponents)
        {
            // Skip Vek.Color and Vek.Input types
            if (type.Name == "Color" || type.Name == "Input")
            {
                continue;
            }

            var typeName = type.Name.Replace("`", "");
            var hasExampleLink = TypesWithExamples.Exists(exampleType => exampleType.Name == type.Name);
            cb.AddLine($"yield return new TestCaseData(\"{type.Name}\", {hasExampleLink.ToString().ToLowerInvariant()}).SetName(\"{typeName}_API\");");
        }

        cb.IndentLevel--;
        cb.AddLine("}");
        cb.AddLine();
    }

    /// <summary>
    /// Creates the LegacyApiCases method that yields TestCaseData for legacy API links.
    /// </summary>
    public void WriteLegacyApiCases(CodeBuilder cb)
    {
        cb.AddLine("public static IEnumerable<TestCaseData> LegacyApiCases()");
        cb.AddLine("{");
        cb.IndentLevel++;

        foreach (var url in _legacyApiAddresses)
        {
            var component = url.Replace("api/", "");
            cb.AddLine($"yield return new TestCaseData(\"{component}\").SetName(\"{component.Replace("/", "_")}_Legacy_API\");");
        }

        cb.IndentLevel--;
        cb.AddLine("}");
        cb.AddLine();
    }

    /// <summary>
    /// Creates the test method for public type API pages.
    /// </summary>
    public void WritePublicTypeTest(CodeBuilder cb)
    {
        cb.AddLine("[Test]");
        cb.AddLine("[TestCaseSource(nameof(ApiCases))]");
        cb.AddLine("public async Task ApiPage_Renders(string typeName, bool hasExampleLink)");
        cb.AddLine("{");
        cb.IndentLevel++;
        cb.AddLine(@"_navigationManager.NavigateTo($""/components/{typeName}"");");
        cb.AddLine(@"var comp = _ctx.Render<Api>(parameters => parameters.Add(x => x.TypeName, typeName));");
        cb.AddLine(@"await _ctx.Services.GetService<IRenderQueueService>().WaitUntilEmpty();");
        cb.AddLine(@"comp.Find("".vek-breadcrumbs"");");
        cb.AddLine("if (hasExampleLink)");
        cb.AddLine("{");
        cb.IndentLevel++;
        cb.AddLine(@"var exampleLink = comp.FindComponents<VekLink>().FirstOrDefault(link => link.Instance.Href != null && link.Instance.Href.StartsWith(""/component""));");
        cb.AddLine(@"exampleLink.Should().NotBeNull();");
        cb.IndentLevel--;
        cb.AddLine("}");
        cb.IndentLevel--;
        cb.AddLine("}");
        cb.AddLine();
    }

    /// <summary>
    /// Creates the test method for legacy API links.
    /// </summary>
    public void WriteLegacyApiTest(CodeBuilder cb)
    {
        cb.AddLine("[Test]");
        cb.AddLine("[TestCaseSource(nameof(LegacyApiCases))]");
        cb.AddLine("public async Task LegacyApiPage_Renders(string component)");
        cb.AddLine("{");
        cb.IndentLevel++;
        cb.AddLine(@"_navigationManager.NavigateTo($""/components/api/{component}"");");
        cb.AddLine(@"var comp = _ctx.Render<Api>(parameters => parameters.Add(x => x.TypeName, component));");
        cb.AddLine(@"await _ctx.Services.GetService<IRenderQueueService>().WaitUntilEmpty();");
        cb.AddLine(@"comp.Find("".vek-breadcrumbs"");");
        cb.IndentLevel--;
        cb.AddLine("}");
        cb.AddLine();
    }

    /// <summary>
    /// Gets whether a type is excluded from documentation.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>When <c>true</c>, the type is excluded from documentation.</returns>
    private static bool IsExcluded(Type type)
    {
        if (ExcludedTypes.Contains(type.Name))
        {
            return true;
        }
        if (type.FullName != null && ExcludedTypes.Contains(type.FullName))
        {
            return true;
        }
        if (type.FullName != null && ExcludedTypes.Any(type.FullName.StartsWith))
        {
            return true;
        }

        return false;
    }

    private static bool ImplementsConverterInterface(Type type)
    {
        if (!type.IsClass)
        {
            return false;
        }

        return type
            .GetInterfaces()
            .Any(i => i.IsGenericType
                      && i.GetGenericTypeDefinition() == typeof(IConverter<,>));
    }

    /// <summary>
    /// Any types to exclude from documentation.
    /// </summary>
    public static List<string> ExcludedTypes { get; } =
    [
        "ActivatableCallback",
        "AbstractLocalizationInterceptor",
        "CloneableCloneStrategy`1",
        "CssBuilder",
        "Vek._Imports",
        "Vek.CategoryAttribute",
        "Vek.CategoryTypes",
        "Vek.CategoryTypes+",
        "Vek.Colors",
        "Vek.Colors+",
        "Vek.Icons",
        "Vek.Icons+",
        "Vek.LabelAttribute",
        "Vek.Resources.LanguageResource",
        "object",
        "string"
    ];
}
