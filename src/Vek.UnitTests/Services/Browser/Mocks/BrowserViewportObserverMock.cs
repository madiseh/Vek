// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Vek.Services;

namespace Vek.UnitTests.Services.Browser.Mocks;

#nullable enable
internal class BrowserViewportObserverMock : IBrowserViewportObserver
{
    public Guid Id { get; } = Guid.NewGuid();

    public ResizeOptions? ResizeOptions { get; }

    public List<BrowserViewportEventArgs> Notifications { get; } = new();

    public BrowserViewportObserverMock() : this(null)
    {
    }

    public BrowserViewportObserverMock(ResizeOptions? resizeOptions)
    {
        ResizeOptions = resizeOptions;
    }

    public Task NotifyBrowserViewportChangeAsync(BrowserViewportEventArgs browserViewportEventArgs)
    {
        Notifications.Add(browserViewportEventArgs);

        return Task.CompletedTask;
    }
}
