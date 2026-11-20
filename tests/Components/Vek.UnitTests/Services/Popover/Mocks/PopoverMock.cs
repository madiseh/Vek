// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Components;

namespace Vek.UnitTests.Services.Popover.Mocks;

#nullable enable
internal class PopoverMock : IPopover
{
    public Guid Id { get; } = Guid.NewGuid();

    public string PopoverClass { get; set; } = string.Empty;

    public string PopoverStyles { get; set; } = string.Empty;

    public bool Open { get; set; }

    public object? Tag { get; set; }

    public Dictionary<string, object?> UserAttributes { get; set; } = new();

    public RenderFragment? ChildContent { get; set; }
}
