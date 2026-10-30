using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Vek.Components.Components.CodeView;

/// <summary>Displays titled code samples through an accessible tab interface.</summary>
public partial class VekCodeViewMulti
{
    private readonly string _instanceId = Guid.NewGuid().ToString("N");
    private ElementReference[] _tabElements = [];
    private int _selectedIndex;

    /// <summary>The samples available for selection.</summary>
    [Parameter, EditorRequired]
    public IReadOnlyList<VekCodeViewTab> Tabs { get; set; } = [];

    private VekCodeViewTab? ActiveTab =>
        Tabs.Count > 0 && (uint)_selectedIndex < (uint)Tabs.Count ? Tabs[_selectedIndex] : null;

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        if (_tabElements.Length != Tabs.Count)
        {
            _tabElements = new ElementReference[Tabs.Count];
        }

        _selectedIndex = Tabs.Count == 0 ? 0 : Math.Clamp(_selectedIndex, 0, Tabs.Count - 1);
    }

    private string GetTabId(int index) => $"vek-code-view-multi-{_instanceId}-tab-{index}";

    private string GetPanelId(int index) => $"vek-code-view-multi-{_instanceId}-panel-{index}";

    private void SelectTab(int index)
    {
        if ((uint)index < (uint)Tabs.Count)
        {
            _selectedIndex = index;
        }
    }

    private async Task HandleTabKeyDownAsync(KeyboardEventArgs args, int currentIndex)
    {
        var nextIndex = args.Key switch
        {
            "ArrowRight" => Math.Min(currentIndex + 1, Tabs.Count - 1),
            "ArrowLeft" => Math.Max(currentIndex - 1, 0),
            "Home" => 0,
            "End" => Tabs.Count - 1,
            _ => currentIndex
        };

        if (nextIndex == currentIndex || (uint)nextIndex >= (uint)Tabs.Count)
        {
            return;
        }

        _selectedIndex = nextIndex;
        await _tabElements[nextIndex].FocusAsync();
    }
}
