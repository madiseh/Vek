// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Vek.Services;
using Vek.Utilities;

namespace Vek
{

    /// <summary>
    /// Allows the user to select a single choice from a group of options. Use radio buttons (not switches) when only one item can be selected from a list.
    /// </summary>
    /// <typeparam name="T">The type of value being selected, often a <c>bool</c>.</typeparam>
    /// <seealso cref="VekCheckBox{T}" />
    /// <seealso cref="VekRadioGroup{T}" />
    /// <seealso cref="VekSwitch{T}"/>
    public partial class VekRadio<T> : VekBooleanInput<T>
    {
        private IVekRadioGroup? _parent;
        private readonly string _elementId = Identifier.Create("radio");
        private readonly string _ariaId = Identifier.Create("radio-aria-");

        public VekRadio()
        {
            // The VekRadioGroup is the form participant; individual radios must not register (#11540).
            SubscribeToParentForm = false;
        }

        protected override string Classname => new CssBuilder("vek-input-control-boolean-input")
            .AddClass("vek-disabled", GetDisabledState())
            .AddClass("vek-readonly", GetReadOnlyState())
            .AddClass("vek-input-with-content", ChildContent is not null)
            .AddClass(Class)
            .Build();

        protected override string LabelClassname => new CssBuilder("vek-radio")
            .AddClass($"vek-disabled", GetDisabledState())
            .AddClass($"vek-readonly", GetReadOnlyState())
            .AddClass($"vek-input-content-placement-{ConvertPlacement(LabelPlacement).ToStringFast(true)}")
            .Build();

        protected override string IconClassname => new CssBuilder("vek-button-root vek-icon-button")
            .AddClass("vek-ripple vek-ripple-radio", Ripple && !GetDisabledState() && !GetReadOnlyState())
            .AddClass($"vek-{Color.ToStringFast(true)}-text", !GetDisabledState() && (UncheckedColor == null || Checked))
            .AddClass($"vek-{UncheckedColor?.ToStringFast(true)}-text", !GetDisabledState() && UncheckedColor != null && !Checked)
            .AddClass($"hover:vek-{Color.ToStringFast(true)}-hover", !GetReadOnlyState() && !GetDisabledState() && (UncheckedColor == null || Checked))
            .AddClass($"hover:vek-{UncheckedColor?.ToStringFast(true)}-hover", !GetReadOnlyState() && !GetDisabledState() && UncheckedColor != null && !Checked)
            .AddClass("vek-radio-dense", Dense)
            .AddClass("vek-disabled", GetDisabledState())
            .AddClass("vek-readonly", GetReadOnlyState())
            .AddClass("vek-checked", Checked)
            .AddClass("vek-error-text", GroupHasErrors)
            .Build();

        /// <summary>
        /// The <c>name</c> shared by every radio in the parent group.
        /// </summary>
        /// <remarks>
        /// Pushed by the group rather than read off it, so a radio re-renders when it changes without the group having to notify every radio on every render.
        /// </remarks>
        [CascadingParameter(Name = "RadioGroupName")]
        private string? GroupName { get; set; }

        /// <summary>
        /// Whether the parent group is showing a validation error.
        /// </summary>
        [CascadingParameter(Name = "RadioGroupHasErrors")]
        private bool GroupHasErrors { get; set; }

        /// <summary>
        /// The id of the parent group's error text, used to describe each radio.
        /// </summary>
        [CascadingParameter(Name = "RadioGroupErrorId")]
        private string? GroupErrorId { get; set; }

        [Inject]
        private IKeyInterceptorService KeyInterceptorService { get; set; } = null!;

        /// <summary>
        /// The parent Radio Group
        /// </summary>
        [CascadingParameter]
        internal IVekRadioGroup? IVekRadioGroup
        {
            get => _parent;
            set
            {
                _parent = value;
                _parent?.CheckGenericTypeMatch(this);
            }
        }

        /// <summary>
        /// The color to use when in an unchecked state.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Radio.Appearance)]
        public Color? UncheckedColor { get; set; } = null;

        /// <summary>
        /// The Aria Label to be assigned to the radio button.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>. Used to improve accessibility for screen readers. Adds an <c>aria-labelledby</c> to the <c>input</c> element.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Radio.Appearance)]
        public string? AriaLabel { get; set; }

        /// <summary>
        /// Uses compact vertical padding.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Radio.Appearance)]
        public bool Dense { get; set; }

        /// <summary>
        /// The icon displayed when in a checked state.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Icons.Material.Filled.RadioButtonChecked"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Appearance)]
        public string CheckedIcon { get; set; } = Icons.Material.Filled.RadioButtonChecked;

        /// <summary>
        /// The icon displayed when in an unchecked state.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Icons.Material.Filled.RadioButtonUnchecked"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Appearance)]
        public string UncheckedIcon { get; set; } = Icons.Material.Filled.RadioButtonUnchecked;

        /// <summary>
        /// The icon to display for an indeterminate state.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Icons.Material.Filled.IndeterminateCheckBox"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Appearance)]
        public string IndeterminateIcon { get; set; } = Icons.Material.Filled.IndeterminateCheckBox;

        private string GetIcon()
        {
            return Checked switch
            {
                true => CheckedIcon,
                false => UncheckedIcon
            };
        }

        internal bool Checked { get; private set; }

        internal VekRadioGroup<T>? VekRadioGroup => IVekRadioGroup as VekRadioGroup<T>;

        internal void SetChecked(bool value)
        {
            if (Checked != value)
            {
                Checked = value;
                StateHasChanged();
            }
        }

        /// <summary>
        /// Checks this radio button.
        /// </summary>
        /// <remarks>
        /// When part of a <see cref="VekRadioGroup{T}"/>, other values will be unchecked.
        /// </remarks>
        public Task SelectAsync()
        {
            if (VekRadioGroup is not null)
            {
                return VekRadioGroup.SetSelectedRadioAsync(this);
            }

            return Task.CompletedTask;
        }

        internal Task OnClickAsync()
        {
            if (GetDisabledState() || GetReadOnlyState() || (VekRadioGroup?.GetReadOnlyState() ?? false))
            {
                return Task.CompletedTask;
            }

            if (VekRadioGroup != null)
            {
                return VekRadioGroup.SetSelectedRadioAsync(this);
            }

            return Task.CompletedTask;
        }

        protected Task HandleKeyDownAsync(KeyboardEventArgs obj) => KeyInterceptorService.DispatchAsync(_elementId, KeyEventKind.Down, obj);

        private bool CanHandleKeys() => !GetDisabledState() && !GetReadOnlyState() && !(VekRadioGroup?.GetReadOnlyState() ?? false);

        /// <inheritdoc />
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            if (VekRadioGroup is not null)
            {
                await VekRadioGroup.RegisterRadioAsync(this);
            }
        }

        /// <inheritdoc />
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                var options = new KeyInterceptorOptions(
                    "vek-button-root",
                    [
                        // prevent scrolling page
                        new(" ", preventDown: "key+none", preventUp: "key+none"),
                        new("Enter", preventDown: "key+none"),
                        new("NumpadEnter", preventDown: "key+none")
                    ]);

                await KeyInterceptorService.SubscribeAsync(_elementId, options, keys => keys
                    .When(CanHandleKeys, builder => builder
                        .OnKeyDownAny(["Enter", "NumpadEnter", " "], SelectAsync)));
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        /// <inheritdoc />
        protected override async ValueTask DisposeAsyncCore()
        {
            await base.DisposeAsyncCore();
            VekRadioGroup?.UnregisterRadio(this);
            if (IsJSRuntimeAvailable)
            {
                await KeyInterceptorService.UnsubscribeAsync(_elementId);
            }
        }
    }
}
