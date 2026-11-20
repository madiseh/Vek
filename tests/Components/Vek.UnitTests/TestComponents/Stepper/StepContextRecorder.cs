#nullable enable
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Vek.UnitTests.TestComponents.Stepper;

public class StepContextRecorder : ComponentBase
{
    [Parameter]
    public string? ActiveMarkup { get; set; }

    [Parameter]
    public Action<VekStepContext?>? Capture { get; set; }

    [CascadingParameter]
    public VekStepContext? Context { get; set; }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        Capture?.Invoke(Context);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (Context?.IsActive == true && !string.IsNullOrEmpty(ActiveMarkup))
        {
            builder.AddMarkupContent(0, ActiveMarkup);
        }
    }
}
