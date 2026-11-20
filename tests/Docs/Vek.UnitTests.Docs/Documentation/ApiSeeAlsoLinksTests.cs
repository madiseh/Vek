// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using AwesomeAssertions;
using Vek.Docs.Components;
using Vek.Docs.Models;
using NUnit.Framework;

namespace Vek.UnitTests.Docs.Documentation;

/// <summary>
/// Tests for the <see cref="ApiMemberTable"/> component.
/// </summary>
[TestFixture]
public sealed class ApiSeeAlsoLinksTests : BunitTest
{
    /// <summary>
    /// Renders <see cref="ApiMemberTableMode.SeeAlso"/> when see-also links exist.
    /// </summary>
    /// <remarks>
    /// At the time of writing this test, there are see-also links for <see cref="VekButton"/>.
    /// </remarks>
    [Test]
    public void ApiSeeAlsoLinks_RenderSeeAlso_WhenExisting()
    {
        // Get a type with see-also links
        var vekButton = ApiDocumentation.GetType("Vek.VekButton");
        using var comp = Context.Render<ApiSeeAlsoLinks>(parameters => parameters.Add(x => x.Type, vekButton));

        comp.Markup.Should().Contain("<a href=\"/api/VekButtonGroup\"", "There should be a see-also link to VekButtonGroup");

        comp.Markup.Should().Contain("class=\"vek-typography vek-link vek-primary-text vek-link-underline-hover vek-typography-caption docs-link docs-code docs-code-primary\">VekButtonGroup</a>", "There should be a see-also link to VekButtonGroup");

        comp.Markup.Should().NotContain("<div class=\"vek-alert-message\">No see-also links match the current filters.</div>", "There should NOT be a message saying no members are found");
    }

    /// <summary>
    /// Renders the empty state in <see cref="ApiMemberTableMode.SeeAlso"/> when no see-also links exist.
    /// </summary>
    /// <remarks>
    /// At the time of writing this test, there are no see-also links for <see cref="VekAlert"/>.
    /// </remarks>
    [Test]
    public void ApiSeeAlsoLinks_RenderSeeAlso_WhenNotExisting()
    {
        // Get a type with no see-also links
        var vekAlert = ApiDocumentation.GetType("Vek.VekAlert");
        using var comp = Context.Render<ApiSeeAlsoLinks>(parameters => parameters.Add(x => x.Type, vekAlert));

        comp.Markup.Should().NotContain("<div class=\"vek-alert-message\">No see-also links match the current filters.</div>", "the current assertion expects no empty-state message");
    }
}
