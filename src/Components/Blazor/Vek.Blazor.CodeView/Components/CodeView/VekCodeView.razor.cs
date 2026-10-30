using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Vek.Components.Components.CodeView;

/// <summary>Displays read-only code with syntax highlighting and a copy action.</summary>
public partial class VekCodeView : IAsyncDisposable
{
    private const string ClipboardModulePath = "./_content/Vek.Blazor.CodeView/Components/CodeView/VekCodeView.razor.js";
    private IJSObjectReference? _clipboardModule;
    private bool _copySucceeded;
    private string _copyStatus = string.Empty;

    /// <summary>The code to display.</summary>
    [Parameter, EditorRequired]
    public string Code { get; set; } = string.Empty;

    /// <summary>The syntax to use when highlighting <see cref="Code"/>.</summary>
    [Parameter, EditorRequired]
    public VekCodeLanguage Language { get; set; }

    private string LanguageLabel => Language switch
    {
        VekCodeLanguage.Html => "HTML",
        VekCodeLanguage.Java => "Java",
        VekCodeLanguage.CSharp => "C#",
        VekCodeLanguage.Sql => "SQL",
        _ => "Code"
    };

    private IReadOnlyList<VekCodeToken> Tokens => VekCodeHighlighter.Highlight(Code ?? string.Empty, Language);

    private async Task CopyAsync()
    {
        _copySucceeded = false;

        try
        {
            _clipboardModule ??= await JsRuntime.InvokeAsync<IJSObjectReference>("import", ClipboardModulePath);
            _copySucceeded = await _clipboardModule.InvokeAsync<bool>("copyTextToClipboard", Code ?? string.Empty);
        }
        catch (JSException)
        {
            _copySucceeded = false;
        }
        catch (InvalidOperationException)
        {
            _copySucceeded = false;
        }

        _copyStatus = _copySucceeded ? "Code copied to clipboard." : "Could not copy code to clipboard.";
    }

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = default!;

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_clipboardModule is null)
        {
            return;
        }

        try
        {
            await _clipboardModule.DisposeAsync();
        }
        catch (JSDisconnectedException)
        {
            // The browser session can already be gone during disposal.
        }
    }
}
