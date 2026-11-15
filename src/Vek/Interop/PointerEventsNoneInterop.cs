// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.JSInterop;

namespace Vek.Interop;


internal class PointerEventsNoneInterop
{
    private readonly IJSRuntime _jsRuntime;

    public PointerEventsNoneInterop(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public ValueTask<bool> ListenForPointerEventsAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>(
        DotNetObjectReference<T> dotNetObjectReference,
        string elementId,
        PointerEventsNoneOptions options,
        CancellationToken cancellationToken = default) where T : class
    {
        return _jsRuntime.InvokeVoidAsyncWithErrorHandling("vekPointerEventsNone.listenForPointerEvents", cancellationToken, dotNetObjectReference, elementId, options);
    }

    public ValueTask<bool> CancelListenerAsync(string elementId, CancellationToken cancellationToken = default)
    {
        return _jsRuntime.InvokeVoidAsyncWithErrorHandling("vekPointerEventsNone.cancelListener", cancellationToken, elementId);
    }

    public ValueTask DisposeAsync(CancellationToken cancellationToken = default)
    {
        return _jsRuntime.InvokeVoidAsyncIgnoreErrors("vekPointerEventsNone.dispose", cancellationToken);
    }
}
