// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.JSInterop;

namespace Vek.Interop;

internal class PopoverJsInterop
{
    private readonly IJSRuntime _jsRuntime;

    public PopoverJsInterop(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public ValueTask<bool> Initialize(string containerClass, int flipMargin, int overflowPadding, CancellationToken cancellationToken = default)
    {
        return _jsRuntime.InvokeVoidAsyncWithErrorHandling("vekPopover.initialize", cancellationToken, containerClass, flipMargin, overflowPadding);
    }

    public ValueTask<bool> Connect(Guid id, CancellationToken cancellationToken = default)
    {
        return _jsRuntime.InvokeVoidAsyncWithErrorHandling("vekPopover.connect", cancellationToken, id);
    }

    public ValueTask<bool> Disconnect(Guid id, CancellationToken cancellationToken = default)
    {
        return _jsRuntime.InvokeVoidAsyncWithErrorHandling("vekPopover.disconnect", cancellationToken, id);
    }

    public ValueTask<(bool success, int value)> CountProviders(CancellationToken cancellationToken = default)
    {
        return _jsRuntime.InvokeAsyncWithErrorHandling<int>("vekpopoverHelper.countProviders", cancellationToken);
    }

    public ValueTask DisposeAsync(CancellationToken cancellationToken = default)
    {
        return _jsRuntime.InvokeVoidAsyncIgnoreErrors("vekPopover.dispose", cancellationToken);
    }
}
