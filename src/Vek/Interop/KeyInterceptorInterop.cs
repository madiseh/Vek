// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.JSInterop;
using Vek.Services;

namespace Vek.Interop;

internal class KeyInterceptorInterop
{
    private readonly IJSRuntime _jsRuntime;

    public KeyInterceptorInterop(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public ValueTask<bool> Connect<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>(DotNetObjectReference<T> dotNetObjectReference, string elementId, KeyInterceptorOptions options) where T : class
    {
        return _jsRuntime.InvokeVoidAsyncWithErrorHandling("vekKeyInterceptor.connect", dotNetObjectReference, elementId, options);
    }

    public ValueTask Disconnect(string elementId)
    {
        return _jsRuntime.InvokeVoidAsyncIgnoreErrors("vekKeyInterceptor.disconnect", elementId);
    }

    public ValueTask UpdateKey(string elementId, KeyOptions option)
    {
        return _jsRuntime.InvokeVoidAsyncIgnoreErrors("vekKeyInterceptor.updatekey", elementId, option);
    }
}
