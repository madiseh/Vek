// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Immutable;

namespace Vek.Analyzers.Internal;

/// <summary>
/// A component parameter that an earlier Vek major version removed, together with the guidance <c>VEK0002</c> appends when the old name still appears on the component that used to declare it.
/// </summary>
/// <remarks>
/// This is a lookup table, not a migration tool.
/// The analyzer never reads, evaluates, or rewrites the attribute's value; it only explains what the replacement parameter is so the reader does not have to search the migration guides.
/// </remarks>
internal sealed class ParameterMigration
{
    private const string V7Guide = "https://github.com/madiseh/vek/discussions/12658";
    private const string V8Guide = "https://github.com/madiseh/vek/discussions/12659";
    private const string V9Guide = "https://github.com/madiseh/vek/issues/12666";

    // Used when the guide for that release does not cover the removal, or describes it wrongly.
    private const string Docs = "https://github.com/madiseh/vekfeatures/analyzers#migration-hints";

    private ParameterMigration(string componentMetadataName, string parameterName, string replacementParameterName, ParameterMigrationKind kind, LocalizableString hint)
    {
        ComponentMetadataName = componentMetadataName;
        ParameterName = parameterName;
        ReplacementParameterName = replacementParameterName;
        Kind = kind;
        Hint = hint;
    }

    /// <summary>
    /// The fully qualified metadata name of the component that declared the removed parameter.
    /// </summary>
    /// <remarks>
    /// Resolved through the compilation so the hint follows type identity rather than the display tag name, which several unrelated components share.
    /// A base type is only used when every current component deriving from it has the replacement, so the advice holds for all of them.
    /// Some of those components were added after the removal and never had the old parameter, such as VekFabMenu, VekContextualActionBar, and SelectColumn.
    /// </remarks>
    internal string ComponentMetadataName { get; }

    /// <summary>
    /// The removed parameter's name, matched case-insensitively like the rest of VEK0002.
    /// </summary>
    internal string ParameterName { get; }

    /// <summary>
    /// The current parameter the hint points at.
    /// </summary>
    internal string ReplacementParameterName { get; }

    /// <summary>
    /// How the value carries over, which the catalog tests use to check the hint against the real replacement.
    /// </summary>
    internal ParameterMigrationKind Kind { get; }

    /// <summary>
    /// The replacement guidance appended to the diagnostic message.
    /// </summary>
    internal LocalizableString Hint { get; }

    /// <summary>
    /// Every removed parameter VEK0002 can explain.
    /// </summary>
    /// <remarks>
    /// Each entry is checked against the source of the release that removed the parameter, not only against the migration guide, because the guides name the wrong component or conversion in several places.
    /// Removals that need more than an in-place change, such as a parameter that moved to a parent component or a set of booleans that became one enum, are left to the guides.
    /// </remarks>
    internal static ImmutableArray<ParameterMigration> All { get; } = ImmutableArray.Create(
        // Buttons
        Rename("Vek.VekBaseButton", "Link", "Href", "v7", V7Guide),
        Inversion("Vek.VekBaseButton", "DisableRipple", "Ripple", "v7", V7Guide),
        Inversion("Vek.VekBaseButton", "DisableElevation", "DropShadow", "v7", V7Guide),
        Rename("Vek.VekFab", "Icon", "StartIcon", "v7", V7Guide),
        Inversion("Vek.VekToggleIconButton", "DisableRipple", "Ripple", "v7", V7Guide),
        Inversion("Vek.VekToggleIconButton", "DisableElevation", "DropShadow", "v7", V7Guide),
        Inversion("Vek.VekButtonGroup", "DisableElevation", "DropShadow", "v7", V7Guide),
        Rename("Vek.VekButtonGroup", "VerticalAlign", "Vertical", "v7", V7Guide),

        // Selection controls
        // Checked is registered per component because VekRadio shares their base today but never had a public Checked.
        BindingValue("Vek.VekCheckBox`1", "Checked", "Value", "v7", V7Guide),
        BindingCallback("Vek.VekCheckBox`1", "Checked", "Value", "v7", V7Guide),
        Inversion("Vek.VekCheckBox`1", "DisableRipple", "Ripple", "v7", V7Guide),
        Manual("Vek.VekCheckBox`1", "LabelPosition", "LabelPlacement", nameof(Resources.VEK0002MigrationHintLabelPosition)),
        BindingValue("Vek.VekSwitch`1", "Checked", "Value", "v7", V7Guide),
        BindingCallback("Vek.VekSwitch`1", "Checked", "Value", "v7", V7Guide),
        Inversion("Vek.VekSwitch`1", "DisableRipple", "Ripple", "v7", V7Guide),
        Manual("Vek.VekSwitch`1", "LabelPosition", "LabelPlacement", nameof(Resources.VEK0002MigrationHintLabelPosition)),
        Rename("Vek.VekRadio`1", "Option", "Value", "v7", V7Guide),
        Inversion("Vek.VekRadio`1", "DisableRipple", "Ripple", "v7", V7Guide),
        Manual("Vek.VekRadio`1", "Placement", "LabelPlacement", nameof(Resources.VEK0002MigrationHintVekRadioPlacement)),
        BindingValue("Vek.VekRadioGroup`1", "SelectedOption", "Value", "v7", V7Guide),
        BindingCallback("Vek.VekRadioGroup`1", "SelectedOption", "Value", "v7", V7Guide),
        Inversion("Vek.VekToggleGroup`1", "DisableRipple", "Ripple", "v7", V7Guide),
        Rename("Vek.VekToggleGroup`1", "Outline", "Outlined", "v7", V7Guide),
        InversionWithNewDefault("Vek.VekRating", "DisableRipple", "Ripple", "VekRating", "v7", Docs),
        InversionWithNewDefault("Vek.VekRatingItem", "DisableRipple", "Ripple", "VekRatingItem", "v7", Docs),

        // Chips, lists, and menus
        Rename("Vek.VekChip`1", "Link", "Href", "v7", V7Guide),
        InheritedInversion("Vek.VekChip`1", "DisableRipple", "Ripple", "VekChipSet", "v7", V7Guide),
        Rename("Vek.VekChipSet`1", "Filter", "CheckMark", "v7", V7Guide),
        InversionWithNewDefault("Vek.VekList`1", "DisablePadding", "Padding", "VekList", "v7", V7Guide),
        InversionWithNewDefault("Vek.VekList`1", "Clickable", "ReadOnly", "VekList", "v7", V7Guide),
        Inversion("Vek.VekList`1", "DisableGutters", "Gutters", "v7", V7Guide),
        Inversion("Vek.VekListItem`1", "DisableRipple", "Ripple", "v7", V7Guide),
        InheritedInversion("Vek.VekListItem`1", "DisableGutters", "Gutters", "VekList", "v7", V7Guide),
        Rename("Vek.VekListItem`1", "AdornmentColor", "ExpandIconColor", "v7", V7Guide),
        Rename("Vek.VekListItem`1", "OnClickHandlerPreventDefault", "OnClickPreventDefault", "v7", V7Guide),
        InitialState("Vek.VekListItem`1", "InitiallyExpanded", "Expanded", "v7", V7Guide),
        Inversion("Vek.VekListSubheader", "DisableGutters", "Gutters", "v7", V7Guide),
        Inversion("Vek.VekMenu", "DisableRipple", "Ripple", "v7", V7Guide),
        Inversion("Vek.VekMenu", "DisableElevation", "DropShadow", "v7", V7Guide),
        // IsOpen was never a parameter, so only the callback is a migration.
        Rename("Vek.VekMenu", "IsOpenChanged", "OpenChanged", "v7", V7Guide),
        Rename("Vek.VekMenu", "PositionAtCurser", "PositionAtCursor", "v7", Docs),
        Rename("Vek.VekMenuItem", "Link", "Href", "v7", V7Guide),

        // Alerts, dialogs, tooltips, and layout
        Manual("Vek.VekAlert", "AlertTextPosition", "ContentAlignment", nameof(Resources.VEK0002MigrationHintVekAlertAlertTextPosition)),
        BindingValue("Vek.VekDialog", "IsVisible", "Visible", "v7", V7Guide),
        BindingCallback("Vek.VekDialog", "IsVisible", "Visible", "v7", V7Guide),
        Inversion("Vek.VekDialog", "DisableSidePadding", "Gutters", "v7", V7Guide),
        Rename("Vek.VekDialog", "ClassActions", "ActionsClass", "v7", Docs),
        Rename("Vek.VekDialog", "ClassContent", "ContentClass", "v7", V7Guide),
        BindingValue("Vek.VekMessageBox", "IsVisible", "Visible", "v7", V7Guide),
        BindingCallback("Vek.VekMessageBox", "IsVisible", "Visible", "v7", V7Guide),
        BindingValue("Vek.VekTooltip", "IsVisible", "Visible", "v7", V7Guide),
        BindingCallback("Vek.VekTooltip", "IsVisible", "Visible", "v7", V7Guide),
        Manual("Vek.VekTooltip", "Delayed", "Delay", nameof(Resources.VEK0002MigrationHintVekTooltipDelayed)),
        BindingValue("Vek.VekHidden", "IsHidden", "Hidden", "v7", V7Guide),
        BindingCallback("Vek.VekHidden", "IsHidden", "Hidden", "v7", V7Guide),
        BindingValue("Vek.VekExpansionPanel", "IsExpanded", "Expanded", "v7", V7Guide),
        BindingCallback("Vek.VekExpansionPanel", "IsExpanded", "Expanded", "v7", V7Guide),
        Inversion("Vek.VekExpansionPanel", "DisableGutters", "Gutters", "v7", V7Guide),
        InitialState("Vek.VekExpansionPanel", "IsInitiallyExpanded", "Expanded", "v7", V7Guide),
        Inversion("Vek.VekExpansionPanels", "DisableBorders", "Outlined", "v7", V7Guide),
        Inversion("Vek.VekExpansionPanels", "DisableGutters", "Gutters", "v7", V7Guide),
        Inversion("Vek.VekAppBar", "DisableGutters", "Gutters", "v7", V7Guide),
        Inversion("Vek.VekToolBar", "DisableGutters", "Gutters", "v7", V7Guide),
        // The v7 guide lists this under VekDialog, but only VekDrawer ever had it.
        Inversion("Vek.VekDrawer", "DisableOverlay", "Overlay", "v7", Docs),

        // Inputs and pickers
        Inversion("Vek.VekBaseInput`1", "DisableUnderLine", "Underline", "v7", V7Guide),
        Manual("Vek.VekBaseInput`1", "OnKeyPress", "OnKeyDown", nameof(Resources.VEK0002MigrationHintVekBaseInputOnKeyPress)),
        Inversion("Vek.VekField", "DisableUnderLine", "Underline", "v7", V7Guide),
        Manual("Vek.VekTextField`1", "AutoGrow", "Sizing", nameof(Resources.VEK0002MigrationHintVekTextFieldAutoGrow)),
        Manual("Vek.VekInput`1", "AutoGrow", "Sizing", nameof(Resources.VEK0002MigrationHintVekInputAutoGrow)),
        Rename("Vek.VekAutocomplete`1", "IsOpenChanged", "OpenChanged", "v7", V7Guide),
        Rename("Vek.VekAutocomplete`1", "SelectOnClick", "SelectOnActivation", "v7", V7Guide),
        Manual("Vek.VekAutocomplete`1", "SearchFuncWithCancel", "SearchFunc", nameof(Resources.VEK0002MigrationHintVekAutocompleteSearchFuncWithCancel)),
        Manual("Vek.VekSelect`1", "OnOpen", "OpenChanged", nameof(Resources.VEK0002MigrationHintVekSelectOnOpen)),
        Manual("Vek.VekSelect`1", "OnClose", "OpenChanged", nameof(Resources.VEK0002MigrationHintVekSelectOnClose)),
        Inversion("Vek.VekPicker`1", "DisableToolbar", "ShowToolbar", "v7", V7Guide),
        Inversion("Vek.VekPicker`1", "DisableUnderLine", "Underline", "v7", V7Guide),
        Rename("Vek.VekPicker`1", "InputIcon", "AdornmentIcon", "v7", V7Guide),
        Rename("Vek.VekPicker`1", "InputVariant", "Variant", "v7", V7Guide),
        Rename("Vek.VekPicker`1", "ClassActions", "ActionsClass", "v7", Docs),
        Inversion("Vek.VekPickerToolbar", "DisableToolbar", "ShowToolbar", "v7", V7Guide),
        Inversion("Vek.VekColorPicker", "DisableSliders", "ShowSliders", "v7", V7Guide),
        Inversion("Vek.VekColorPicker", "DisablePreview", "ShowPreview", "v7", V7Guide),
        Inversion("Vek.VekColorPicker", "DisableModeSwitch", "ShowModeSwitch", "v7", V7Guide),
        Inversion("Vek.VekColorPicker", "DisableInputs", "ShowInputs", "v7", V7Guide),
        Inversion("Vek.VekColorPicker", "DisableDragEffect", "DragEffect", "v7", V7Guide),
        Inversion("Vek.VekColorPicker", "DisableColorField", "ShowColorField", "v7", V7Guide),
        Inversion("Vek.VekColorPicker", "DisableAlpha", "ShowAlpha", "v7", V7Guide),

        // Navigation, tabs, and progress
        Inversion("Vek.VekNavGroup", "DisableRipple", "Ripple", "v7", V7Guide),
        Inversion("Vek.VekNavLink", "DisableRipple", "Ripple", "v7", V7Guide),
        Inversion("Vek.VekTabs", "DisableRipple", "Ripple", "v7", V7Guide),
        Inversion("Vek.VekTabs", "DisableSliderAnimation", "SliderAnimation", "v7", V7Guide),
        Manual("Vek.VekTabs", "TabPanelClass", "TabButtonsClass", nameof(Resources.VEK0002MigrationHintVekTabsTabPanelClass)),
        // VekTabPanel does not derive from VekTabs, so its own current PanelClass is untouched.
        Manual("Vek.VekTabs", "PanelClass", "TabPanelsClass", nameof(Resources.VEK0002MigrationHintVekTabsPanelClass)),
        Inversion("Vek.VekTimeline", "DisableModifiers", "Modifiers", "v7", V7Guide),
        Rename("Vek.VekProgressCircular", "Minimum", "Min", "v7", V7Guide),
        Rename("Vek.VekProgressCircular", "Maximum", "Max", "v7", V7Guide),
        Rename("Vek.VekProgressLinear", "Minimum", "Min", "v7", V7Guide),
        Rename("Vek.VekProgressLinear", "Maximum", "Max", "v7", V7Guide),
        Rename("Vek.VekCarousel`1", "ShowDelimiters", "ShowBullets", "v7", V7Guide),
        Rename("Vek.VekCarousel`1", "DelimitersColor", "BulletsColor", "v7", V7Guide),
        Rename("Vek.VekCarousel`1", "DelimitersClass", "BulletsClass", "v7", Docs),
        Rename("Vek.VekCarousel`1", "DelimiterTemplate", "BulletTemplate", "v7", Docs),
        Inversion("Vek.VekPagination", "DisableElevation", "DropShadow", "v7", V7Guide),

        // Tables, data grids, trees, drag and drop, and charts
        // Unlike VekDataGridPager, this pager keeps the value.
        // The v7 guide's generic inversion bullet for this name only applies to VekDataGridPager, so the hint links the docs, which show both pagers side by side.
        Manual("Vek.VekTablePager", "DisableRowsPerPage", "HideRowsPerPage", nameof(Resources.VEK0002MigrationHintVekTablePagerDisableRowsPerPage)),
        Inversion("Vek.VekDataGridPager`1", "DisableRowsPerPage", "PageSizeSelector", "v7", V7Guide),
        Rename("Vek.VekDataGrid`1", "CancelledEditingItem", "CanceledEditingItem", "v8", V8Guide),
        Rename("Vek.PropertyColumn`2", "IsEditable", "Editable", "v7", V7Guide),
        Rename("Vek.TemplateColumn`1", "IsEditable", "Editable", "v7", V7Guide),
        Rename("Vek.VekTr", "IsCheckable", "Checkable", "v7", V7Guide),
        BindingValue("Vek.VekTr", "IsChecked", "Checked", "v7", V7Guide),
        BindingCallback("Vek.VekTr", "IsChecked", "Checked", "v7", V7Guide),
        Rename("Vek.VekTr", "IsExpandable", "Expandable", "v7", V7Guide),
        Rename("Vek.VekTr", "IsEditable", "Editable", "v7", V7Guide),
        Rename("Vek.VekTHeadRow", "IsCheckable", "Checkable", "v7", V7Guide),
        Rename("Vek.VekTHeadRow", "IsExpandable", "Expandable", "v7", V7Guide),
        Rename("Vek.VekTFootRow", "IsCheckable", "Checkable", "v7", V7Guide),
        Rename("Vek.VekTFootRow", "IsExpandable", "Expandable", "v7", V7Guide),
        Rename("Vek.VekTableGroupRow`1", "IsCheckable", "Checkable", "v7", V7Guide),
        Rename("Vek.VekTreeView`1", "CanHover", "Hover", "v7", V7Guide),
        // ActivatedValue never existed, so this is a callback rename rather than a binding pair.
        Rename("Vek.VekTreeView`1", "ActivatedValueChanged", "SelectedValueChanged", "v7", V7Guide),
        Manual("Vek.VekTreeViewItem`1", "ExpandedIcon", "ExpandButtonIcon", nameof(Resources.VEK0002MigrationHintVekTreeViewItemExpandedIcon)),
        Rename("Vek.VekTreeViewItem`1", "ExpandedIconColor", "ExpandButtonIconColor", "v7", Docs),
        Rename("Vek.VekDropZone`1", "ItemIsDisabled", "ItemDisabled", "v7", V7Guide),
        Rename("Vek.VekDropContainer`1", "ItemIsDisabled", "ItemDisabled", "v7", V7Guide),
        Rename("Vek.VekChart`1", "XAxisLabels", "ChartLabels", "v9", V9Guide),
        Rename("Vek.VekChart`1", "InputLabels", "ChartLabels", "v9", V9Guide));

    private static ParameterMigration Rename(string component, string parameter, string replacement, string release, string url) =>
        Create(component, parameter, replacement, ParameterMigrationKind.Rename, nameof(Resources.VEK0002MigrationHintRename), parameter, release, replacement, url);

    private static ParameterMigration Inversion(string component, string parameter, string replacement, string release, string url) =>
        Create(component, parameter, replacement, ParameterMigrationKind.Inversion, nameof(Resources.VEK0002MigrationHintInversion), parameter, release, replacement, url);

    private static ParameterMigration InversionWithNewDefault(string component, string parameter, string replacement, string componentName, string release, string url) =>
        Create(component, parameter, replacement, ParameterMigrationKind.InversionWithNewDefault, nameof(Resources.VEK0002MigrationHintInversionWithNewDefault), parameter, release, replacement, componentName, url);

    private static ParameterMigration InheritedInversion(string component, string parameter, string replacement, string parentName, string release, string url) =>
        Create(component, parameter, replacement, ParameterMigrationKind.InheritedInversion, nameof(Resources.VEK0002MigrationHintInheritedInversion), parameter, release, replacement, parentName, url);

    private static ParameterMigration BindingValue(string component, string parameter, string replacement, string release, string url) =>
        Create(component, parameter, replacement, ParameterMigrationKind.Binding, nameof(Resources.VEK0002MigrationHintBinding),
            parameter, release, replacement, $"{parameter}Changed", $"{replacement}Changed", parameter, replacement, url);

    // Registers the callback half of a pair whose value half is registered with BindingValue.
    private static ParameterMigration BindingCallback(string component, string valueParameter, string valueReplacement, string release, string url) =>
        Create(component, $"{valueParameter}Changed", $"{valueReplacement}Changed", ParameterMigrationKind.Binding, nameof(Resources.VEK0002MigrationHintBinding),
            $"{valueParameter}Changed", release, $"{valueReplacement}Changed", valueParameter, valueReplacement, valueParameter, valueReplacement, url);

    private static ParameterMigration InitialState(string component, string parameter, string replacement, string release, string url) =>
        Create(component, parameter, replacement, ParameterMigrationKind.InitialState, nameof(Resources.VEK0002MigrationHintInitialState), parameter, release, replacement, url);

    // A manual hint is written out in full, with its own release and link, because its conversion does not fit a template.
    private static ParameterMigration Manual(string component, string parameter, string replacement, string resourceName) =>
        Create(component, parameter, replacement, ParameterMigrationKind.Manual, resourceName);

    private static ParameterMigration Create(string component, string parameter, string replacement, ParameterMigrationKind kind, string resourceName, params string[] formatArguments) =>
        new(component, parameter, replacement, kind, new LocalizableResourceString(resourceName, Resources.ResourceManager, typeof(Resources), formatArguments));
}
