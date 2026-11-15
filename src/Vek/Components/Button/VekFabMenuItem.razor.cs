using Microsoft.AspNetCore.Components;
using Vek.Utilities;

namespace Vek;

/// <summary>
/// A floating action button shown as one of the selectable options within a <see cref="VekFabMenu"/>.
/// </summary>
/// <seealso cref="VekFab" />
/// <seealso cref="VekFabMenu" />
public partial class VekFabMenuItem : VekFab
{
    /// <summary>
    /// Indicates whether the <see cref="Variant"/> property was explicitly set by the user.
    /// </summary>
    private bool _variantExplicitlySet;

    /// <summary>
    /// CSS class names for the component, including base classes and conditional classes based on properties.
    /// </summary>
    private new string Classname => new CssBuilder("vek-fab-menu-item")
            .AddClass(Class)
            .Build();

    /// <summary>
    /// The parent <see cref="VekFabMenu"/> component, used to inherit <see cref="Variant"/> when not explicitly set.
    /// </summary>
    [CascadingParameter]
    public VekFabMenu? ParentMenu { get; set; }

    /// <summary>
    /// The display variation to use.
    /// </summary>
    private Variant EffectiveVariant => _variantExplicitlySet ? Variant : ParentMenu?.Variant ?? Variant;

    /// <summary>
    /// The size of the menu item.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="Size.Medium"/>.
    /// </remarks>
    [Parameter, Category(CategoryTypes.Button.Appearance)]
    public override Size Size { get; set; } = Size.Medium;

    /// <summary>
    /// Sets the parameters for the component and determines if the Variant was explicitly set.
    /// </summary>
    /// <param name="parameters">The parameters to set.</param>
    public override Task SetParametersAsync(ParameterView parameters)
    {
        _variantExplicitlySet = parameters.TryGetValue<Variant>(nameof(Variant), out _);
        return base.SetParametersAsync(parameters);
    }
}
