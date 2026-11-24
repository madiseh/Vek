// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Vek.Charts;
using Vek.Docs.Models;

namespace Vek.Docs.Services
{
#nullable enable
    /// <summary>
    /// The aim of this class is to add new items to NavMenu
    /// </summary>
    public class MenuService : IMenuService
    {
        private IEnumerable<DocsLink>? _features;
        private IEnumerable<DocsLink>? _utilities;
        private DocsComponents? _docsComponentsApi; //cached property
        private IEnumerable<DocsLink>? _customization;
        private readonly Dictionary<Type, VekComponent> _parents = [];
        private readonly Dictionary<Type, VekComponent> _componentLookup = [];

        /// <summary>
        /// Here is where the links for the Components Menu in NavMenu are added
        /// Add here the new menu elements without caring about the order.
        /// They will be reordered automatically
        /// </summary>
        private readonly List<VekComponent> _docsComponents = new DocsComponents()
            //Individual elements
            .AddItem("Container", typeof(VekContainer))
            .AddItem("Grid", typeof(VekGrid), typeof(VekItem))
            .AddItem("Hidden", typeof(VekHidden))
            .AddItem("Breakpoint Provider", typeof(VekBreakpointProvider))
            .AddItem("Chips", typeof(VekChip<T>))
            .AddItem("Chip Set", typeof(VekChipSet<T>))
            .AddItem("Badge", typeof(VekBadge))
            .AddItem("App Bar", typeof(VekAppBar))
            .AddItem("Drawer", typeof(VekDrawer), typeof(VekDrawerHeader), typeof(VekDrawerContainer))
            .AddItem("Drop Zone", typeof(VekDropZone<T>), typeof(VekDropContainer<T>), typeof(VekDynamicDropItem<T>))
            .AddItem("Link", typeof(VekLink))
            .AddItem("Menu", typeof(VekMenu), typeof(VekMenuItem))
            .AddItem("Message Box", typeof(VekMessageBox))
            .AddItem("Nav Menu", typeof(VekNavMenu), typeof(VekNavLink), typeof(VekNavGroup))
            .AddItem("Tabs", typeof(VekTabs), typeof(VekTabPanel), typeof(VekDynamicTabs))
            .AddItem("Progress", typeof(VekProgressCircular), typeof(VekProgressLinear))
            .AddItem("Dialog", typeof(VekDialog), typeof(VekDialogContainer), typeof(VekDialogProvider))
            .AddItem("Snackbar", typeof(SnackbarService), typeof(VekSnackbarProvider), typeof(VekSnackbarElement))
            .AddItem("Avatar", typeof(VekAvatar), typeof(VekAvatarGroup))
            .AddItem("Alert", typeof(VekAlert))
            .AddItem("Card", typeof(VekCard), typeof(VekCardActions), typeof(VekCardContent), typeof(VekCardHeader), typeof(VekCardMedia))
            .AddItem("Divider", typeof(VekDivider))
            .AddItem("Expansion Panels", typeof(VekExpansionPanels), typeof(VekExpansionPanel))
            .AddItem("Image", typeof(VekImage))
            .AddItem("Icons", typeof(VekIcon))
            .AddItem("List", typeof(VekList<T>), typeof(VekListItem<T>), typeof(VekListSubheader))
            .AddItem("Paper", typeof(VekPaper))
            .AddItem("Rating", typeof(VekRating), typeof(VekRatingItem))
            .AddItem("Skeleton", typeof(VekSkeleton))
            .AddItem("Table", typeof(VekTable<T>), typeof(VekTableBase), typeof(VekTablePager), typeof(VekTableGroupRow<T>), typeof(VekTableSortLabel<T>), typeof(VekTd), typeof(VekTh), typeof(VekTr), typeof(VekTFootRow), typeof(VekTHeadRow))
            // Data Grid Group
            .AddNavGroup("Data Grid", false, new DocsComponents()
                .AddItem("Overview", typeof(VekDataGrid<T>), "datagrid", typeof(Column<T>), typeof(FilterHeaderCell<T>), typeof(FooterCell<T>), typeof(HeaderCell<T>), typeof(HierarchyColumn<T>), typeof(VekDataGridPager<T>), typeof(TemplateColumn<T>))
                .AddItem("Sorting", typeof(VekDataGrid<T>), "datagrid/sorting")
                .AddItem("Filtering", typeof(VekDataGrid<T>), "datagrid/filtering")
                .AddItem("Grouping", typeof(VekDataGrid<T>), "datagrid/grouping")
                .AddItem("Editing", typeof(VekDataGrid<T>), "datagrid/editing")
                .AddItem("Selection", typeof(VekDataGrid<T>), "datagrid/selection")
                .AddItem("Detail Row", typeof(VekDataGrid<T>), "datagrid/detail-row")
                .AddItem("Columns & Layout", typeof(VekDataGrid<T>), "datagrid/columns")
                .AddItem("Virtualization & Server Data", typeof(VekDataGrid<T>), "datagrid/virtualization")
            , sortByName: false, icon: Icons.Material.Filled.TableView)
            .AddItem("Simple Table", typeof(VekSimpleTable))
            .AddItem("Tooltip", typeof(VekTooltip))
            .AddItem("Typography", typeof(VekText))
            .AddItem("Overlay", typeof(VekOverlay))
            .AddItem("Highlighter", typeof(VekHighlighter))
            .AddItem("Element", typeof(VekElement))
            .AddItem("Focus Trap", typeof(VekFocusTrap))
            .AddItem("Tree View", typeof(VekTreeView<T>), typeof(VekTreeViewItem<T>), typeof(VekTreeViewItemToggleButton))
            .AddItem("Breadcrumbs", typeof(VekBreadcrumbs))
            .AddItem("Scroll To Top", typeof(VekScrollToTop))
            .AddItem("Popover", typeof(VekPopover))
            .AddItem("Swipe Area", typeof(VekSwipeArea))
            .AddItem("Tool Bar", typeof(VekToolBar))
            .AddItem("Carousel", typeof(VekCarousel<T>), typeof(VekCarouselItem))
            .AddItem("Timeline", typeof(VekTimeline), typeof(VekTimelineItem))
            .AddItem("Pagination", typeof(VekPagination))
            .AddItem("Stack", typeof(VekStack))
            .AddItem("Spacer", typeof(VekSpacer))
            .AddItem("Collapse", typeof(VekCollapse))
            .AddItem("Stepper", typeof(VekStepper), typeof(VekStep))
            .AddItem("Split Panel", typeof(VekSplitPanel))
            .AddItem("Exit Prompt", typeof(VekExitPrompt))
            .AddItem("Hotkey", typeof(VekHotkey))

            //GROUPS

            //Inputs
            .AddNavGroup("Form & Inputs", false, new DocsComponents()
                .AddItem("Radio", typeof(VekRadio<T>), typeof(VekRadioGroup<T>))
                .AddItem("Check Box", typeof(VekCheckBox<T>))
                .AddItem("Select", typeof(VekSelect<T>), typeof(VekSelectItem<T>))
                .AddItem("Slider", typeof(VekSlider<T>))
                .AddItem("Switch", typeof(VekSwitch<T>))
                .AddItem("Text Field", typeof(VekTextField<T>))
                .AddItem("Masked Input", typeof(VekMaskedInput), "mask")
                .AddItem("Numeric Field", typeof(VekNumericField<T>))
                .AddItem("Form", typeof(VekForm))
                .AddItem("Field", typeof(VekField))
                .AddItem("File Upload", typeof(VekFileUpload<T>))
                .AddItem("Toggle Group", typeof(VekToggleGroup<T>), typeof(VekToggleItem<T>))
            , icon: Icons.Material.Filled.TextFields)

            // Autocomplete
            .AddNavGroup("Autocomplete", false, new DocsComponents()
                .AddItem("Overview", typeof(VekAutocomplete<T>), "autocomplete")
                .AddItem("Templates & Presentation", typeof(VekAutocomplete<T>), "autocomplete/templates-presentation")
                .AddItem("Async Search & Virtualization", typeof(VekAutocomplete<T>), "autocomplete/async-search-virtualization")
                .AddItem("Validation & Coercion", typeof(VekAutocomplete<T>), "autocomplete/validation-coercion")
                .AddItem("Keyboard & Accessibility", typeof(VekAutocomplete<T>), "autocomplete/keyboard-accessibility")
            , sortByName: false, icon: Icons.Material.Filled.Search)

            // Pickers
            .AddNavGroup("Date Picker", false, new DocsComponents()
                .AddItem("Overview", typeof(VekDatePicker), "datepicker")
                .AddItem("Date Input", typeof(VekDatePicker), "datepicker/typed-input")
                .AddItem("Selectable Dates", typeof(VekDatePicker), "datepicker/availability")
                .AddItem("Calendar Navigation", typeof(VekDatePicker), "datepicker/calendar-navigation")
                .AddItem("Calendar Display", typeof(VekDatePicker), "datepicker/display-modes")
                .AddItem("Culture & Formatting", typeof(VekDatePicker), "datepicker/culture-formatting")
                .AddItem("Keyboard & Accessibility", typeof(VekDatePicker), "datepicker/keyboard-accessibility")
                .AddItem("Date Range", typeof(VekDateRangePicker), "daterangepicker")
            , sortByName: false, icon: Icons.Material.Filled.CalendarMonth)
            .AddNavGroup("Pickers", false, new DocsComponents()
                .AddItem("Time Picker", typeof(VekTimePicker))
                .AddItem("Color Picker", typeof(VekColorPicker))
            )

            //Buttons
            .AddNavGroup("Buttons", false, new DocsComponents()
                .AddItem("Button", typeof(VekButton))
                .AddItem("Button Group", typeof(VekButtonGroup))
                .AddItem("Icon Button", typeof(VekIconButton))
                .AddItem("Toggle Icon Button", typeof(VekToggleIconButton))
                .AddItem("Button FAB", typeof(VekFab))
                .AddItem("Button FAB Menu", typeof(VekFabMenu))
            , icon: Icons.Material.Filled.TouchApp)

            //Charts
            .AddNavGroup("Charts", false, new DocsComponents()
                .AddItem("Donut Chart", typeof(Donut<T>), typeof(DonutChartOptions), typeof(Legend<T>))
                .AddItem("Line Chart", typeof(Line<T>), typeof(LineChartOptions), typeof(Legend<T>))
                .AddItem("Pie Chart", typeof(Pie<T>), typeof(PieChartOptions), typeof(Legend<T>))
                .AddItem("Bar Chart", typeof(Bar<T>), typeof(BarChartOptions), typeof(Legend<T>))
                .AddItem("Heat Map Chart", typeof(HeatMap<T>), typeof(HeatMapChartOptions), typeof(Legend<T>))
                .AddItem("Stacked Bar Chart", typeof(StackedBar<T>), typeof(StackedBarChartOptions), typeof(Legend<T>))
                .AddItem("Time Series Chart", typeof(TimeSeries<T>), typeof(TimeSeriesChartOptions), typeof(Legend<T>))
                .AddItem("Radar Chart", typeof(Radar<T>), typeof(RadarChartOptions), typeof(Legend<T>))
                .AddItem("Rose Chart", typeof(Rose<T>), typeof(RoseChartOptions), typeof(Legend<T>))
                .AddItem("Sankey Chart", typeof(Sankey<T>), typeof(SankeyChartOptions), typeof(Legend<T>))
                .AddItem("Scatter Plot Chart", typeof(ScatterPlot<T>), typeof(ScatterPlotChartOptions), typeof(Legend<T>))
                .AddItem("Universal Chart", typeof(VekChart<T>), typeof(VekAxisChartBase<,>), typeof(ChartOptions))
            , icon: Icons.Material.Filled.InsertChart)

            // this must be last!
            .GetComponentsSortedByName();

        /// <summary>
        /// Features menu links
        /// </summary>
        public IEnumerable<DocsLink> Features => _features ??= new List<DocsLink>
        {
            new DocsLink { Title = "Breakpoints", Href = "features/breakpoints" },
            new DocsLink { Title = "Colors", Href = "features/colors" },
            new DocsLink { Title = "Elevation", Href = "features/elevation" },
            new DocsLink { Title = "Converters", Href = "features/converters" },
            new DocsLink { Title = "Icon Reference", Href = "features/icons" }, // <-- note: title changed from "Icons" to "Icon Reference" to avoid confusion in Search box with the VekIcon page which is also called "Icons"
            new DocsLink { Title = "Parameter State", Href = "features/parameterstate" },
            new DocsLink { Title = "Masking", Href = "features/masking" },
            new DocsLink { Title = "RTL Languages", Href = "features/rtl-languages" },
            new DocsLink { Title = "Localization", Href = "features/localization" },
            new DocsLink { Title = "Analyzers", Href = "features/analyzers" },
            new DocsLink { Title = "Services", Href = "features/services" },
        }.OrderBy(x => x.Title);

        /// <summary>
        /// Customization menu links
        /// </summary>
        public IEnumerable<DocsLink> Customization => _customization ??= new List<DocsLink>
        {
            new DocsLink { Title = "Default theme", Href = "customization/default-theme" },
            new DocsLink { Title = "Overview", Href = "customization/overview" },
            new DocsLink { Title = "Palette", Href = "customization/palette" },
            new DocsLink { Title = "Typography", Href = "customization/typography" },
            new DocsLink { Title = "z-index", Href = "customization/z-index" },
            new DocsLink { Title = "Pseudo CSS", Href = "customization/pseudocss" },
            new DocsLink { Title = "Globals", Href = "customization/globals", Order = 100 },
        }.OrderBy(link => link.Order).ThenBy(x => x.Title);

        /// <summary>
        /// CSS Utilities menu links
        /// </summary>
        public IEnumerable<DocsLink> Utilities => _utilities ??= new List<DocsLink>
        {
            new DocsLink { Group = "Layout", Title = "Display", Href = "utilities/display" },
            new DocsLink { Group = "Layout", Title = "Z-Index", Href = "utilities/z-index" },
            new DocsLink { Group = "Layout", Title = "Overflow", Href = "utilities/overflow" },
            new DocsLink { Group = "Layout", Title = "Visibility", Href = "utilities/visibility" },
            new DocsLink { Group = "Layout", Title = "Object Fit", Href = "utilities/object-fit" },
            new DocsLink { Group = "Layout", Title = "Object Position", Href = "utilities/object-position" },
            new DocsLink { Group = "Layout", Title = "Position", Href = "utilities/position" },

            new DocsLink { Group = "Flexbox", Title = "Enable Flexbox", Href = "utilities/enable-flex" },
            new DocsLink { Group = "Flexbox", Title = "Flex Direction", Href = "utilities/flex-direction" },
            new DocsLink { Group = "Flexbox", Title = "Flex Wrap", Href = "utilities/flex-wrap" },
            new DocsLink { Group = "Flexbox", Title = "Flex", Href = "utilities/flex" },
            new DocsLink { Group = "Flexbox", Title = "Flex Grow", Href = "utilities/flex-grow" },
            new DocsLink { Group = "Flexbox", Title = "Flex Shrink", Href = "utilities/flex-shrink" },
            new DocsLink { Group = "Flexbox", Title = "Order", Href = "utilities/order" },
            new DocsLink { Group = "Flexbox", Title = "Gap", Href = "utilities/gap" },
            new DocsLink { Group = "Flexbox", Title = "Justify Content", Href = "utilities/justify-content" },
            new DocsLink { Group = "Flexbox", Title = "Align Content", Href = "utilities/align-content" },
            new DocsLink { Group = "Flexbox", Title = "Align Items", Href = "utilities/align-items" },
            new DocsLink { Group = "Flexbox", Title = "Align Self", Href = "utilities/align-self" },

            new DocsLink { Group = "Spacing", Title = "Spacing", Href = "utilities/spacing" },

            new DocsLink { Group = "Borders", Title = "Border Radius", Href = "utilities/border-radius" },
            new DocsLink { Group = "Borders", Title = "Border Style", Href = "utilities/border-style" },
            new DocsLink { Group = "Borders", Title = "Border Width", Href = "utilities/border-width" },

            new DocsLink { Group = "Interactivity", Title = "Cursor", Href = "utilities/cursor" },
            new DocsLink { Group = "Interactivity", Title = "Pointer Events", Href = "utilities/pointer-events" },
        };

        public IEnumerable<VekComponent> Components => _docsComponents;

        public IEnumerable<VekComponent> Api => DocsComponentsApi.Components;

        public MenuService()
        {
            foreach (var component in Components)
            {
                if (component.IsNavGroup)
                {
                    foreach (var groupComponent in component.GroupComponents)
                    {
                        _componentLookup.TryAdd(groupComponent.Type, groupComponent);
                        _parents.TryAdd(groupComponent.Type, component);
                        if (groupComponent.ChildTypes is not null)
                        {
                            foreach (var childType in groupComponent.ChildTypes)
                            {
                                _parents.TryAdd(childType, component);
                            }
                        }
                    }
                }
                else
                {
                    _componentLookup.TryAdd(component.Type, component);
                    // top-level types refer to themself as parent ;)
                    _parents.TryAdd(component.Type, component);
                    if (component.ChildTypes is not null)
                    {
                        foreach (var childType in component.ChildTypes)
                        {
                            _parents.TryAdd(childType, component);
                        }
                    }
                }
            }
        }

        public VekComponent? GetParent(Type? child)
        {
            return child is not null
                ? _parents.GetValueOrDefault(child)
                : null;
        }

        public VekComponent? GetComponent(Type? type)
        {
            if (type is null)
            {
                return null;
            }

            return _componentLookup.TryGetValue(type, out var component)
                ? component
                : _parents.GetValueOrDefault(type);
        }

        /// <inheritdoc />
        public string? GetComponentName(string typeName)
        {
            var cleanName = typeName.Replace("`1", "<T>").Replace("`2", "<T, U>");
            foreach (var component in _docsComponents)
            {
                if (component.ComponentName != null && component.ComponentName.Equals(cleanName, StringComparison.OrdinalIgnoreCase))
                {
                    return component.Name;
                }

                if (component.GroupComponents != null)
                {
                    foreach (var groupComponent in component.GroupComponents)
                    {
                        if (groupComponent.ComponentName.Equals(cleanName, StringComparison.OrdinalIgnoreCase))
                        {
                            return groupComponent.Name;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// This autogenerates the Menu for the API
        /// </summary>
        private DocsComponents DocsComponentsApi
        {
            get
            {
                //caching property
                if (_docsComponentsApi is not null)
                {
                    return _docsComponentsApi;
                }

                _docsComponentsApi = new DocsComponents();
                var addedTypes = new HashSet<Type>();
                foreach (var item in Components)
                {
                    if (item.IsNavGroup)
                    {
                        if (item.Name == "Data Grid")
                        {
                            _docsComponentsApi.AddItem(item.Name, typeof(VekDataGrid<T>));
                            addedTypes.Add(typeof(VekDataGrid<T>));
                            continue;
                        }

                        foreach (var groupComponent in item.GroupComponents)
                        {
                            if (addedTypes.Add(groupComponent.Type))
                            {
                                _docsComponentsApi.AddItem(groupComponent.Name, groupComponent.Type);
                            }
                        }
                    }
                    else
                    {
                        if (addedTypes.Add(item.Type))
                        {
                            _docsComponentsApi.AddItem(item.Name, item.Type);
                        }
                    }
                }

                return _docsComponentsApi;
            }
        }

        /// <summary>
        /// Gets the sub-menu, if any, matching the specified type.
        /// </summary>
        /// <param name="parent">The parent to start searching from.</param>
        /// <param name="type">The type to find.</param>
        /// <returns>The menu whose link, child type, or group component matches the type.</returns>
        public VekComponent? GetExample(DocumentedType type)
        {
            // Go through each menu...
            foreach (var menu in Components)
            {
                // Is there a menu for this type?  If so, return it.
                var component = GetExample(menu, type);
                if (component != null)
                {
                    return component;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the sub-menu, if any, matching the specified type.
        /// </summary>
        /// <param name="parent">The parent to start searching from.</param>
        /// <param name="type">The type to find.</param>
        /// <returns>The menu whose link, child type, or group component matches the type.</returns>
        public static VekComponent? GetExample(VekComponent parent, DocumentedType type)
        {
            // Does the name match the menu link?
            if (parent.ComponentName == type.NameFriendly.Replace("<TData>", "<T>"))
            {
                return parent;
            }

            // Are there child types to search?
            if (parent.ChildTypes != null)
            {
                foreach (var childType in parent.ChildTypes)
                {
                    // Does the child type's name match?
                    if (childType.Name == type.Name)
                    {
                        return parent;
                    }
                }
            }

            // Are there sub-menus to search?
            if (parent.IsNavGroup && parent.GroupComponents != null)
            {
                foreach (var subMenu in parent.GroupComponents)
                {
                    // Search one level deeper
                    var component = GetExample(subMenu, type);
                    if (component != null)
                    {
                        return component;
                    }
                }
            }

            return null;
        }
    }
}
