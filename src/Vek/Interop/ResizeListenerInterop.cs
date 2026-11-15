// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.JSInterop;
using Vek.Services;

namespace Vek.Interop;

internal class ResizeListenerInterop
{
    private readonly IJSRuntime _jsRuntime;

    public ResizeListenerInterop(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async ValueTask<bool> MatchMedia(string mediaQuery, CancellationToken cancellationToken = default)
    {
        var (_, value) = await _jsRuntime.InvokeAsyncWithErrorHandling(false, "vekResizeListener.matchMedia", cancellationToken, mediaQuery);

        return value;
    }

    public ValueTask<bool> ListenForResize<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>(DotNetObjectReference<T> dotNetObjectReference, ResizeOptions options, Guid javaScriptListerId, CancellationToken cancellationToken = default) where T : class
    {
        return _jsRuntime.InvokeVoidAsyncWithErrorHandling("vekResizeListenerFactory.listenForResize", cancellationToken, dotNetObjectReference, options, javaScriptListerId);
    }

    public ValueTask<bool> CancelListener(Guid javaScriptListerId, CancellationToken cancellationToken = default)
    {
        return _jsRuntime.InvokeVoidAsyncWithErrorHandling("vekResizeListenerFactory.cancelListener", cancellationToken, javaScriptListerId);
    }

    [ExcludeFromCodeCoverage(Justification = "Not used in the core for now.")]
    public ValueTask<bool> CancelListeners(Guid[] jsListenerIds, CancellationToken cancellationToken = default)
    {
        return _jsRuntime.InvokeVoidAsyncWithErrorHandling("vekResizeListenerFactory.cancelListeners", cancellationToken, jsListenerIds);
    }

    public ValueTask DisposeAsync(CancellationToken cancellationToken = default)
    {
        return _jsRuntime.InvokeVoidAsyncIgnoreErrors("vekResizeListenerFactory.dispose", cancellationToken);
    }

    public async ValueTask<BrowserWindowSize> GetBrowserWindowSize(CancellationToken cancellationToken = default)
    {
        var (_, value) = await _jsRuntime.InvokeAsyncWithErrorHandling(new BrowserWindowSize(), "vekResizeListener.getBrowserWindowSize", cancellationToken);

        return value;
    }
}
