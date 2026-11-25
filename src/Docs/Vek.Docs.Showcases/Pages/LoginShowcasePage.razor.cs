// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Vek;

namespace Vek.Docs.Showcases.Pages;

public partial class LoginShowcasePage : ComponentBase
{
    [Inject] private IJsApiService JsApiService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    private string _activeVariantId = "login-01";
    private int _tabIndex = 0; // 0 = Preview, 1 = Code

    // Variant 1 state
    private string _v1Email = "m@example.com";
    private string _v1Password = string.Empty;
    private bool _v1RememberMe = true;
    private bool _v1ShowPassword = false;

    // Variant 2 state
    private string _v2Email = string.Empty;
    private string _v2Password = string.Empty;
    private bool _v2ShowPassword = false;

    // Variant 3 state
    private string _v3Email = string.Empty;
    private string _v3Password = string.Empty;
    private bool _v3ShowPassword = false;

    // Variant 4 state
    private string _v4Email = string.Empty;
    private string _v4Password = string.Empty;
    private bool _v4TermsAccepted = true;
    private bool _v4ShowPassword = false;

    private readonly List<VariantOption> _variants = new()
    {
        new("login-01", "Login 01 - Centered Card", "Simple card layout with email, password, and Google auth. Ideal for standard web apps."),
        new("login-02", "Login 02 - Split Screen", "Two-column authentication layout with brand illustration on the right side."),
        new("login-03", "Login 03 - Muted Minimalist", "Modern clean layout on muted background with single-click social auth."),
        new("login-04", "Login 04 - Split Side Hero", "Full-height split layout with corporate testimonial and brand quote.")
    };

    private void SelectVariant(string id)
    {
        _activeVariantId = id;
    }

    private async Task CopyCurrentCodeAsync()
    {
        var code = GetCurrentVariantCode();
        await JsApiService.CopyToClipboardAsync(code);
        Snackbar.Add("Code copied to clipboard!", Severity.Success);
    }

    private void SimulateLogin(string variantName)
    {
        Snackbar.Add($"Simulated login submitted for {variantName}!", Severity.Info);
    }

    private void SimulateSocialAuth(string provider)
    {
        Snackbar.Add($"Redirecting to {provider} authentication...", Severity.Info);
    }

    private string GetCurrentVariantCode()
    {
        return _activeVariantId switch
        {
            "login-01" => Login01Code,
            "login-02" => Login02Code,
            "login-03" => Login03Code,
            "login-04" => Login04Code,
            _ => Login01Code
        };
    }

    private sealed record VariantOption(string Id, string Title, string Description);

    private const string Login01Code =
@"@using Vek

<VekCard Outlined=""true"" Class=""pa-8 mx-auto"" Style=""max-width: 420px; width: 100%; border-radius: 12px;"">
    <VekCardContent>
        <VekStack Spacing=""4"">
            <VekStack Spacing=""1"" AlignItems=""AlignItems.Center"">
                <VekText Typo=""Typo.h5"">Welcome back</VekText>
                <VekText Typo=""Typo.body2"" Color=""Color.Secondary"">
                    Enter your email to sign in to your account
                </VekText>
            </VekStack>

            <VekButton Variant=""Variant.Outlined""
                       Color=""Color.Default""
                       FullWidth=""true""
                       StartIcon=""@Icons.Custom.Brands.Google""
                       OnClick=""@(() => LoginWith(""Google""))"">
                Continue with Google
            </VekButton>

            <VekDivider Text=""OR CONTINUE WITH"" />

            <VekStack Spacing=""3"">
                <VekTextField @bind-Value=""_email""
                              Label=""Email""
                              Placeholder=""name@example.com""
                              InputType=""InputType.Email""
                              Variant=""Variant.Outlined""
                              Required=""true"" />

                <VekTextField @bind-Value=""_password""
                              Label=""Password""
                              InputType=""@(_showPassword ? InputType.Text : InputType.Password)""
                              Variant=""Variant.Outlined""
                              Adornment=""Adornment.End""
                              AdornmentIcon=""@(_showPassword ? Icons.Material.Filled.VisibilityOff : Icons.Material.Filled.Visibility)""
                              OnAdornmentClick=""@(() => _showPassword = !_showPassword)""
                              Required=""true"" />

                <VekStack Row=""true"" AlignItems=""AlignItems.Center"" Justify=""Justify.SpaceBetween"">
                    <VekCheckBox @bind-Value=""_rememberMe"" Label=""Remember me"" Dense=""true"" />
                    <VekLink Href=""#"" Underline=""Underline.Hover"" Typo=""Typo.body2"">
                        Forgot password?
                    </VekLink>
                </VekStack>

                <VekButton Variant=""Variant.Filled""
                           Color=""Color.Primary""
                           FullWidth=""true""
                           OnClick=""HandleLogin"">
                    Sign in
                </VekButton>
            </VekStack>

            <VekText Typo=""Typo.body2"" Align=""Align.Center"">
                Don't have an account? <VekLink Href=""#"" Underline=""Underline.Hover"">Sign up</VekLink>
            </VekText>
        </VekStack>
    </VekCardContent>
</VekCard>

@code {
    private string _email = ""m@example.com"";
    private string _password = string.Empty;
    private bool _rememberMe = true;
    private bool _showPassword = false;

    private void HandleLogin()
    {
        // Authenticate user
    }

    private void LoginWith(string provider)
    {
        // External social login
    }
}";

    private const string Login02Code =
@"@using Vek

<VekGrid Spacing=""0"" Class=""mx-auto"" Style=""max-width: 900px; width: 100%; border-radius: 12px; overflow: hidden; border: 1px solid var(--vek-palette-lines-default);"">
    <!-- Form Side -->
    <VekItem xs=""12"" md=""6"" Class=""pa-8 d-flex flex-column justify-center"" Style=""background: var(--vek-palette-surface);"">
        <VekStack Spacing=""4"" Style=""max-width: 360px; margin: 0 auto; width: 100%;"">
            <VekStack Spacing=""1"">
                <VekText Typo=""Typo.h5"">Welcome back</VekText>
                <VekText Typo=""Typo.body2"" Color=""Color.Secondary"">
                    Login to your Acme Inc account
                </VekText>
            </VekStack>

            <VekStack Spacing=""3"">
                <VekTextField @bind-Value=""_email""
                              Label=""Email address""
                              Placeholder=""m@example.com""
                              InputType=""InputType.Email""
                              Variant=""Variant.Outlined""
                              Required=""true"" />

                <VekTextField @bind-Value=""_password""
                              Label=""Password""
                              InputType=""@(_showPassword ? InputType.Text : InputType.Password)""
                              Variant=""Variant.Outlined""
                              Adornment=""Adornment.End""
                              AdornmentIcon=""@(_showPassword ? Icons.Material.Filled.VisibilityOff : Icons.Material.Filled.Visibility)""
                              OnAdornmentClick=""@(() => _showPassword = !_showPassword)""
                              Required=""true"" />

                <VekButton Variant=""Variant.Filled""
                           Color=""Color.Primary""
                           FullWidth=""true""
                           OnClick=""HandleLogin"">
                    Sign In
                </VekButton>
            </VekStack>

            <VekDivider Text=""OR"" />

            <VekStack Spacing=""2"">
                <VekButton Variant=""Variant.Outlined""
                           Color=""Color.Default""
                           FullWidth=""true""
                           StartIcon=""@Icons.Custom.Brands.GitHub""
                           OnClick=""@(() => LoginWith(""GitHub""))"">
                    Login with GitHub
                </VekButton>
                <VekButton Variant=""Variant.Outlined""
                           Color=""Color.Default""
                           FullWidth=""true""
                           StartIcon=""@Icons.Custom.Brands.Google""
                           OnClick=""@(() => LoginWith(""Google""))"">
                    Login with Google
                </VekButton>
            </VekStack>

            <VekText Typo=""Typo.caption"" Align=""Align.Center"" Color=""Color.Secondary"">
                By clicking continue, you agree to our <VekLink Href=""#"" Underline=""Underline.Hover"">Terms of Service</VekLink> and <VekLink Href=""#"" Underline=""Underline.Hover"">Privacy Policy</VekLink>.
            </VekText>
        </VekStack>
    </VekItem>

    <!-- Side Image / Brand Showcase -->
    <VekItem xs=""12"" md=""6"" Class=""pa-10 d-none d-md-flex flex-column justify-space-between"" Style=""background: linear-gradient(145deg, #1e293b, #0f172a); color: white;"">
        <VekStack Row=""true"" AlignItems=""AlignItems.Center"" Spacing=""2"">
            <VekIcon Icon=""@Icons.Material.Filled.AutoAwesome"" Color=""Color.Warning"" />
            <VekText Typo=""Typo.h6"">Acme Corporation</VekText>
        </VekStack>

        <VekStack Spacing=""2"">
            <VekText Typo=""Typo.h5"">""Everything you need to deliver modern apps in minutes.""</VekText>
            <VekText Typo=""Typo.body2"" Style=""opacity: 0.8;"">
                Explore our component system with enterprise accessibility, theming, and responsive layouts built-in.
            </VekText>
        </VekStack>

        <VekText Typo=""Typo.caption"" Style=""opacity: 0.6;"">
            © 2026 Acme Inc. All rights reserved.
        </VekText>
    </VekItem>
</VekGrid>

@code {
    private string _email = string.Empty;
    private string _password = string.Empty;
    private bool _showPassword = false;

    private void HandleLogin() { }
    private void LoginWith(string provider) { }
}";

    private const string Login03Code =
@"@using Vek

<VekCard Outlined=""true"" Class=""pa-8 mx-auto"" Style=""max-width: 440px; width: 100%; border-radius: 16px; background-color: var(--vek-palette-surface);"">
    <VekCardContent>
        <VekStack Spacing=""4"">
            <VekStack Spacing=""1"" AlignItems=""AlignItems.Center"">
                <VekIcon Icon=""@Icons.Material.Filled.Lock"" Color=""Color.Primary"" Size=""Size.Large"" />
                <VekText Typo=""Typo.h5"">Sign In to Platform</VekText>
                <VekText Typo=""Typo.body2"" Color=""Color.Secondary"">
                    Access your cloud dashboard & analytics
                </VekText>
            </VekStack>

            <VekStack Spacing=""3"">
                <VekTextField @bind-Value=""_email""
                              Label=""Work Email""
                              Placeholder=""name@company.com""
                              InputType=""InputType.Email""
                              Variant=""Variant.Outlined""
                              Required=""true"" />

                <VekTextField @bind-Value=""_password""
                              Label=""Password""
                              InputType=""@(_showPassword ? InputType.Text : InputType.Password)""
                              Variant=""Variant.Outlined""
                              Adornment=""Adornment.End""
                              AdornmentIcon=""@(_showPassword ? Icons.Material.Filled.VisibilityOff : Icons.Material.Filled.Visibility)""
                              OnAdornmentClick=""@(() => _showPassword = !_showPassword)""
                              Required=""true"" />

                <VekButton Variant=""Variant.Filled""
                           Color=""Color.Primary""
                           FullWidth=""true""
                           OnClick=""HandleLogin"">
                    Continue with Email
                </VekButton>
            </VekStack>

            <VekDivider Text=""OR CONTINUE WITH"" />

            <VekGrid Spacing=""2"">
                <VekItem xs=""6"">
                    <VekButton Variant=""Variant.Outlined""
                               FullWidth=""true""
                               StartIcon=""@Icons.Custom.Brands.GitHub""
                               OnClick=""@(() => LoginWith(""GitHub""))"">
                        GitHub
                    </VekButton>
                </VekItem>
                <VekItem xs=""6"">
                    <VekButton Variant=""Variant.Outlined""
                               FullWidth=""true""
                               StartIcon=""@Icons.Custom.Brands.Google""
                               OnClick=""@(() => LoginWith(""Google""))"">
                        Google
                    </VekButton>
                </VekItem>
            </VekGrid>
        </VekStack>
    </VekCardContent>
</VekCard>

@code {
    private string _email = string.Empty;
    private string _password = string.Empty;
    private bool _showPassword = false;

    private void HandleLogin() { }
    private void LoginWith(string provider) { }
}";

    private const string Login04Code =
@"@using Vek

<VekGrid Spacing=""0"" Class=""mx-auto"" Style=""max-width: 980px; width: 100%; border-radius: 12px; overflow: hidden; border: 1px solid var(--vek-palette-lines-default);"">
    <!-- Left Hero Side -->
    <VekItem xs=""12"" md=""5"" Class=""pa-10 d-flex flex-column justify-space-between"" Style=""background: #09090b; color: #ffffff;"">
        <VekStack Row=""true"" AlignItems=""AlignItems.Center"" Spacing=""2"">
            <VekIcon Icon=""@Icons.Material.Filled.Shield"" Color=""Color.Info"" />
            <VekText Typo=""Typo.h6"">Vek Platform</VekText>
        </VekStack>

        <VekStack Spacing=""3"">
            <VekCard Outlined=""true"" Class=""pa-4"" Style=""background: rgba(255, 255, 255, 0.05); border-color: rgba(255, 255, 255, 0.15);"">
                <VekCardContent Class=""pa-0"">
                    <VekText Typo=""Typo.body2"" Style=""font-style: italic; color: #f1f5f9;"">
                        ""This library has saved our team countless hours of work. Blazor component authoring feels smooth, modern and rock solid.""
                    </VekText>
                    <VekText Typo=""Typo.caption"" Class=""mt-3 d-block"" Style=""color: #94a3b8;"">
                        Sofia Davis — Engineering Lead
                    </VekText>
                </VekCardContent>
            </VekCard>
        </VekStack>

        <VekText Typo=""Typo.caption"" Style=""color: #64748b;"">
            Designed for high performance Blazor apps.
        </VekText>
    </VekItem>

    <!-- Right Login Form -->
    <VekItem xs=""12"" md=""7"" Class=""pa-8 d-flex flex-column justify-center"" Style=""background: var(--vek-palette-surface);"">
        <VekStack Spacing=""4"" Style=""max-width: 380px; margin: 0 auto; width: 100%;"">
            <VekStack Spacing=""1"">
                <VekText Typo=""Typo.h5"">Create or sign in</VekText>
                <VekText Typo=""Typo.body2"" Color=""Color.Secondary"">
                    Enter your email below to access your workspace
                </VekText>
            </VekStack>

            <VekButton Variant=""Variant.Outlined""
                       FullWidth=""true""
                       StartIcon=""@Icons.Custom.Brands.GitHub""
                       OnClick=""@(() => LoginWith(""GitHub""))"">
                Continue with GitHub
            </VekButton>

            <VekDivider Text=""OR USE EMAIL"" />

            <VekStack Spacing=""3"">
                <VekTextField @bind-Value=""_email""
                              Label=""Email address""
                              Placeholder=""name@company.com""
                              InputType=""InputType.Email""
                              Variant=""Variant.Outlined""
                              Required=""true"" />

                <VekTextField @bind-Value=""_password""
                              Label=""Password""
                              InputType=""@(_showPassword ? InputType.Text : InputType.Password)""
                              Variant=""Variant.Outlined""
                              Adornment=""Adornment.End""
                              AdornmentIcon=""@(_showPassword ? Icons.Material.Filled.VisibilityOff : Icons.Material.Filled.Visibility)""
                              OnAdornmentClick=""@(() => _showPassword = !_showPassword)""
                              Required=""true"" />

                <VekCheckBox @bind-Value=""_terms"" Label=""I agree to the terms and privacy policy"" Dense=""true"" />

                <VekButton Variant=""Variant.Filled""
                           Color=""Color.Primary""
                           FullWidth=""true""
                           OnClick=""HandleLogin"">
                    Sign in with Email
                </VekButton>
            </VekStack>
        </VekStack>
    </VekItem>
</VekGrid>

@code {
    private string _email = string.Empty;
    private string _password = string.Empty;
    private bool _terms = true;
    private bool _showPassword = false;

    private void HandleLogin() { }
    private void LoginWith(string provider) { }
}";
}
