// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using BytexDigital.Blazor.Components.CookieConsent.Dialogs.Prompt.Default;

namespace Vek.Docs.Pages.Consent.Prompt
{
#nullable enable
    public class VekCookieConsentPromptVariant : CookieConsentDefaultPromptVariant
    {
        /// <inheritdoc />
        public override Type ComponentType { get; set; } = typeof(VekCookieConsentPrompt);
    }
}
