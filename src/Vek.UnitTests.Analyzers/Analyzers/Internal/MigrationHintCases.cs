// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Vek.UnitTests.Analyzers.Internal;

#nullable enable
/// <summary>
/// A removed parameter written on a concrete component, with the guidance VEK0002 has to append for it.
/// </summary>
/// <param name="ComponentMetadataName">The component a consumer writes, which may derive from the type that declared the removed parameter.</param>
/// <param name="Removed">The removed parameter as written in markup.</param>
/// <param name="Replacement">The current parameter the hint must point at.</param>
/// <param name="ExpectedHint">Fragments the appended hint must contain.</param>
/// <param name="TypeArguments">Type arguments for a generic component, defaulting to <c>string</c> for each.</param>
internal sealed record MigrationHintCase(string ComponentMetadataName, string Removed, string Replacement, IReadOnlyList<string> ExpectedHint, string? TypeArguments = null)
{
    /// <summary>
    /// The display name VEK0002 prints for the component.
    /// </summary>
    internal string TagName
    {
        get
        {
            var name = ComponentMetadataName[(ComponentMetadataName.LastIndexOf('.') + 1)..];
            var tick = name.IndexOf('`');

            return tick < 0 ? name : name[..tick];
        }
    }

    /// <summary>
    /// The component as C# type syntax, closed over <see cref="TypeArguments"/> when it is generic.
    /// </summary>
    internal string TypeSyntax
    {
        get
        {
            var tick = ComponentMetadataName.IndexOf('`');
            if (tick < 0)
                return $"global::{ComponentMetadataName}";

            var arity = int.Parse(ComponentMetadataName[(tick + 1)..]);

            return $"global::{ComponentMetadataName[..tick]}<{TypeArguments ?? string.Join(", ", Enumerable.Repeat("string", arity))}>";
        }
    }

    public override string ToString() => $"{TagName}.{Removed}";
}

/// <summary>
/// Every removed parameter VEK0002 explains, written the way a consumer meets it: on the concrete component, with the wording the reader sees.
/// </summary>
/// <remarks>
/// This list is written from the evidence for each removal rather than generated from the analyzer's catalog, so a catalog entry that points at the wrong replacement, the wrong component, or the wrong direction of an inversion fails here.
/// Base-type registrations are exercised through several concrete descendants.
/// </remarks>
internal static class MigrationHintCases
{
    internal const string V7Guide = "https://github.com/madiseh/vek/discussions/12658";
    internal const string V8Guide = "https://github.com/madiseh/vek/discussions/12659";
    internal const string V9Guide = "https://github.com/madiseh/vek/issues/12666";
    internal const string Docs = "https://github.com/madiseh/vekfeatures/analyzers#migration-hints";

    internal static IReadOnlyList<MigrationHintCase> All { get; } =
    [
        // Buttons
        Renamed("Vek.VekButton", "Link", "Href", "v7", V7Guide),
        Renamed("Vek.VekIconButton", "Link", "Href", "v7", V7Guide),
        Renamed("Vek.VekFab", "Link", "Href", "v7", V7Guide),
        Inverted("Vek.VekButton", "DisableRipple", "Ripple", "v7", V7Guide),
        Inverted("Vek.VekIconButton", "DisableRipple", "Ripple", "v7", V7Guide),
        Inverted("Vek.VekFab", "DisableRipple", "Ripple", "v7", V7Guide),
        Inverted("Vek.VekButton", "DisableElevation", "DropShadow", "v7", V7Guide),
        Inverted("Vek.VekIconButton", "DisableElevation", "DropShadow", "v7", V7Guide),
        Inverted("Vek.VekFab", "DisableElevation", "DropShadow", "v7", V7Guide),
        Renamed("Vek.VekFab", "Icon", "StartIcon", "v7", V7Guide),
        Inverted("Vek.VekToggleIconButton", "DisableRipple", "Ripple", "v7", V7Guide),
        Inverted("Vek.VekToggleIconButton", "DisableElevation", "DropShadow", "v7", V7Guide),
        Inverted("Vek.VekButtonGroup", "DisableElevation", "DropShadow", "v7", V7Guide),
        Renamed("Vek.VekButtonGroup", "VerticalAlign", "Vertical", "v7", V7Guide),

        // Selection controls
        BoundValue("Vek.VekCheckBox`1", "Checked", "Value", "v7", V7Guide),
        BoundCallback("Vek.VekCheckBox`1", "Checked", "Value", "v7", V7Guide),
        Inverted("Vek.VekCheckBox`1", "DisableRipple", "Ripple", "v7", V7Guide),
        LabelPosition("Vek.VekCheckBox`1"),
        BoundValue("Vek.VekSwitch`1", "Checked", "Value", "v7", V7Guide),
        BoundCallback("Vek.VekSwitch`1", "Checked", "Value", "v7", V7Guide),
        Inverted("Vek.VekSwitch`1", "DisableRipple", "Ripple", "v7", V7Guide),
        LabelPosition("Vek.VekSwitch`1"),
        Renamed("Vek.VekRadio`1", "Option", "Value", "v7", V7Guide),
        Inverted("Vek.VekRadio`1", "DisableRipple", "Ripple", "v7", V7Guide),
        Manual("Vek.VekRadio`1", "Placement", "LabelPlacement",
            "'Placement' was removed in v8 and replaced by 'LabelPlacement', which takes the same Placement value.",
            "Placement.Left and Placement.Right no longer swap sides in right-to-left layouts, so use Placement.Start or Placement.End there.",
            $"See {V8Guide}"),
        BoundValue("Vek.VekRadioGroup`1", "SelectedOption", "Value", "v7", V7Guide),
        BoundCallback("Vek.VekRadioGroup`1", "SelectedOption", "Value", "v7", V7Guide),
        Inverted("Vek.VekToggleGroup`1", "DisableRipple", "Ripple", "v7", V7Guide),
        Renamed("Vek.VekToggleGroup`1", "Outline", "Outlined", "v7", V7Guide),
        InvertedWithNewDefault("Vek.VekRating", "DisableRipple", "Ripple", "VekRating", "v7", Docs),
        InvertedWithNewDefault("Vek.VekRatingItem", "DisableRipple", "Ripple", "VekRatingItem", "v7", Docs),

        // Chips, lists, and menus
        Renamed("Vek.VekChip`1", "Link", "Href", "v7", V7Guide),
        InvertedFollowingParent("Vek.VekChip`1", "DisableRipple", "Ripple", "VekChipSet", "v7", V7Guide),
        Renamed("Vek.VekChipSet`1", "Filter", "CheckMark", "v7", V7Guide),
        InvertedWithNewDefault("Vek.VekList`1", "DisablePadding", "Padding", "VekList", "v7", V7Guide),
        InvertedWithNewDefault("Vek.VekList`1", "Clickable", "ReadOnly", "VekList", "v7", V7Guide),
        Inverted("Vek.VekList`1", "DisableGutters", "Gutters", "v7", V7Guide),
        Inverted("Vek.VekListItem`1", "DisableRipple", "Ripple", "v7", V7Guide),
        InvertedFollowingParent("Vek.VekListItem`1", "DisableGutters", "Gutters", "VekList", "v7", V7Guide),
        Renamed("Vek.VekListItem`1", "AdornmentColor", "ExpandIconColor", "v7", V7Guide),
        Renamed("Vek.VekListItem`1", "OnClickHandlerPreventDefault", "OnClickPreventDefault", "v7", V7Guide),
        InitialState("Vek.VekListItem`1", "InitiallyExpanded", "Expanded", "v7", V7Guide),
        Inverted("Vek.VekListSubheader", "DisableGutters", "Gutters", "v7", V7Guide),
        Inverted("Vek.VekMenu", "DisableRipple", "Ripple", "v7", V7Guide),
        Inverted("Vek.VekMenu", "DisableElevation", "DropShadow", "v7", V7Guide),
        Renamed("Vek.VekMenu", "IsOpenChanged", "OpenChanged", "v7", V7Guide),
        Renamed("Vek.VekMenu", "PositionAtCurser", "PositionAtCursor", "v7", Docs),
        Renamed("Vek.VekMenuItem", "Link", "Href", "v7", V7Guide),

        // Alerts, dialogs, tooltips, and layout
        Manual("Vek.VekAlert", "AlertTextPosition", "ContentAlignment",
            "'AlertTextPosition' was removed in v7 and replaced by 'ContentAlignment', which takes a HorizontalAlignment with the same member names instead of an AlertTextPosition:",
            "AlertTextPosition=\"AlertTextPosition.Center\" becomes ContentAlignment=\"HorizontalAlignment.Center\"",
            $"See {V7Guide}"),
        BoundValue("Vek.VekDialog", "IsVisible", "Visible", "v7", V7Guide),
        BoundCallback("Vek.VekDialog", "IsVisible", "Visible", "v7", V7Guide),
        Inverted("Vek.VekDialog", "DisableSidePadding", "Gutters", "v7", V7Guide),
        Renamed("Vek.VekDialog", "ClassActions", "ActionsClass", "v7", Docs),
        Renamed("Vek.VekDialog", "ClassContent", "ContentClass", "v7", V7Guide),
        BoundValue("Vek.VekMessageBox", "IsVisible", "Visible", "v7", V7Guide),
        BoundCallback("Vek.VekMessageBox", "IsVisible", "Visible", "v7", V7Guide),
        BoundValue("Vek.VekTooltip", "IsVisible", "Visible", "v7", V7Guide),
        BoundCallback("Vek.VekTooltip", "IsVisible", "Visible", "v7", V7Guide),
        Manual("Vek.VekTooltip", "Delayed", "Delay",
            "'Delayed' was removed in v7 and replaced by 'Delay', which is in milliseconds instead of seconds:",
            "Delayed=\"0.5\" becomes Delay=\"500\"",
            "Delayed=\"@seconds\" becomes Delay=\"@(seconds * 1000)\"",
            $"See {Docs}"),
        BoundValue("Vek.VekHidden", "IsHidden", "Hidden", "v7", V7Guide),
        BoundCallback("Vek.VekHidden", "IsHidden", "Hidden", "v7", V7Guide),
        BoundValue("Vek.VekExpansionPanel", "IsExpanded", "Expanded", "v7", V7Guide),
        BoundCallback("Vek.VekExpansionPanel", "IsExpanded", "Expanded", "v7", V7Guide),
        Inverted("Vek.VekExpansionPanel", "DisableGutters", "Gutters", "v7", V7Guide),
        InitialState("Vek.VekExpansionPanel", "IsInitiallyExpanded", "Expanded", "v7", V7Guide),
        Inverted("Vek.VekExpansionPanels", "DisableBorders", "Outlined", "v7", V7Guide),
        Inverted("Vek.VekExpansionPanels", "DisableGutters", "Gutters", "v7", V7Guide),
        Inverted("Vek.VekAppBar", "DisableGutters", "Gutters", "v7", V7Guide),
        Inverted("Vek.VekToolBar", "DisableGutters", "Gutters", "v7", V7Guide),
        Inverted("Vek.VekDrawer", "DisableOverlay", "Overlay", "v7", Docs),

        // Inputs and pickers
        Inverted("Vek.VekTextField`1", "DisableUnderLine", "Underline", "v7", V7Guide),
        Inverted("Vek.VekNumericField`1", "DisableUnderLine", "Underline", "v7", V7Guide),
        Inverted("Vek.VekSelect`1", "DisableUnderLine", "Underline", "v7", V7Guide),
        Inverted("Vek.VekAutocomplete`1", "DisableUnderLine", "Underline", "v7", V7Guide),
        OnKeyPress("Vek.VekTextField`1"),
        OnKeyPress("Vek.VekNumericField`1"),
        Inverted("Vek.VekField", "DisableUnderLine", "Underline", "v7", V7Guide),
        Manual("Vek.VekTextField`1", "AutoGrow", "Sizing", MigrationHintExpectations.AutoGrow),
        Manual("Vek.VekInput`1", "AutoGrow", "Sizing",
            "'AutoGrow' was removed in v9 and replaced by 'Sizing':",
            "AutoGrow=\"true\" becomes Sizing=\"InputSizing.Auto\"",
            "AutoGrow=\"false\" becomes Sizing=\"InputSizing.Fixed\"",
            "AutoGrow=\"@expression\" becomes Sizing=\"@(expression ? InputSizing.Auto : InputSizing.Fixed)\"",
            "Lines and MaxLines keep their meaning.",
            $"See {Docs}"),
        Renamed("Vek.VekAutocomplete`1", "IsOpenChanged", "OpenChanged", "v7", V7Guide),
        Renamed("Vek.VekAutocomplete`1", "SelectOnClick", "SelectOnActivation", "v7", V7Guide),
        Manual("Vek.VekAutocomplete`1", "SearchFuncWithCancel", "SearchFunc",
            "'SearchFuncWithCancel' was removed in v7 and replaced by 'SearchFunc', which now takes the same cancellable delegate:",
            "SearchFuncWithCancel=\"Search\" becomes SearchFunc=\"Search\"",
            "Remove any old SearchFunc that has no CancellationToken parameter",
            $"See {V7Guide}"),
        Manual("Vek.VekSelect`1", "OnOpen", "OpenChanged",
            "'OnOpen' was removed in v9. Use 'OpenChanged', which receives true when the list opens and false when it closes:",
            "OnOpen=\"Handler\" becomes OpenChanged=\"@(open => { if (open) Handler(); })\"",
            "or OpenChanged=\"@(async open => { if (open) await Handler(); })\" when Handler returns a Task",
            "raised only when the open state actually changes",
            $"See {Docs}"),
        Manual("Vek.VekSelect`1", "OnClose", "OpenChanged",
            "'OnClose' was removed in v9. Use 'OpenChanged', which receives true when the list opens and false when it closes:",
            "OnClose=\"Handler\" becomes OpenChanged=\"@(open => { if (!open) Handler(); })\"",
            "or OpenChanged=\"@(async open => { if (!open) await Handler(); })\" when Handler returns a Task",
            "a single OpenChanged handler for both",
            $"See {Docs}"),
        Inverted("Vek.VekDatePicker", "DisableToolbar", "ShowToolbar", "v7", V7Guide),
        Inverted("Vek.VekDateRangePicker", "DisableToolbar", "ShowToolbar", "v7", V7Guide),
        Inverted("Vek.VekTimePicker", "DisableToolbar", "ShowToolbar", "v7", V7Guide),
        Inverted("Vek.VekColorPicker", "DisableToolbar", "ShowToolbar", "v7", V7Guide),
        Inverted("Vek.VekDatePicker", "DisableUnderLine", "Underline", "v7", V7Guide),
        Renamed("Vek.VekDatePicker", "InputIcon", "AdornmentIcon", "v7", V7Guide),
        Renamed("Vek.VekDatePicker", "InputVariant", "Variant", "v7", V7Guide),
        Renamed("Vek.VekTimePicker", "ClassActions", "ActionsClass", "v7", Docs),
        Inverted("Vek.VekPickerToolbar", "DisableToolbar", "ShowToolbar", "v7", V7Guide),
        Inverted("Vek.VekColorPicker", "DisableSliders", "ShowSliders", "v7", V7Guide),
        Inverted("Vek.VekColorPicker", "DisablePreview", "ShowPreview", "v7", V7Guide),
        Inverted("Vek.VekColorPicker", "DisableModeSwitch", "ShowModeSwitch", "v7", V7Guide),
        Inverted("Vek.VekColorPicker", "DisableInputs", "ShowInputs", "v7", V7Guide),
        Inverted("Vek.VekColorPicker", "DisableDragEffect", "DragEffect", "v7", V7Guide),
        Inverted("Vek.VekColorPicker", "DisableColorField", "ShowColorField", "v7", V7Guide),
        Inverted("Vek.VekColorPicker", "DisableAlpha", "ShowAlpha", "v7", V7Guide),

        // Navigation, tabs, and progress
        Inverted("Vek.VekNavGroup", "DisableRipple", "Ripple", "v7", V7Guide),
        Inverted("Vek.VekNavLink", "DisableRipple", "Ripple", "v7", V7Guide),
        Inverted("Vek.VekTabs", "DisableRipple", "Ripple", "v7", V7Guide),
        Inverted("Vek.VekTabs", "DisableSliderAnimation", "SliderAnimation", "v7", V7Guide),
        Manual("Vek.VekTabs", "TabPanelClass", "TabButtonsClass",
            "'TabPanelClass' was removed in v9 and replaced by 'TabButtonsClass', which takes the same value and still styles each tab button.",
            "TabPanelsClass and VekTabPanel's PanelClass style the panels instead.",
            $"See {V9Guide}"),
        Manual("Vek.VekTabs", "PanelClass", "TabPanelsClass",
            "'PanelClass' was removed in v9 and replaced by 'TabPanelsClass', which takes the same value and still styles the wrapper around all panels.",
            "VekTabPanel's own PanelClass is a different parameter that styles one panel.",
            $"See {V9Guide}"),
        Manual("Vek.VekDynamicTabs", "PanelClass", "TabPanelsClass",
            "'PanelClass' was removed in v9 and replaced by 'TabPanelsClass'",
            $"See {V9Guide}"),
        Inverted("Vek.VekTimeline", "DisableModifiers", "Modifiers", "v7", V7Guide),
        Renamed("Vek.VekProgressCircular", "Minimum", "Min", "v7", V7Guide),
        Renamed("Vek.VekProgressCircular", "Maximum", "Max", "v7", V7Guide),
        Renamed("Vek.VekProgressLinear", "Minimum", "Min", "v7", V7Guide),
        Renamed("Vek.VekProgressLinear", "Maximum", "Max", "v7", V7Guide),
        Renamed("Vek.VekCarousel`1", "ShowDelimiters", "ShowBullets", "v7", V7Guide),
        Renamed("Vek.VekCarousel`1", "DelimitersColor", "BulletsColor", "v7", V7Guide),
        Renamed("Vek.VekCarousel`1", "DelimitersClass", "BulletsClass", "v7", Docs),
        Renamed("Vek.VekCarousel`1", "DelimiterTemplate", "BulletTemplate", "v7", Docs),
        Inverted("Vek.VekPagination", "DisableElevation", "DropShadow", "v7", V7Guide),

        // Tables, data grids, trees, drag and drop, and charts
        Manual("Vek.VekTablePager", "DisableRowsPerPage", "HideRowsPerPage",
            "'DisableRowsPerPage' was removed in v7 and replaced by 'HideRowsPerPage', which keeps the same value:",
            "DisableRowsPerPage=\"true\" becomes HideRowsPerPage=\"true\"",
            "Do not invert it",
            $"See {Docs}"),
        Inverted("Vek.VekDataGridPager`1", "DisableRowsPerPage", "PageSizeSelector", "v7", V7Guide),
        Renamed("Vek.VekDataGrid`1", "CancelledEditingItem", "CanceledEditingItem", "v8", V8Guide),
        Renamed("Vek.PropertyColumn`2", "IsEditable", "Editable", "v7", V7Guide),
        Renamed("Vek.TemplateColumn`1", "IsEditable", "Editable", "v7", V7Guide),
        Renamed("Vek.VekTr", "IsCheckable", "Checkable", "v7", V7Guide),
        BoundValue("Vek.VekTr", "IsChecked", "Checked", "v7", V7Guide),
        BoundCallback("Vek.VekTr", "IsChecked", "Checked", "v7", V7Guide),
        Renamed("Vek.VekTr", "IsExpandable", "Expandable", "v7", V7Guide),
        Renamed("Vek.VekTr", "IsEditable", "Editable", "v7", V7Guide),
        Renamed("Vek.VekTHeadRow", "IsCheckable", "Checkable", "v7", V7Guide),
        Renamed("Vek.VekTHeadRow", "IsExpandable", "Expandable", "v7", V7Guide),
        Renamed("Vek.VekTFootRow", "IsCheckable", "Checkable", "v7", V7Guide),
        Renamed("Vek.VekTFootRow", "IsExpandable", "Expandable", "v7", V7Guide),
        Renamed("Vek.VekTableGroupRow`1", "IsCheckable", "Checkable", "v7", V7Guide),
        Renamed("Vek.VekTreeView`1", "CanHover", "Hover", "v7", V7Guide),
        Renamed("Vek.VekTreeView`1", "ActivatedValueChanged", "SelectedValueChanged", "v7", V7Guide),
        Manual("Vek.VekTreeViewItem`1", "ExpandedIcon", "ExpandButtonIcon",
            "'ExpandedIcon' was removed in v7 and replaced by 'ExpandButtonIcon', which takes the same value and still sets the expand button's icon.",
            "It is not IconExpanded, which replaces the item's own icon while the item is expanded.",
            $"See {V7Guide}"),
        Renamed("Vek.VekTreeViewItem`1", "ExpandedIconColor", "ExpandButtonIconColor", "v7", Docs),
        Renamed("Vek.VekDropZone`1", "ItemIsDisabled", "ItemDisabled", "v7", V7Guide),
        Renamed("Vek.VekDropContainer`1", "ItemIsDisabled", "ItemDisabled", "v7", V7Guide),
        Renamed("Vek.VekChart`1", "XAxisLabels", "ChartLabels", "v9", V9Guide) with { TypeArguments = "double" },
        Renamed("Vek.VekChart`1", "InputLabels", "ChartLabels", "v9", V9Guide) with { TypeArguments = "double" },
    ];

    private static MigrationHintCase Renamed(string component, string removed, string replacement, string version, string url) =>
        new(component, removed, replacement,
        [
            $"'{removed}' was removed in {version} and replaced by '{replacement}', which takes the same value.",
            $"See {url}"
        ]);

    private static MigrationHintCase Inverted(string component, string removed, string replacement, string version, string url) =>
        new(component, removed, replacement, InversionFragments(removed, replacement, version, url));

    private static MigrationHintCase InvertedWithNewDefault(string component, string removed, string replacement, string owner, string version, string url) =>
        new(component, removed, replacement,
        [
            .. InversionFragments(removed, replacement, version, url),
            $"The default changed as well, so a {owner} that never set '{removed}' now needs {replacement}=\"true\" to keep its old behavior."
        ]);

    private static MigrationHintCase InvertedFollowingParent(string component, string removed, string replacement, string parent, string version, string url) =>
        new(component, removed, replacement,
        [
            $"'{removed}' was removed in {version} and replaced by '{replacement}', which inverts the value:",
            $"{removed}=\"true\" becomes {replacement}=\"false\"",
            $"{removed}=\"@expression\" becomes {replacement}=\"@(expression ? false : null)\"",
            $"For {removed}=\"false\", leave '{replacement}' unset so it follows the parent {parent}, because {replacement}=\"true\" now overrides the parent.",
            $"See {url}"
        ]);

    private static MigrationHintCase BoundValue(string component, string removed, string replacement, string version, string url) =>
        new(component, removed, replacement,
        [
            $"'{removed}' was removed in {version} and replaced by '{replacement}', which takes the same value.",
            $"Its partner '{removed}Changed' became '{replacement}Changed', so @bind-{removed} becomes @bind-{replacement}.",
            $"See {url}"
        ]);

    private static MigrationHintCase BoundCallback(string component, string removedValue, string replacementValue, string version, string url) =>
        new(component, $"{removedValue}Changed", $"{replacementValue}Changed",
        [
            $"'{removedValue}Changed' was removed in {version} and replaced by '{replacementValue}Changed', which takes the same value.",
            $"Its partner '{removedValue}' became '{replacementValue}', so @bind-{removedValue} becomes @bind-{replacementValue}.",
            $"See {url}"
        ]);

    private static MigrationHintCase InitialState(string component, string removed, string replacement, string version, string url) =>
        new(component, removed, replacement,
        [
            $"'{removed}' was removed in {version} and replaced by '{replacement}': {removed}=\"true\" becomes {replacement}=\"true\".",
            $"'{replacement}' holds the current state rather than a starting value, so merge it with any existing {replacement} or @bind-{replacement} instead of adding a second attribute.",
            $"See {url}"
        ]);

    private static MigrationHintCase LabelPosition(string component) =>
        Manual(component, "LabelPosition", "LabelPlacement",
            "'LabelPosition' was removed in v8 and replaced by 'LabelPlacement', which takes a Placement instead of a LabelPosition:",
            "LabelPosition=\"LabelPosition.Start\" becomes LabelPlacement=\"Placement.Start\"",
            "LabelPosition=\"LabelPosition.End\" becomes LabelPlacement=\"Placement.End\"",
            $"See {V8Guide}");

    private static MigrationHintCase OnKeyPress(string component) =>
        Manual(component, "OnKeyPress", "OnKeyDown",
            "'OnKeyPress' was removed in v7. Use 'OnKeyDown', which receives the same KeyboardEventArgs but also fires for keys that type no character, such as Tab, Shift, and the arrow keys, so check the key in the handler.",
            $"See {V7Guide}");

    private static MigrationHintCase Manual(string component, string removed, string replacement, params string[] fragments) =>
        new(component, removed, replacement, fragments);

    private static string[] InversionFragments(string removed, string replacement, string version, string url) =>
    [
        $"'{removed}' was removed in {version} and replaced by '{replacement}', which inverts the value:",
        $"{removed}=\"true\" becomes {replacement}=\"false\"",
        $"{removed}=\"false\" becomes {replacement}=\"true\"",
        $"{removed}=\"@expression\" becomes {replacement}=\"@(!expression)\"",
        $"See {url}"
    ];
}
#nullable restore
