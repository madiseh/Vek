// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Vek.Docs.Models;

/// <summary>
/// Represents a set of XML documentation for Vek types.
/// </summary>
public static partial class ApiDocumentation
{
    /// <summary>
    /// The generated documentation for events.
    /// </summary>
    /// <remarks>
    /// Reading this builds the members of every type, because it can be enumerated.
    /// Prefer the lookups below, which build only the type that declares the member being looked for.
    /// </remarks>
    public static Dictionary<string, DocumentedEvent> Events
    {
        get
        {
            ApiDocumentationMembers.EnsureAll();
            return ApiDocumentationMembers.EventsInternal;
        }
    }

    /// <summary>
    /// The generated documentation for fields.
    /// </summary>
    /// <remarks>
    /// Reading this builds the members of every type, because it can be enumerated.
    /// Prefer the lookups below, which build only the type that declares the member being looked for.
    /// </remarks>
    public static Dictionary<string, DocumentedField> Fields
    {
        get
        {
            ApiDocumentationMembers.EnsureAll();
            return ApiDocumentationMembers.FieldsInternal;
        }
    }

    /// <summary>
    /// The generated documentation for types.
    /// </summary>
    public static Dictionary<string, DocumentedType> Types { get; private set; } = [];

    /// <summary>
    /// The generated documentation for properties.
    /// </summary>
    /// <remarks>
    /// Reading this builds the members of every type, because it can be enumerated.
    /// Prefer the lookups below, which build only the type that declares the member being looked for.
    /// </remarks>
    public static Dictionary<string, DocumentedProperty> Properties
    {
        get
        {
            ApiDocumentationMembers.EnsureAll();
            return ApiDocumentationMembers.PropertiesInternal;
        }
    }

    /// <summary>
    /// The generated documentation for methods.
    /// </summary>
    /// <remarks>
    /// Reading this builds the members of every type, because it can be enumerated.
    /// Prefer the lookups below, which build only the type that declares the member being looked for.
    /// </remarks>
    public static Dictionary<string, DocumentedMethod> Methods
    {
        get
        {
            ApiDocumentationMembers.EnsureAll();
            return ApiDocumentationMembers.MethodsInternal;
        }
    }

    /// <summary>
    /// Gets an event, field, method, or property by its name.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static DocumentedMember GetMember(string name)
    {
        // Is this an external member?
        if (!name.StartsWith("Vek", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }
        // Is this an icon?  (We don't document those, but we show them as icons)
        if (name.StartsWith("Vek.Icons", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }
        // An exact key match needs only the type that declares the member, so every kind is tried by key first.
        // Going through GetProperty first instead would build every type as soon as the name turned out to be a method, which is what most cross-references are.
        var match = ApiDocumentationMembers.Member(name);
        if (match != null)
        {
            return match;
        }

        // No key matched, so fall back to the searches by name, which need every type built.
        DocumentedMember result = GetProperty(name);
        result ??= GetField(name);
        result ??= GetEvent(name);
        result ??= GetMethod(name);
        return result;
    }

    /// <summary>
    /// Gets a documented type by its name.
    /// </summary>
    /// <param name="name">The name of the type to find.</param>
    public static DocumentedType GetType(string name)
    {
        // Anything to do?
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        // Is this an external member?
        if (name.StartsWith("System", StringComparison.OrdinalIgnoreCase) || name.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        // First, try an exact match
        if (Types.TryGetValue(name, out var match))
        {
            return match;
        }

        // Next, try with the Vek namespace
        if (Types.TryGetValue("Vek." + name, out match))
        {
            return match;
        }

        // Look for a component with a generic
        if (Types.TryGetValue("Vek." + name + "`1", out match))
        {
            return match;
        }

        // Look for a component with two generics
        if (Types.TryGetValue("Vek." + name + "`2", out match))
        {
            return match;
        }

        // Look for legacy links like "api/bar"
        if (LegacyToModernTypeNames.TryGetValue(name.ToLowerInvariant(), out var newTypeName) && Types.TryGetValue(newTypeName, out match))
        {
            return match;
        }

        // Try to match just on the name
        var looseMatch = Types.FirstOrDefault(type =>
            // Look for a match on just the name
            type.Value.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
            // Look for a match on the name with a generic
            || type.Value.Name.Equals(name + "`1", StringComparison.OrdinalIgnoreCase)
            // Look for a match on the name with two generics
            || type.Value.Name.Equals(name + "`2", StringComparison.OrdinalIgnoreCase)
            // .. or the friendly name
            || type.Value.NameFriendly.Equals(name, StringComparison.OrdinalIgnoreCase)).Value;
        if (looseMatch != null)
        {
            return looseMatch;
        }

        // Nothing found        
        return null;
    }

    /// <summary>
    /// Yields documented types whose friendly name matches the given text.
    /// </summary>
    /// <param name="text"></param>
    public static IEnumerable<DocumentedType> FindTypesByFriendlyName(string text)
    {
        return Types.Where(type => type.Value.NameFriendly.Contains(text))
            .Select(pair => pair.Value);
    }

    /// <summary>
    /// Gets a documented property by its name.
    /// </summary>
    /// <param name="name">The name of the property to find.</param>
    public static DocumentedProperty GetProperty(string name)
    {
        // Anything to do?
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }
        // A member's key starts with the key of the type that declares it, so an exact match only needs that one type to be built.
        var match = ApiDocumentationMembers.Property(name) ?? ApiDocumentationMembers.Property("Vek." + name);
        if (match != null)
        {
            return match;
        }
        // Nothing matched by key.  Fall back to a search by name, which needs every type built.
        var byName = Properties.SingleOrDefault(member => member.Value.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        return byName.Value;
    }

    /// <summary>
    /// Gets a documented property by its name.
    /// </summary>
    /// <param name="name">The name of the field to find.</param>
    public static DocumentedField GetField(string name)
    {
        // Anything to do?
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }
        // A member's key starts with the key of the type that declares it, so an exact match only needs that one type to be built.
        var match = ApiDocumentationMembers.Field(name) ?? ApiDocumentationMembers.Field("Vek." + name);
        if (match != null)
        {
            return match;
        }
        // Nothing matched by key.  Fall back to a search by name, which needs every type built.
        var byName = Fields.SingleOrDefault(member => member.Value.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        return byName.Value;
    }

    /// <summary>
    /// Gets a documented method by its name.
    /// </summary>
    /// <param name="name">The name of the method to find.</param>
    public static DocumentedMethod GetMethod(string name)
    {
        // Anything to do?
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }
        // A member's key starts with the key of the type that declares it, so an exact match only needs that one type to be built.
        var match = ApiDocumentationMembers.Method(name) ?? ApiDocumentationMembers.Method("Vek." + name);
        if (match != null)
        {
            return match;
        }
        // Nothing matched by key.  Fall back to a search by name, which needs every type built.
        var byName = Methods.SingleOrDefault(member => member.Value.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        return byName.Value;
    }

    /// <summary>
    /// Gets a documented event by its name.
    /// </summary>
    /// <param name="name">The name of the event to find.</param>
    public static DocumentedEvent GetEvent(string name)
    {
        // Anything to do?
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }
        // A member's key starts with the key of the type that declares it, so an exact match only needs that one type to be built.
        var match = ApiDocumentationMembers.Event(name) ?? ApiDocumentationMembers.Event("Vek." + name);
        if (match != null)
        {
            return match;
        }
        // Nothing matched by key.  Fall back to a search by name, which needs every type built.
        var byName = Events.SingleOrDefault(member => member.Value.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        return byName.Value;
    }

    /// <summary>
    /// A dictionary that maps legacy api links to the new format.
    /// </summary>
    /// <remarks>
    /// This can be removed once it is decided that users are no longer using legacy API links.
    /// </remarks>
    private static readonly Dictionary<string, string> LegacyToModernTypeNames = new()
    {
        { "alert", "Vek.VekAlert" },
        { "appbar", "Vek.VekAppBar" },
        { "avatar", "Vek.VekAvatar" },
        { "avatargroup", "Vek.VekAvatarGroup" },
        { "autocomplete", "Vek.VekAutocomplete`1" },
        { "badge", "Vek.VekBadge" },
        { "bar", "Vek.Charts.Bar`1" },
        { "barchart", "Vek.Charts.Bar`1" },
        { "breadcrumbs", "Vek.VekBreadcrumbs" },
        { "breakpointprovider", "Vek.VekBreakpointProvider" },
        { "button", "Vek.VekButton" },
        { "buttonfab", "Vek.VekFab" },
        { "buttongroup", "Vek.VekButtonGroup" },
        { "card", "Vek.VekCard" },
        { "cardactions", "Vek.VekCardActions" },
        { "cardcontent", "Vek.VekCardContent" },
        { "cardheader", "Vek.VekCardHeader" },
        { "cardmedia", "Vek.VekCardMedia" },
        { "carousel", "Vek.VekCarousel`1" },
        { "carouselitem", "Vek.VekCarouselItem" },
        { "checkbox", "Vek.VekCheckBox`1" },
        { "chips", "Vek.VekChip`1" },
        { "chipset", "Vek.VekChipSet`1" },
        { "collapse", "Vek.VekCollapse" },
        { "colorpicker", "Vek.VekColorPicker" },
        { "container", "Vek.VekContainer" },
        { "datagrid", "Vek.VekDataGrid`1" },
        { "datepicker", "Vek.VekDatePicker" },
        { "daterangepicker", "Vek.VekDateRangePicker" },
        { "dialog", "Vek.VekDialog" },
        { "dialoginstance", "Vek.VekDialogContainer" },
        { "dialogprovider", "Vek.VekDialogProvider" },
        { "divider", "Vek.VekDivider" },
        { "donut", "Vek.Charts.Donut`1" },
        { "donutchart", "Vek.Charts.Donut`1" },
        { "drawer", "Vek.VekDrawer" },
        { "drawercontainer", "Vek.VekDrawerContainer" },
        { "drawerheader", "Vek.VekDrawerHeader" },
        { "dynamictabs", "Vek.VekDynamicTabs" },
        { "element", "Vek.VekElement" },
        { "expansionpanel", "Vek.VekExpansionPanel" },
        { "expansionpanels", "Vek.VekExpansionPanels" },
        { "field", "Vek.VekField" },
        { "fileuploader", "Vek.VekFileUpload`1" },
        { "focustrap", "Vek.VekFocusTrap" },
        { "form", "Vek.VekForm" },
        { "grid", "Vek.VekGrid" },
        { "hidden", "Vek.VekHidden" },
        { "highlighter", "Vek.VekHighlighter" },
        { "iconbutton", "Vek.VekIconButton" },
        { "icons", "Vek.VekIcon" },
        { "input", "Vek.VekInput`1" },
        { "inputcontrol", "Vek.VekInputControl" },
        { "inputlabel", "Vek.VekInputLabel" },
        { "item", "Vek.VekItem" },
        { "legend", "Vek.Charts.Legend`1" },
        { "line", "Vek.Charts.Line`1" },
        { "linechart", "Vek.Charts.Line`1" },
        { "link", "Vek.VekLink" },
        { "list", "Vek.VekList`1" },
        { "listitem", "Vek.VekListItem`1" },
        { "listsubheader", "Vek.VekListSubheader" },
        { "maincontent", "Vek.VekMainContent" },
        { "menu", "Vek.VekMenu" },
        { "menuitem", "Vek.VekMenuItem" },
        { "messagebox", "Vek.VekMessageBox" },
        { "navgroup", "Vek.VekNavGroup" },
        { "navlink", "Vek.VekNavLink" },
        { "navmenu", "Vek.VekNavMenu" },
        { "numericfield", "Vek.VekNumericField`1" },
        { "overlay", "Vek.VekOverlay" },
        { "pagecontentnavigation", "Vek.VekPageContentNavigation" },
        { "pagination", "Vek.VekPagination" },
        { "paper", "Vek.VekPaper" },
        { "pie", "Vek.Charts.Pie`1" },
        { "piechart", "Vek.Charts.Pie`1" },
        { "popover", "Vek.VekPopover" },
        { "progress", "Vek.VekProgressLinear" },
        { "radio", "Vek.VekRadio`1" },
        { "radiogroup", "Vek.VekRadioGroup`1" },
        { "rangeinput", "Vek.VekRangeInput`1" },
        { "rating", "Vek.VekRating" },
        { "ratingitem", "Vek.VekRatingItem" },
        { "rtlprovider", "Vek.VekRTLProvider" },
        { "scrolltotop", "Vek.VekScrollToTop" },
        { "select", "Vek.VekSelect`1" },
        { "selectitem", "Vek.VekSelectItem`1" },
        { "simpletable", "Vek.VekSimpleTable" },
        { "skeleton", "Vek.VekSkeleton" },
        { "slider", "Vek.VekSlider`1" },
        { "snackbar", "Vek.VekSnackbarProvider" },
        { "snackbarelement", "Vek.VekSnackbarElement" },
        { "sparkline", "Vek.Charts.Line`1" },
        { "stackedbar", "Vek.Charts.StackedBar`1" },
        { "swipearea", "Vek.VekSwipeArea" },
        { "switch", "Vek.VekSwitch`1" },
        { "table", "Vek.VekTable`1" },
        { "tablegrouprow", "Vek.VekTableGroupRow`1" },
        { "tablepager", "Vek.VekTablePager" },
        { "tablesortlabel", "Vek.VekTableSortLabel`1" },
        { "tabs", "Vek.VekTabs" },
        { "td", "Vek.VekTd" },
        { "textfield", "Vek.VekTextField`1" },
        { "tfootrow", "Vek.VekTFootRow" },
        { "th", "Vek.VekTh" },
        { "theadrow", "Vek.VekTHeadRow" },
        { "timeline", "Vek.VekTimeline" },
        { "timelineitem", "Vek.VekTimelineItem" },
        { "timepicker", "Vek.VekTimePicker" },
        { "timeseries", "Vek.Charts.TimeSeries`1" },
        { "toggleiconbutton", "Vek.VekToggleIconButton" },
        { "toolbar", "Vek.VekToolBar" },
        { "tooltip", "Vek.VekTooltip" },
        { "tr", "Vek.VekTr" },
        { "treeview", "Vek.VekTreeView`1" },
        { "treeviewitem", "Vek.VekTreeViewItem`1" },
        { "treeviewitemtogglebutton", "Vek.VekTreeViewItemToggleButton" },
        { "typography", "Vek.Typography" },
    };
}
