using Vek.Utilities;

namespace Vek;


/// <summary>
/// Arranges the top-level page structure of an app with an app bar, navigation drawer, and main content area.
/// </summary>
/// <remarks>
/// Layouts often contain <see cref="VekAppBar"/> and <see cref="VekDrawer"/> components.  The <see cref="VekMainContent"/> component is used to contain page content.
/// In your layout component, but above this component, add <see cref="VekThemeProvider"/>, <see cref="VekPopoverProvider"/>, <see cref="VekDialogProvider"/>, and <see cref="VekSnackbarProvider"/> components to enable all Vek features.
/// </remarks>
/// <seealso cref="VekAppBar" />
/// <seealso cref="VekDrawer" />
/// <seealso cref="VekMainContent" />
public partial class VekLayout : VekDrawerContainer
{
    protected override string Classname =>
        new CssBuilder("vek-layout")
            .AddClass(base.Classname)
            .Build();

    public VekLayout()
    {
        Fixed = true;
    }
}
