// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.JSInterop;

namespace Vek;

/// <summary>
/// Lightweight wrapper around common browser JS calls used across Vek components.
/// </summary>
/// <remarks>
/// Keeps interop usage centralized for tasks like clipboard, window navigation, and style updates so components stay focused on UI logic.
/// </remarks>
internal class JsApiService : IJsApiService
{
    private readonly IJSRuntime _jsRuntime;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsApiService"/> class.
    /// </summary>
    /// <param name="jsRuntime">The JavaScript runtime to use for invoking JavaScript functions.</param>
    public JsApiService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <inheritdoc />
    public ValueTask CopyToClipboardAsync(string? text)
    {
        return _jsRuntime.InvokeVoidAsync("vekWindow.copyToClipboard", text);
    }

    /// <inheritdoc />
    public ValueTask Open(string link, string target)
    {
        if (target == "_blank")
        {
            return OpenInNewTabAsync(link);
        }

        return _jsRuntime.InvokeVoidAsync("open", link, target);
    }

    /// <inheritdoc />
    public ValueTask OpenInNewTabAsync(string url)
    {
        return _jsRuntime.InvokeVoidAsync("vekWindow.open", url, "_blank");
    }

    /// <inheritdoc />
    public ValueTask UpdateStyleProperty(string elementId, string propertyName, object value)
    {
        return _jsRuntime.InvokeVoidAsync("vekWindow.updateStyleProperty", elementId, propertyName, value);
    }
}
