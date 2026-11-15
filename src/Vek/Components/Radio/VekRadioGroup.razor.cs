// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Components;
using Vek.Utilities;
using Vek.Utilities.Exceptions;

namespace Vek
{

    /// <summary>
    /// A group of <see cref="VekRadio{T}"/> components.
    /// </summary>
    /// <typeparam name="T">The type of value being selected.</typeparam>
    public partial class VekRadioGroup<T> : VekFormComponent<T, T>, IVekRadioGroup
    {
        private VekRadio<T>? _selectedRadio;
        private readonly HashSet<VekRadio<T>> _radios = new();

        protected string Classname =>
            new CssBuilder("vek-input-control-boolean-input")
                .AddClass(Class)
                .Build();

        private string GetInputClass() =>
            new CssBuilder("vek-radio-group")
                .AddClass(InputClass)
                .Build();

        [CascadingParameter(Name = "ParentDisabled")]
        private bool ParentDisabled { get; set; }

        [CascadingParameter(Name = "ParentReadOnly")]
        private bool ParentReadOnly { get; set; }

        /// <summary>
        /// The CSS classes for this button group.
        /// </summary>
        /// <remarks>
        /// Multiple classes must be separated by spaces.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Radio.Appearance)]
        public string? InputClass { get; set; }

        /// <summary>
        /// The CSS styles for this button group.
        /// </summary>
        [Obsolete("Prefer the InputClass property with CSS https://github.com/madiseh/vek/issues/12047")]
        [Parameter]
        [Category(CategoryTypes.Radio.Appearance)]
        public string? InputStyle { get; set; }

        /// <summary>
        /// The content within this button group.
        /// </summary>
        /// <remarks>
        /// Usually a set of <see cref="VekRadio{T}"/> components.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Radio.Behavior)]
        public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// The unique name for this button group.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.Radio.Behavior)]
        public string Name { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Prevents the user from interacting with this group.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Behavior)]
        public bool Disabled { get; set; }

        /// <summary>
        /// Prevents the selected value from being changed.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Behavior)]
        public bool ReadOnly { get; set; }

        /// <summary>
        /// The current value.
        /// </summary>
        /// <remarks>
        /// When this value changes, the <see cref="ValueChanged"/> event occurs.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Radio.Data)]
        public T? Value
        {
            get => _value;
            set => SetSelectedOptionAsync(value, true, updateValue: false).CatchAndLog();
        }

        /// <summary>
        /// Occurs whenever <see cref="Value"/> has changed.
        /// </summary>
        [Parameter]
        public EventCallback<T> ValueChanged { get; set; }

        internal bool GetDisabledState() => Disabled || ParentDisabled; //internal because the VekRadio reads this value directly

        internal bool GetReadOnlyState() => ReadOnly || ParentReadOnly; //internal because the VekRadio reads this value directly

        /// <summary>
        /// The id of the rendered error text, so each <see cref="VekRadio{T}"/> can reference it through <c>aria-describedby</c>.
        /// </summary>
        internal string? GetErrorId() => ErrorIdState.Value;

        protected async Task SetSelectedOptionAsync(T? option, bool updateRadio, bool updateValue = true)
        {
            if (!OptionEquals(_value, option))
            {
                _value = option;

                if (updateRadio)
                {
                    var radio = _radios.FirstOrDefault(r => OptionEquals(r.ReadValue, _value));
                    await SetSelectedRadioAsync(radio, false);
                }

                if (updateValue)
                    await ValueChanged.InvokeAsync(_value);

                await BeginValidateAsync();
                FieldChanged(_value);
            }
        }

        /// <summary>
        /// Tests whether the specified value is valid for this button.
        /// </summary>
        /// <param name="selectItem">The value to examine.</param>
        /// <exception cref="GenericTypeMismatchException">Raised if the specified value does not match <c>T</c>.</exception>
        public void CheckGenericTypeMatch(object selectItem)
        {
            var itemT = selectItem.GetType().GenericTypeArguments[0];
            if (itemT != typeof(T))
            {
                throw new GenericTypeMismatchException("VekRadioGroup", "VekRadio", typeof(T), itemT);
            }
        }

        internal Task SetSelectedRadioAsync(VekRadio<T> radio)
        {
            Touched = true;
            return SetSelectedRadioAsync(radio, true);
        }

        protected async Task SetSelectedRadioAsync(VekRadio<T>? radio, bool updateOption)
        {
            if (_selectedRadio != radio)
            {
                _selectedRadio = radio;

                foreach (var item in _radios)
                {
                    item.SetChecked(item == _selectedRadio);
                }

                if (updateOption)
                {
                    await SetSelectedOptionAsync(GetValueOrDefault(_selectedRadio), false);
                }
            }
        }

        internal Task RegisterRadioAsync(VekRadio<T> radio)
        {
            _radios.Add(radio);

            if (_selectedRadio is null)
            {
                if (OptionEquals(radio.ReadValue, _value))
                {
                    return SetSelectedRadioAsync(radio, false);
                }
            }
            return Task.CompletedTask;
        }

        internal void UnregisterRadio(VekRadio<T> radio)
        {
            _radios.Remove(radio);

            if (_selectedRadio == radio)
            {
                _selectedRadio = null;
            }
        }

        protected override async Task ResetValueAsync()
        {
            // Route through SetSelectedOptionAsync so the value, selected radio, and checked markup stay in sync.
            await SetSelectedOptionAsync(default, updateRadio: true);
            await base.ResetValueAsync();
        }

        /// <inheritdoc />
        protected override IConverter<T?, T?> GetDefaultConverter() => EmptyConverter<T?>.Instance;

        private static T? GetValueOrDefault(VekRadio<T>? radio)
        {
            return radio is not null ? radio.ReadValue : default;
        }

        private static bool OptionEquals(T? option1, T? option2)
        {
            return EqualityComparer<T>.Default.Equals(option1, option2);
        }
    }
}
