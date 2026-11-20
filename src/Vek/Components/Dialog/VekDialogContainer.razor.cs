// Copyright (c) 2019 Blazored
// Copyright (c) 2020 Adapted by Jonny Larsson, Meinrad Recheis and Contributors

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Vek.Services;
using Vek.State;
using Vek.Utilities;

namespace Vek
{
    /// <summary>
    /// An instance of a <see cref="VekDialog"/>.
    /// </summary>
    /// <remarks>
    /// When a <see cref="VekDialog"/> is shown, a new instance is created.  This instance can then be used to perform actions such as hiding the dialog programmatically.
    /// </remarks>
    /// <seealso cref="VekDialog"/>
    /// <seealso cref="VekDialogProvider"/>
    /// <seealso cref="DialogOptions"/>
    /// <seealso cref="DialogParameters{T}"/>
    /// <seealso cref="DialogReference"/>
    /// <seealso cref="DialogService"/>
    public partial class VekDialogContainer : VekComponentBase, IVekDialogInstanceInternal, IAsyncDisposable
    {
        private bool _disposed;
        private VekDialog? _dialog;
        private ElementReference _dialogContainerReference;
        private DotNetObjectReference<DialogInteractionCallbacks>? _dialogInteractionReference;
        private DialogBounds? _dialogBounds;
        private bool? _lastDraggable;
        private bool? _lastResizable;
        private readonly ParameterState<DialogOptions> _dialogOptionsState;
        private readonly ParameterState<string?> _titleState;

        internal string ElementId { get; } = Identifier.Create("dialog");

        public VekDialogContainer()
        {
            var registerScope = CreateRegisterScope();
            _dialogOptionsState = registerScope.RegisterParameter<DialogOptions>(nameof(Options))
                .WithParameter(() => Options);
            _titleState = registerScope.RegisterParameter<string?>(nameof(Title))
                .WithParameter(() => Title);
        }

        [Inject]
        private IKeyInterceptorService KeyInterceptorService { get; set; } = null!;

        [Inject]
        private IJSRuntime JsRuntime { get; set; } = null!;

        /// <summary>
        /// Displays this dialog right-to-left.
        /// </summary>
        [CascadingParameter(Name = "RightToLeft")]
        public bool RightToLeft { get; set; }

        [CascadingParameter]
        private VekDialogProvider Parent { get; set; } = null!;

        [CascadingParameter]
        private DialogOptions GlobalDialogOptions { get; set; } = DialogOptions.Default;

        /// <summary>
        /// The options used for this dialog.
        /// </summary>
        /// <remarks>
        /// Defaults to the options in the <see cref="VekDialog"/> or options passed during <see cref="DialogService.ShowAsync(Type)"/> methods.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.Dialog.Misc)] // Behavior and Appearance
        public DialogOptions Options { get; set; } = DialogOptions.Default;

        /// <summary>
        /// The text displayed at the top of this dialog if <see cref="TitleContent" /> is not set.
        /// </summary>
        [Parameter, ParameterState]
        [Category(CategoryTypes.Dialog.Behavior)]
        public string? Title { get; set; }

        /// <summary>
        /// The custom content at the top of this dialog.
        /// </summary>
        /// <remarks>
        /// This content will display so long as <see cref="Title"/> is not set.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Dialog.Behavior)]
        public RenderFragment? TitleContent { get; set; }

        /// <summary>
        /// The content within this dialog.
        /// </summary>
        /// <remarks>
        /// Defaults to the content of the <see cref="VekDialog"/> being displayed.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Dialog.Behavior)]
        public RenderFragment? Content { get; set; }

        /// <inheritdoc />
        [Parameter]
        [Category(CategoryTypes.Dialog.Behavior)]
        public Guid Id { get; set; }

        /// <summary>
        /// The custom icon displayed in the upper-right corner for closing this dialog.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Icons.Material.Filled.Close"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Dialog.Appearance)]
        public string CloseIcon { get; set; } = Icons.Material.Filled.Close;

        protected string TitleClassname =>
            new CssBuilder("vek-dialog-title")
                .AddClass(_dialog?.TitleClass)
                .Build();

        protected string Classname =>
            new CssBuilder("vek-dialog")
                .AddClass(GetMaxWidth(), !GetFullScreen())
                .AddClass("vek-dialog-width-full", GetFullWidth() && !GetFullScreen())
                .AddClass("vek-dialog-fullscreen", GetFullScreen())
                .AddClass("vek-dialog-rtl", RightToLeft)
                .AddClass("vek-dialog-draggable", GetDraggable())
                .AddClass("vek-dialog-resizable", GetResizable())
                .AddClass(_dialog?.Class)
                .Build();

        protected string DialogStyle
        {
            get
            {
                var builder = StyleBuilder.Default(Style);
                if (_dialogBounds is { } bounds)
                {
                    builder
                        .AddStyle("position:absolute")
                        .AddStyle(FormattableString.Invariant($"left:{bounds.Left}px"))
                        .AddStyle(FormattableString.Invariant($"top:{bounds.Top}px"))
                        .AddStyle(FormattableString.Invariant($"width:{bounds.Width}px"))
                        .AddStyle(FormattableString.Invariant($"height:{bounds.Height}px"))
                        .AddStyle("transform:none")
                        .AddStyle("margin:0");
                }

                return builder.Build();
            }
        }

        protected string BackgroundClassname =>
            new CssBuilder("vek-overlay-dialog")
                .AddClass($"vek-skip-overlay-section") // dialog overlay remains outside of Section
                .AddClass("vek-skip-overlay-positioning") // popovers try to position the overlay by zindex, this skips that behavior if a user puts the dialog provider above the popover provider
                .AddClass(GetBackgroundClass())
                .Build();

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            var draggable = GetDraggable();
            var resizable = GetResizable();
            if ((firstRender || draggable != _lastDraggable || resizable != _lastResizable) && IsJSRuntimeAvailable)
            {
                _lastDraggable = draggable;
                _lastResizable = resizable;

                if (draggable || resizable)
                {
                    _dialogInteractionReference ??= DotNetObjectReference.Create(new DialogInteractionCallbacks(this));
                    await JsRuntime.InvokeVoidAsync("vekDialogInteraction.initialize", _dialogContainerReference, _dialogInteractionReference, draggable, resizable, _dialogBounds);
                }
                else if (!firstRender)
                {
                    await JsRuntime.InvokeVoidAsync("vekDialogInteraction.destroy", _dialogContainerReference);
                }
            }

            if (firstRender)
            {
                var options = new KeyInterceptorOptions(
                    "vek-dialog",
                    [
                        new("/./", subscribeDown: true, subscribeUp: true)
                    ]);

                await KeyInterceptorService.SubscribeAsync(ElementId, options, keys => keys
                    .OnKeyDown("Escape", HandleEscapeAsync)
                    .OnKeyDown("/./", HandleAnyKeyDownAsync)
                    .OnKeyUp("/./", HandleAnyKeyUpAsync));
            }
            await base.OnAfterRenderAsync(firstRender);
        }

        private Task HandleEscapeAsync()
        {
            if (GetCloseOnEscapeKey())
            {
                ((IVekDialogInstance)this).Cancel();
            }
            return Task.CompletedTask;
        }

        private async Task HandleAnyKeyDownAsync(KeyboardEventArgs args)
        {
            // Don't invoke callback for Escape - it's handled separately
            if (args.Key == "Escape")
                return;

            if (_dialog is not null && _dialog.OnKeyDown.HasDelegate)
            {
                await _dialog.OnKeyDown.InvokeAsync(args);
                // Note: we need to force a render here because the user will expect this blazor standard functionality.
                // Since the event originates from KeyInterceptor it will not cause a render automatically.
                await InvokeAsync(StateHasChanged);
            }
        }

        private async Task HandleAnyKeyUpAsync(KeyboardEventArgs args)
        {
            if (_dialog is not null && _dialog.OnKeyUp.HasDelegate)
            {
                await _dialog.OnKeyUp.InvokeAsync(args);
                // note: we need to force a render here because the user will expect this blazor standard functionality
                // Since the event originates from KeyInterceptor it will not cause a render automatically.
                await InvokeAsync(StateHasChanged);
            }
        }

        private async Task OnMouseUpAsync(MouseEventArgs args)
        {
            if (args.Button > 0)
                await RefocusDialogAsync();
        }

        /// <summary>
        /// The element that names the dialog: an explicit <see cref="VekDialog.AriaLabelledBy"/>, else the title bar when it is shown and no <see cref="VekDialog.AriaLabel"/> replaces it.
        /// </summary>
        private string? GetAriaLabelledBy()
        {
            if (!string.IsNullOrWhiteSpace(_dialog?.AriaLabelledBy))
            {
                return _dialog.AriaLabelledBy;
            }

            return GetHideHeader() || !string.IsNullOrWhiteSpace(_dialog?.AriaLabel) ? null : $"_{Id:N}_title";
        }

        /// <summary>
        /// The text that names the dialog when no element does: an explicit <see cref="VekDialog.AriaLabel"/>, else the title while the header is hidden.
        /// </summary>
        private string? GetAriaLabel()
        {
            if (!string.IsNullOrWhiteSpace(_dialog?.AriaLabelledBy))
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(_dialog?.AriaLabel))
            {
                return _dialog.AriaLabel;
            }

            return GetHideHeader() && !string.IsNullOrWhiteSpace(_titleState.Value) ? _titleState.Value : null;
        }

        private bool GetHideHeader()
        {
            if (GetDialogOptionsOrDefault.NoHeader.HasValue)
                return GetDialogOptionsOrDefault.NoHeader.Value;

            if (GlobalDialogOptions.NoHeader.HasValue)
                return GlobalDialogOptions.NoHeader.Value;

            return false;
        }

        private bool GetCloseButton()
        {
            if (GetDialogOptionsOrDefault.CloseButton.HasValue)
                return GetDialogOptionsOrDefault.CloseButton.Value;

            if (GlobalDialogOptions.CloseButton.HasValue)
                return GlobalDialogOptions.CloseButton.Value;

            return false;
        }

        private bool GetBackdropClick()
        {
            if (GetDialogOptionsOrDefault.BackdropClick.HasValue)
                return GetDialogOptionsOrDefault.BackdropClick.Value;

            if (GlobalDialogOptions.BackdropClick.HasValue)
                return GlobalDialogOptions.BackdropClick.Value;

            return true;
        }

        private bool GetCloseOnEscapeKey()
        {
            if (GetDialogOptionsOrDefault.CloseOnEscapeKey.HasValue)
                return GetDialogOptionsOrDefault.CloseOnEscapeKey.Value;

            if (GlobalDialogOptions.CloseOnEscapeKey.HasValue)
                return GlobalDialogOptions.CloseOnEscapeKey.Value;

            return false;
        }

        private async Task HandleBackgroundClickAsync(MouseEventArgs args)
        {
            if (!GetBackdropClick())
            {
                await RefocusDialogAsync();
                return;
            }

            if (_dialog is not null && _dialog.OnBackdropClick.HasDelegate)
            {
                await _dialog.OnBackdropClick.InvokeAsync(args);
                await RefocusDialogAsync();
            }
            else
            {
                ((IVekDialogInstance)this).Cancel();
            }
        }

        private async Task RefocusDialogAsync()
        {
            if (GetCloseOnEscapeKey() && !_disposed)
            {
                await _dialogContainerReference.FocusAsync();
            }
        }

        private string GetPosition()
        {
            DialogPosition position;

            if (GetDialogOptionsOrDefault.Position.HasValue)
            {
                position = GetDialogOptionsOrDefault.Position.Value;
            }
            else if (GlobalDialogOptions.Position.HasValue)
            {
                position = GlobalDialogOptions.Position.Value;
            }
            else
            {
                position = DialogPosition.Center;
            }
            return $"vek-dialog-{position.ToStringFast(true)}";
        }

        private string GetMaxWidth()
        {
            MaxWidth maxWidth;

            if (GetDialogOptionsOrDefault.MaxWidth.HasValue)
            {
                maxWidth = GetDialogOptionsOrDefault.MaxWidth.Value;
            }
            else if (GlobalDialogOptions.MaxWidth.HasValue)
            {
                maxWidth = GlobalDialogOptions.MaxWidth.Value;
            }
            else
            {
                maxWidth = MaxWidth.Small;
            }
            return $"vek-dialog-width-{maxWidth.ToStringFast(true)}";
        }

        private bool GetFullWidth() => GetDialogOptionsOrDefault.FullWidth ?? GlobalDialogOptions.FullWidth ?? false;

        private bool GetFullScreen() => GetDialogOptionsOrDefault.FullScreen ?? GlobalDialogOptions.FullScreen ?? false;

        private bool GetDraggable() => GetDialogOptionsOrDefault.Draggable ?? GlobalDialogOptions.Draggable ?? false;

        private bool GetResizable() => GetDialogOptionsOrDefault.Resizable ?? GlobalDialogOptions.Resizable ?? false;

        private string? GetBackgroundClass() => GetDialogOptionsOrDefault.BackgroundClass ?? GlobalDialogOptions.BackgroundClass;

        private DialogOptions GetDialogOptionsOrDefault => _dialogOptionsState.Value ?? DialogOptions.Default;

        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            if (IsJSRuntimeAvailable)
            {
                await KeyInterceptorService.UnsubscribeAsync(ElementId);
                if (_dialogInteractionReference is not null)
                {
                    await JsRuntime.InvokeVoidAsync("vekDialogInteraction.destroy", _dialogContainerReference);
                }
            }

            _dialogInteractionReference?.Dispose();
        }

        private Task SetDialogBoundsAsync(double left, double top, double width, double height)
        {
            _dialogBounds = new DialogBounds(left, top, width, height);
            return InvokeAsync(StateHasChanged);
        }

        private sealed record DialogBounds(double Left, double Top, double Width, double Height);

        private sealed class DialogInteractionCallbacks(VekDialogContainer dialog)
        {
            [JSInvokable]
            public Task SetDialogBoundsAsync(double left, double top, double width, double height)
            {
                return dialog.SetDialogBoundsAsync(left, top, width, height);
            }
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        string IVekDialogInstance.ElementId => ElementId;

        /// <inheritdoc />
        string? IVekDialogInstance.Title => _titleState.Value;

        /// <inheritdoc />
        DialogOptions IVekDialogInstance.Options => GetDialogOptionsOrDefault;

        /// <inheritdoc />
        async Task IVekDialogInstance.SetOptionsAsync(DialogOptions options)
        {
            await _dialogOptionsState.SetValueAsync(options);
            Parent.SetOptions(Id, options);
            await InvokeAsync(StateHasChanged);
        }

        /// <inheritdoc />
        async Task IVekDialogInstance.SetTitleAsync(string? title)
        {
            await _titleState.SetValueAsync(title);
            await InvokeAsync(StateHasChanged);
        }

        /// <inheritdoc />
        void IVekDialogInstance.Close()
        {
            ((IVekDialogInstance)this).Close(DialogResult.Ok<object?>(null));
        }

        /// <inheritdoc />
        void IVekDialogInstance.Close(DialogResult dialogResult)
        {
            Parent.DismissInstance(Id, dialogResult);
        }

        /// <inheritdoc />
        void IVekDialogInstance.Close<T>(T returnValue)
        {
            var dialogResult = DialogResult.Ok<T>(returnValue);
            Parent.DismissInstance(Id, dialogResult);
        }

        /// <inheritdoc />
        void IVekDialogInstance.Cancel() => ((IVekDialogInstance)this).Close(DialogResult.Cancel());

        /// <inheritdoc />
        void IVekDialogInstanceInternal.Register(VekDialog dialog)
        {
            _dialog = dialog;
            Class = dialog.Class;
            Style = dialog.Style;
            TitleContent = dialog.TitleContent;
            StateHasChanged();
        }

        /// <inheritdoc />
        void IVekDialogInstance.StateHasChanged() => StateHasChanged();

        /// <inheritdoc />
        void IVekDialogInstance.CancelAll()
        {
            Parent?.DismissAll();
        }
    }
}
