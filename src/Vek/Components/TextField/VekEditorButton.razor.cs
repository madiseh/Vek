using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Vek.Utilities;

namespace Vek;

/// <summary>
/// An action button displayed inside a text field's input frame.
/// </summary>
public partial class VekEditorButton
{
    protected string Classname => new CssBuilder("vek-input-editor-button")
        .AddClass("vek-input-editor-button-start", Placement == Adornment.Start)
        .AddClass("vek-input-editor-button-end", Placement != Adornment.Start)
        .AddClass(Class)
        .Build();

    /// <summary>
    /// Indicates whether the containing input is disabled.
    /// </summary>
    [CascadingParameter(Name = "EditorButtonDisabled")]
    private bool InputDisabled { get; set; }

    private bool EffectiveDisabled => Disabled || InputDisabled;

    /// <summary>
    /// Positions the button at the start or end of the input frame.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="Adornment.End"/>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.FormComponent.Appearance)]
    public Adornment Placement { get; set; } = Adornment.End;

    /// <summary>
    /// The icon displayed before the button content.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.FormComponent.Appearance)]
    public string? Icon { get; set; }

    /// <summary>
    /// The visible text or additional content displayed in the button.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.FormComponent.Behavior)]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// The accessible name for an icon-only button.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.FormComponent.Appearance)]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Prevents the action while the button or its containing input is disabled.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.FormComponent.Behavior)]
    public bool Disabled { get; set; }

    /// <summary>
    /// Occurs when the action is activated.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.FormComponent.Behavior)]
    public EventCallback<MouseEventArgs> OnClick { get; set; }

    /// <summary>
    /// Additional CSS classes applied to the button.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.FormComponent.Appearance)]
    public string? Class { get; set; }

    /// <summary>
    /// Additional inline styles applied to the button.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.FormComponent.Appearance)]
    public string? Style { get; set; }

    /// <summary>
    /// Unmatched attributes applied to the button element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [Category(CategoryTypes.General.Appearance)]
    public Dictionary<string, object>? UserAttributes { get; set; }
}
