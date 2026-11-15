// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Components;

namespace Vek.Components.Snackbar.InternalComponents;

/// <summary>
/// Renders a snackbar message supplied as an HTML <see cref="MarkupString"/>.
/// </summary>
public partial class SnackbarMessageMarkupString : ComponentBase
{
    /// <summary>
    /// Sets the message to be displayed as HTML content.
    /// </summary>
    /// <remarks>
    /// This property allows you to pass an HTML-formatted message using <see cref="MarkupString"/>.
    /// </remarks>
    [Parameter]
    public MarkupString Message { get; set; }
}
