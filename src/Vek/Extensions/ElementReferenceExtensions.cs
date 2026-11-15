using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Vek.Interop;

namespace Vek
{
    [ExcludeFromCodeCoverage]
    public static class ElementReferenceExtensions
    {
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "<JSRuntime>k__BackingField")]
        private static extern ref IJSRuntime GetJsRuntime(WebElementReferenceContext context);

        internal static IJSRuntime? GetJSRuntime(this ElementReference elementReference)
        {
            if (elementReference.Context is WebElementReferenceContext context)
            {
                var jsRuntime = GetJsRuntime(context);

                return jsRuntime;
            }

            return null;
        }

        public static ValueTask VekFocusFirstAsync(this ElementReference elementReference, int skip = 0, int min = 0) =>
            elementReference.GetJSRuntime()?.InvokeVoidAsyncIgnoreErrors("vekElementRef.focusFirst", elementReference, skip, min) ?? ValueTask.CompletedTask;

        public static ValueTask VekFocusLastAsync(this ElementReference elementReference, int skip = 0, int min = 0) =>
            elementReference.GetJSRuntime()?.InvokeVoidAsyncIgnoreErrors("vekElementRef.focusLast", elementReference, skip, min) ?? ValueTask.CompletedTask;

        public static ValueTask VekSaveFocusAsync(this ElementReference elementReference) =>
            elementReference.GetJSRuntime()?.InvokeVoidAsyncIgnoreErrors("vekElementRef.saveFocus", elementReference) ?? ValueTask.CompletedTask;

        public static ValueTask VekRestoreFocusAsync(this ElementReference elementReference) =>
            elementReference.GetJSRuntime()?.InvokeVoidAsyncIgnoreErrors("vekElementRef.restoreFocus", elementReference) ?? ValueTask.CompletedTask;

        public static ValueTask VekBlurAsync(this ElementReference elementReference) =>
            elementReference.GetJSRuntime()?.InvokeVoidAsync("vekElementRef.blur", elementReference) ?? ValueTask.CompletedTask;

        public static ValueTask VekSelectAsync(this ElementReference elementReference) =>
            elementReference.GetJSRuntime()?.InvokeVoidAsync("vekElementRef.select", elementReference) ?? ValueTask.CompletedTask;

        public static ValueTask VekSelectRangeAsync(this ElementReference elementReference, int pos1, int pos2) =>
            elementReference.GetJSRuntime()?.InvokeVoidAsync("vekElementRef.selectRange", elementReference, pos1, pos2) ?? ValueTask.CompletedTask;

        public static ValueTask VekChangeCssAsync(this ElementReference elementReference, string css) =>
            elementReference.GetJSRuntime()?.InvokeVoidAsync("vekElementRef.changeCss", elementReference, css) ?? ValueTask.CompletedTask;

        public static async ValueTask<BoundingClientRect> VekGetBoundingClientRectAsync(this ElementReference elementReference)
        {
            var jsRuntime = elementReference.GetJSRuntime();
            if (jsRuntime is null)
            {
                return new BoundingClientRect();
            }

            var (_, boundingClientRect) = await jsRuntime.InvokeAsyncWithErrorHandling(new BoundingClientRect(), "vekElementRef.getBoundingClientRect", elementReference);

            return boundingClientRect;
        }

        public static async ValueTask<int[]> AddDefaultPreventingHandlers(this ElementReference elementReference, string[] eventNames)
        {
            var jsRuntime = elementReference.GetJSRuntime();
            if (jsRuntime is null)
            {
                return Array.Empty<int>();
            }

            var (_, listenerIds) = await jsRuntime.InvokeAsyncWithErrorHandling(Array.Empty<int>(), "vekElementRef.addDefaultPreventingHandlers", elementReference, eventNames);

            return listenerIds;
        }

        public static ValueTask RemoveDefaultPreventingHandlers(this ElementReference elementReference, string[] eventNames, int[] listenerIds)
        {
            // No handlers were attached (for example, the script was unavailable), so there is nothing to remove.
            if (listenerIds.Length == 0)
            {
                return ValueTask.CompletedTask;
            }

            if (eventNames.Length != listenerIds.Length)
            {
                throw new ArgumentException($"Number of elements in {nameof(eventNames)} and {nameof(listenerIds)} has to match.");
            }

            return elementReference.GetJSRuntime()?.InvokeVoidAsyncIgnoreErrors("vekElementRef.removeDefaultPreventingHandlers", elementReference, eventNames, listenerIds) ?? ValueTask.CompletedTask;
        }

        public static ValueTask VekAttachBlurEventWithJS<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>(
            this ElementReference elementReference,
            DotNetObjectReference<T> obj) where T : class =>
            elementReference.GetJSRuntime()?.InvokeVoidAsyncIgnoreErrors("vekElementRef.addOnBlurEvent", elementReference, obj) ?? ValueTask.CompletedTask;
    }
}
