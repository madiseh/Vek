// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using AwesomeAssertions;
using Vek.Docs.Components;
using NUnit.Framework;

namespace Vek.UnitTests.Docs.Documentation;

/// <summary>
/// Tests for the <see cref="ApiTypeLink"/> component.
/// </summary>
[TestFixture]
public sealed class ApiTypeLinkTests : BunitTest
{
    /// <summary>
    /// Renders booleans.
    /// </summary>
    [Test]
    public void ApiTypeLink_Boolean()
    {
        var comp = Context.Render<ApiTypeLink>(parameters => parameters.Add(x => x.TypeName, "System.Boolean"));

        comp.Markup.Should().Be("<code class=\"docs-code docs-code-primary\">bool</code>");
    }

    /// <summary>
    /// Renders boolean arrays.
    /// </summary>
    [Test]
    public void ApiTypeLink_BooleanArray()
    {
        var comp = Context.Render<ApiTypeLink>(parameters => parameters.Add(x => x.TypeName, "System.Boolean[]"));

        comp.Markup.Should().Be("<code class=\"docs-code docs-code-primary\">bool[]</code>");
    }

    /// <summary>
    /// Renders integers.
    /// </summary>
    [Test]
    public void ApiTypeLink_Int32()
    {
        var comp = Context.Render<ApiTypeLink>(parameters => parameters.Add(x => x.TypeName, "System.Int32"));

        comp.Markup.Should().Be("<code class=\"docs-code docs-code-primary\">int</code>");
    }

    /// <summary>
    /// Renders int arrays.
    /// </summary>
    [Test]
    public void ApiTypeLink_Int32Array()
    {
        var comp = Context.Render<ApiTypeLink>(parameters => parameters.Add(x => x.TypeName, "System.Int32[]"));

        comp.Markup.Should().Be("<code class=\"docs-code docs-code-primary\">int[]</code>");
    }

    /// <summary>
    /// Renders longs.
    /// </summary>
    [Test]
    public void ApiTypeLink_Int64()
    {
        var comp = Context.Render<ApiTypeLink>(parameters => parameters.Add(x => x.TypeName, "System.Int64"));

        comp.Markup.Should().Be("<code class=\"docs-code docs-code-primary\">long</code>");
    }

    /// <summary>
    /// Renders long arrays.
    /// </summary>
    [Test]
    public void ApiTypeLink_Int64Array()
    {
        var comp = Context.Render<ApiTypeLink>(parameters => parameters.Add(x => x.TypeName, "System.Int64[]"));

        comp.Markup.Should().Be("<code class=\"docs-code docs-code-primary\">long[]</code>");
    }

    /// <summary>
    /// Renders strings.
    /// </summary>
    [Test]
    public void ApiTypeLink_String()
    {
        var comp = Context.Render<ApiTypeLink>(parameters => parameters.Add(x => x.TypeName, "System.String"));

        comp.Markup.Should().Be("<code class=\"docs-code docs-code-primary\">string</code>");
    }

    /// <summary>
    /// Renders string arrays.
    /// </summary>
    [Test]
    public void ApiTypeLink_StringArray()
    {
        var comp = Context.Render<ApiTypeLink>(parameters => parameters.Add(x => x.TypeName, "System.String[]"));

        comp.Markup.Should().Be("<code class=\"docs-code docs-code-primary\">string[]</code>");
    }

    /// <summary>
    /// Renders doubles.
    /// </summary>
    [Test]
    public void ApiTypeLink_Double()
    {
        var comp = Context.Render<ApiTypeLink>(parameters => parameters.Add(x => x.TypeName, "System.Double"));

        comp.Markup.Should().Be("<code class=\"docs-code docs-code-primary\">double</code>");
    }

    /// <summary>
    /// Renders double arrays.
    /// </summary>
    [Test]
    public void ApiTypeLink_DoubleArray()
    {
        var comp = Context.Render<ApiTypeLink>(parameters => parameters.Add(x => x.TypeName, "System.Double[]"));

        comp.Markup.Should().Be("<code class=\"docs-code docs-code-primary\">double[]</code>");
    }

    /// <summary>
    /// Renders floats.
    /// </summary>
    [Test]
    public void ApiTypeLink_Single()
    {
        var comp = Context.Render<ApiTypeLink>(parameters => parameters.Add(x => x.TypeName, "System.Single"));

        comp.Markup.Should().Be("<code class=\"docs-code docs-code-primary\">float</code>");
    }

    /// <summary>
    /// Renders float arrays.
    /// </summary>
    [Test]
    public void ApiTypeLink_SingleArray()
    {
        var comp = Context.Render<ApiTypeLink>(parameters => parameters.Add(x => x.TypeName, "System.Single[]"));

        comp.Markup.Should().Be("<code class=\"docs-code docs-code-primary\">float[]</code>");
    }

    /// <summary>
    /// Renders objects.
    /// </summary>
    [Test]
    public void ApiTypeLink_Object()
    {
        var comp = Context.Render<ApiTypeLink>(parameters => parameters.Add(x => x.TypeName, "System.Object"));

        comp.Markup.Should().Be("<code class=\"docs-code docs-code-primary\">object</code>");
    }

    /// <summary>
    /// Renders object arrays.
    /// </summary>
    [Test]
    public void ApiTypeLink_ObjectArray()
    {
        var comp = Context.Render<ApiTypeLink>(parameters => parameters.Add(x => x.TypeName, "System.Object[]"));

        comp.Markup.Should().Be("<code class=\"docs-code docs-code-primary\">object[]</code>");
    }

    /// <summary>
    /// Renders void return types.
    /// </summary>
    [Test]
    public void ApiTypeLink_Void()
    {
        var comp = Context.Render<ApiTypeLink>(parameters => parameters.Add(x => x.TypeName, "System.Void"));

        comp.Markup.Should().Be("<code class=\"docs-code docs-code-primary\">void</code>");
    }

    /// <summary>
    /// Renders Vek component links.
    /// </summary>
    [Test]
    public void ApiTypeLink_Vek_Component()
    {
        var comp = Context.Render<ApiTypeLink>(parameters => parameters.Add(x => x.TypeName, "Vek.VekAlert"));

        comp.Markup.Should().Contain("<a href=\"/api/VekAlert\" blazor:onclick=\"6\" class=\"vek-typography vek-link vek-primary-text vek-link-underline-hover vek-typography-caption docs-link docs-code docs-code-primary\">VekAlert</a>", "There should be a link to VekAlert");
    }

    /// <summary>
    /// Renders a link with the requested typography.
    /// </summary>
    [Test]
    public void ApiTypeLink_Typo()
    {
        var comp = Context.Render<ApiTypeLink>(parameters => parameters.Add(x => x.TypeName, "Vek.VekAlert").Add(x => x.Typo, Typo.subtitle1));

        comp.Markup.Should().Contain("vek-typography-subtitle1", "The link should use the requested typography");
    }

    /// <summary>
    /// Renders a link without a tooltip when tooltips are turned off.
    /// </summary>
    [Test]
    public void ApiTypeLink_ShowTooltip_False()
    {
        var comp = Context.Render<ApiTypeLink>(parameters => parameters.Add(x => x.TypeName, "Vek.VekAlert").Add(x => x.ShowTooltip, false));

        comp.Markup.Should().NotContain("vek-tooltip-root", "There should be no tooltip");
        comp.Markup.Should().Contain("<a href=\"/api/VekAlert\"", "There should still be a link to VekAlert");
    }

    /// <summary>
    /// Renders Vek enum links.
    /// </summary>
    [Test]
    public void ApiTypeLink_Vek_Enums()
    {
        var comp = Context.Render<ApiTypeLink>(parameters => parameters.Add(x => x.TypeName, "Vek.Adornment"));

        comp.Markup.Should().Contain("<a href=\"/api/Adornment\"", "There should be a link to Adornment");

        comp.Markup.Should().Contain("class=\"vek-typography vek-link vek-primary-text vek-link-underline-hover vek-typography-caption docs-link docs-code docs-code-primary\">Adornment</a>", "There should be a link to Adornment");
    }

    /// <summary>
    /// Renders links to external Microsoft types.
    /// </summary>
    [Test]
    public void ApiTypeLink_External_MicrosoftType()
    {
        var comp = Context.Render<ApiTypeLink>(parameters => parameters.Add(x => x.TypeName, "Microsoft.AspNetCore.Components.RenderFragment"));

        comp.Markup.Should().Contain("<a href=\"https://learn.microsoft.com/dotnet/api/Microsoft.AspNetCore.Components.RenderFragment\" target=\"_external\" blazor:onclick=\"1\" class=\"vek-typography vek-link vek-primary-text vek-link-underline-hover vek-typography-caption docs-link docs-code docs-code-primary\">", "There should be a link to Microsoft docs");

        comp.Markup.Should().Contain("<svg class=\"vek-icon-root vek-icon-default vek-svg-icon vek-icon-size-small\" style=\"position:relative;top:7px;\" focusable=\"false\" viewBox=\"0 0 24 24\" aria-hidden=\"true\" role=\"img\"><path d=\"M0 0h24v24H0z\" fill=\"none\"/><path d=\"M3.9 12c0-1.71 1.39-3.1 3.1-3.1h4V7H7c-2.76 0-5 2.24-5 5s2.24 5 5 5h4v-1.9H7c-1.71 0-3.1-1.39-3.1-3.1zM8 13h8v-2H8v2zm9-6h-4v1.9h4c1.71 0 3.1 1.39 3.1 3.1s-1.39 3.1-3.1 3.1h-4V17h4c2.76 0 5-2.24 5-5s-2.24-5-5-5z\"/></svg>", "There should be a Link icon");
    }

    /// <summary>
    /// Renders links to external system types.
    /// </summary>
    [Test]
    public void ApiTypeLink_External_SystemType()
    {
        var comp = Context.Render<ApiTypeLink>(parameters => parameters.Add(x => x.TypeName, "System.Guid"));

        comp.Markup.Should().Contain("<a href=\"https://learn.microsoft.com/dotnet/api/System.Guid\" target=\"_external\" blazor:onclick=\"1\" class=\"vek-typography vek-link vek-primary-text vek-link-underline-hover vek-typography-caption docs-link docs-code docs-code-primary\">", "There should be a link to Microsoft docs");

        comp.Markup.Should().Contain("<svg class=\"vek-icon-root vek-icon-default vek-svg-icon vek-icon-size-small\" style=\"position:relative;top:7px;\" focusable=\"false\" viewBox=\"0 0 24 24\" aria-hidden=\"true\" role=\"img\"><path d=\"M0 0h24v24H0z\" fill=\"none\"/><path d=\"M3.9 12c0-1.71 1.39-3.1 3.1-3.1h4V7H7c-2.76 0-5 2.24-5 5s2.24 5 5 5h4v-1.9H7c-1.71 0-3.1-1.39-3.1-3.1zM8 13h8v-2H8v2zm9-6h-4v1.9h4c1.71 0 3.1 1.39 3.1 3.1s-1.39 3.1-3.1 3.1h-4V17h4c2.76 0 5-2.24 5-5s-2.24-5-5-5z\"/></svg>", "There should be a Link icon");
    }
}
