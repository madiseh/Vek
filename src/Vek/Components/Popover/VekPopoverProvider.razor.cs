// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Components;
using Vek.Interfaces;

namespace Vek
{

    /// <summary>
    /// A required component which manages all Vek popovers.
    /// </summary>
    /// <remarks>
    /// This component is required for Vek components to display popovers properly.  It is typically added to your main layout page.
    /// </remarks>
    /// <seealso cref="VekThemeProvider"/>
    /// <seealso cref="VekDialogProvider"/>
    /// <seealso cref="VekSnackbarProvider"/>
    public partial class VekPopoverProvider : IDisposable, IPopoverObserver
    {
        private bool _isConnectedToService;

        [Inject]
        internal IPopoverService PopoverService { get; set; } = null!;

        /// <summary>
        /// Controls whether this provider is enabled.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>true</c>.
        /// If more than one <see cref="VekPopoverProvider"/> is detected, this property will be <c>false</c> to ensure only one instance is active.
        /// Can be overridden by setting a cascading parameter of <c>UsePopoverProvider</c> to <c>false</c>.
        /// </remarks>
        [CascadingParameter(Name = "UsePopoverProvider")]
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Releases resources used by this provider.
        /// </summary>
        public void Dispose()
        {
            PopoverService.Unsubscribe(this);
        }

        /// <inheritdoc />
        protected override void OnInitialized()
        {
            if (!Enabled)
            {
                return;
            }

            PopoverService.Subscribe(this);
            _isConnectedToService = true;
        }

        /// <inheritdoc />
        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            if (!Enabled && _isConnectedToService)
            {
                PopoverService.Unsubscribe(this);
                _isConnectedToService = false;

                return;
            }

            // Let's in our new case ignore _isConnectedToService and always update the subscription except Enabled = false. The manager is specifically designed for it.
            // The reason is because If an observer throws an exception during the PopoverCollectionUpdatedNotification, indicating a malfunction, it will be automatically unsubscribed.
            if (Enabled)
            {
                PopoverService.Subscribe(this);
            }
        }

        /// <inheritdoc />
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && Enabled && PopoverService.PopoverOptions.ThrowOnDuplicateProvider)
            {
                if (await PopoverService.GetProviderCountAsync() > 1)
                {
                    throw new InvalidOperationException("Duplicate VekPopoverProvider detected. Please ensure there is only one provider, or disable this warning with PopoverOptions.ThrowOnDuplicateProvider.");
                }
            }
            await base.OnAfterRenderAsync(firstRender);
        }

        /// <inheritdoc />
        Guid IPopoverObserver.Id { get; } = Guid.NewGuid();

        /// <inheritdoc />
        async Task IPopoverObserver.PopoverCollectionUpdatedNotificationAsync(PopoverHolderContainer container, CancellationToken cancellationToken)
        {
            switch (container.Operation)
            {
                // Update popover individually
                case PopoverHolderOperation.Update:
                    {
                        foreach (var holder in container.Holders)
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                return;
                            }

                            if (holder.ElementReference is IVekStateHasChanged stateHasChanged)
                            {
                                await InvokeAsync(stateHasChanged.StateHasChanged);
                            }
                        }

                        break;
                    }
                // Update whole VekPopoverProvider
                case PopoverHolderOperation.Create:
                case PopoverHolderOperation.Remove:
                    await InvokeAsync(StateHasChanged);
                    break;
            }
        }
    }
}
