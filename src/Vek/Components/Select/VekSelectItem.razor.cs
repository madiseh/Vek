using Microsoft.AspNetCore.Components;
using Vek.State;
using Vek.Utilities;

namespace Vek
{
    /// <summary>
    /// A selectable option displayed within a <see cref="VekSelect{T}"/> component.
    /// </summary>
    /// <typeparam name="T">The type of value linked to this item.  Must be the same type as the parent <see cref="VekSelect{T}"/>.</typeparam>
    /// <seealso cref="VekSelect{T}"/>
    public partial class VekSelectItem<T> : VekComponentBase, IDisposable
    {
        private VekSelectContext<T>? _context;
        private VekSelectContext<T>? _shadowContext;
        private IDisposable? _selectionSubscription;

        private string GetCssClasses() => new CssBuilder()
            .AddClass(Class)
            .Build();

        internal string ItemId { get; } = Identifier.Create();

        /// <summary>
        /// Builds fallback accessibility attributes for the rendered option element.
        /// </summary>
        /// <remarks>
        /// Option semantics come from this item's selection state rather than the popup's temporary active item.
        /// </remarks>
        private Dictionary<string, object?> GetUserAttributes()
        {
            var attributes = new Dictionary<string, object?>(UserAttributes, StringComparer.OrdinalIgnoreCase);
            attributes.TryAdd("aria-selected", Selected ? "true" : "false");

            if (Disabled)
            {
                attributes.TryAdd("aria-disabled", "true");
            }

            return attributes;
        }

        public VekSelectItem()
        {
            using var registerScope = CreateRegisterScope();
            registerScope.RegisterParameter<IVekSelect?>(nameof(IVekSelect))
                .WithParameter(() => IVekSelect)
                .WithChangeHandler(OnVekSelectChanged);
            registerScope.RegisterParameter<IVekShadowSelect?>(nameof(IVekShadowSelect))
                .WithParameter(() => IVekShadowSelect)
                .WithChangeHandler(OnVekShadowSelectChanged);
            registerScope.RegisterParameter<T?>(nameof(Value))
                .WithParameter(() => Value)
                .WithChangeHandler(OnValueChanged)
                .WithComparer(() => (_context ?? _shadowContext)?.Comparer ?? EqualityComparer<T?>.Default);
        }

        /// <summary>
        /// The <see cref="VekSelect{T}"/> hosting this item.
        /// </summary>
        /// <remarks>
        /// This cascading parameter is used to obtain the context for registration.
        /// When this parameter changes, OnVekSelectChanged is invoked to handle
        /// registration and unregistration with the appropriate parent.
        /// </remarks>
        [CascadingParameter]
        internal IVekSelect? IVekSelect { get; set; }

        /// <summary>
        /// The shadow select used for items that only provide RenderFragments.
        /// </summary>
        /// <remarks>
        /// Shadow items (HideContent=true) are registered in a separate lookup
        /// for value-to-RenderFragment resolution when the dropdown is closed.
        /// </remarks>
        [CascadingParameter]
        internal IVekShadowSelect? IVekShadowSelect { get; set; }

        /// <summary>
        /// Select items with HideContent==true are only there to register their RenderFragment with the select but
        /// won't render and have no other purpose!
        /// </summary>
        [CascadingParameter(Name = "HideContent")]
        internal bool HideContent { get; set; }

        /// <summary>
        /// Gets the parent VekSelect component.
        /// </summary>
        internal VekSelect<T>? VekSelect => (VekSelect<T>?)IVekSelect;

        /// <summary>
        /// The custom value associated with this item.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Behavior)]
        public T? Value { get; set; }

        /// <summary>
        /// Prevents the user from interacting with this item.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.General.Behavior)]
        public bool Disabled { get; set; }

        /// <summary>
        /// The custom content within this item.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.General.Behavior)]
        public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// Whether multi-selection is enabled in the parent <see cref="VekSelect{T}"/>.
        /// </summary>
        protected bool MultiSelection => _context?.MultiSelection == true;

        /// <summary>
        /// Whether this item is selected.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This state is updated by observing the parent's selection via the context.
        /// Items subscribe to selection changes and update this state accordingly.
        /// </para>
        /// </remarks>
        internal bool Selected { get; set; }

        /// <summary>
        /// The icon to display whether this item is selected.
        /// </summary>
        /// <remarks>
        /// When <see cref="Selected"/> is <c>true</c>, the parent's <see cref="VekSelect{T}.CheckedIcon"/> is returned.  Otherwise, its <see cref="VekSelect{T}.UncheckedIcon"/>.
        /// </remarks>
        protected string? CheckBoxIcon
        {
            get
            {
                if (_context is not { MultiSelection: true } context)
                    return null;
                return Selected ? context.CheckedIcon : context.UncheckedIcon;
            }
        }

        protected string DisplayString
        {
            get
            {
                // Use the parent's ConvertValueToString which delegates to ConvertSet (handles ToStringFunc)
                return VekSelect?.ConvertValueToString(Value) ?? $"{Value}";
            }
        }

        /// <summary>
        /// Handles changes to the IVekShadowSelect cascading parameter.
        /// </summary>
        /// <remarks>
        /// This is invoked when the shadow select parent changes (e.g., when moving between different selects).
        /// It unregisters from the old parent and registers with the new one.
        /// </remarks>
        private void OnVekShadowSelectChanged(ParameterChangedEventArgs<IVekShadowSelect?> args)
        {
            if (args.LastValue?.SelectContext is VekSelectContext<T> oldContext)
            {
                oldContext.UnregisterShadowItem(this);
            }

            if (args.Value?.SelectContext is VekSelectContext<T> newContext)
            {
                _shadowContext = newContext;
                _shadowContext.RegisterShadowItem(this);
            }
        }

        /// <summary>
        /// Handles changes to the IVekSelect cascading parameter.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is invoked when the select parent changes (e.g., when moving between different selects).
        /// It handles the complete lifecycle:
        /// </para>
        /// <list type="number">
        /// <item><description>Unsubscribes from the old parent's selection changes</description></item>
        /// <item><description>Unregisters from the old parent's context</description></item>
        /// <item><description>Registers with the new parent's context</description></item>
        /// <item><description>Subscribes to the new parent's selection changes</description></item>
        /// <item><description>Updates the initial Selected state</description></item>
        /// </list>
        /// </remarks>
        private void OnVekSelectChanged(ParameterChangedEventArgs<IVekSelect?> args)
        {
            if (args.LastValue?.SelectContext is VekSelectContext<T> oldContext)
            {
                _selectionSubscription?.Dispose();
                _selectionSubscription = null;
                oldContext.UnregisterItem(this);
            }

            if (args.Value?.SelectContext is VekSelectContext<T> newContext)
            {
                _context = newContext;

                // Register as a visible item (adds to _items, _valueLookup, and _shadowLookup)
                var isSelected = _context.RegisterItem(this);
                Selected = isSelected;

                _selectionSubscription = _context.SubscribeToSelectionChanges(OnSelectionChangedAsync);
            }
        }

        /// <summary>
        /// Handles selection changes from the parent select.
        /// </summary>
        /// <remarks>
        /// This callback is invoked when the parent's SelectedValues changes.
        /// It updates the local Selected state and triggers a re-render if needed.
        /// This replaces the OnUpdateSelectionStateFromOutside method.
        /// </remarks>
        private Task OnSelectionChangedAsync(IReadOnlyCollection<T?> selectedValues)
        {
            var oldSelected = Selected;
            Selected = selectedValues.Contains(Value);

            if (oldSelected != Selected)
            {
                // Avoid await InvokeAsync(StateHasChanged) due to a MAUI dispatcher bug:
                // https://github.com/madiseh/vek/issues/13009
                // This issue does not occur on other platforms (e.g., WASM, WinForms Hybrid, etc).
                StateHasChanged();
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Handles changes to the <see cref="Value"/> parameter.
        /// </summary>
        /// <remarks>
        /// When the parent swaps the underlying data collection while Blazor reuses the same component instances
        /// (e.g. a keyless <c>@foreach</c> binding new items to existing <see cref="VekSelectItem{T}"/> positions),
        /// the value-keyed lookups in <see cref="VekSelectContext{T}"/> must be updated to avoid stale keys.
        /// </remarks>
        private void OnValueChanged(ParameterChangedEventArgs<T?> args)
        {
            _context?.OnItemValueChanged(this, args.LastValue, args.Value);
            _shadowContext?.OnShadowItemValueChanged(this, args.LastValue, args.Value);

            if (_context is not null)
            {
                //Update selected value when updated without new item creation
                Selected = _context.IsItemSelected(this);
            }
        }

        /// <summary>
        /// Handles click events on the item.
        /// </summary>
        private async Task OnClickHandleAsync()
        {
            if (VekSelect is not null)
            {
                await VekSelect.SelectOption(Value);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases resources used by this component.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            var selection = _selectionSubscription;
            var context = _context;
            var shadow = _shadowContext;

            _selectionSubscription = null;
            _context = null;
            _shadowContext = null;

            selection?.Dispose();
            context?.UnregisterItem(this);
            shadow?.UnregisterShadowItem(this);
        }
    }
}
