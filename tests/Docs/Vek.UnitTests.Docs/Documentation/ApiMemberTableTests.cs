// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using AwesomeAssertions;
using Bunit;
using Vek.Docs.Components;
using Vek.Docs.Models;
using NUnit.Framework;

namespace Vek.UnitTests.Docs.Documentation;

/// <summary>
/// Tests for the <see cref="ApiMemberTable"/> component.
/// </summary>
[TestFixture]
public sealed class ApiMemberTableTests : BunitTest
{
    /// <summary>
    /// Renders the empty-state message for a missing type.
    /// </summary>
    [Test]
    public void ApiMemberTable_RenderMissingType()
    {
        using var comp = Context.Render<ApiMemberTable>(parameters => parameters.Add(x => x.Type, null));

        comp.Markup.Should().Contain("<div class=\"vek-alert-message\">No members match the current filters.</div>", "There should be a message saying no members are found");
    }

    /// <summary>
    /// Renders the empty-state message in <see cref="ApiMemberTableMode.None"/>.
    /// </summary>
    [Test]
    public void ApiMemberTable_RenderNoneMode()
    {
        var vekAlert = ApiDocumentation.GetType("Vek.VekAlert");
        using var comp = Context.Render<ApiMemberTable>(parameters => parameters
            .Add(x => x.Type, vekAlert)
            .Add(x => x.Mode, ApiMemberTableMode.None));

        comp.Markup.Should().Contain("<div class=\"vek-alert-message\">No members match the current filters.</div>", "There should be a message saying no members are found");
    }

    /// <summary>
    /// Renders <see cref="ApiMemberTableMode.Properties"/> with protected properties shown.
    /// </summary>
    [Test]
    public void ApiMemberTable_RenderProperties_WithProtected()
    {
        // Get a type with protected properties
        var vekAlert = ApiDocumentation.GetType("Vek.VekAlert");
        using var comp = Context.Render<ApiMemberTable>(parameters => parameters
            .Add(x => x.Type, vekAlert)
            .Add(x => x.Mode, ApiMemberTableMode.Properties)
            .Add(x => x.ShowProtected, true));

        comp.Markup.Should().NotContain("<div class=\"vek-alert-message\">No members match the current filters.</div>", "There should NOT be a message saying no members are found");

        comp.Markup.Should().Contain("<span class=\"vek-typography vek-typography-body1\">Show Protected</span>", "There should be a switch for protected properties");

        comp.FindAll("td#Classname").Should().ContainSingle("The \"Classname\" protected property should be visible");
    }

    /// <summary>
    /// Renders <see cref="ApiMemberTableMode.Properties"/> with protected properties hidden.
    /// </summary>
    [Test]
    public void ApiMemberTable_RenderProperties_WithoutProtected()
    {
        // Get a type without protected properties
        var vekAlert = ApiDocumentation.GetType("Vek.VekAlert");
        using var comp = Context.Render<ApiMemberTable>(parameters => parameters
            .Add(x => x.Type, vekAlert)
            .Add(x => x.Mode, ApiMemberTableMode.Properties)
            .Add(x => x.ShowProtected, false));

        comp.Markup.Should().NotContain("<div class=\"vek-alert-message\">No members match the current filters.</div>", "There should NOT be a message saying no members are found");

        comp.Markup.Should().Contain("<span class=\"vek-typography vek-typography-body1\">Show Protected</span>", "There should be a switch for protected properties");

        comp.FindAll("td#Classname").Should().BeEmpty("The \"Classname\" protected property should NOT be visible");
    }

    /// <summary>
    /// Renders <see cref="ApiMemberTableMode.Methods"/> with protected methods shown.
    /// </summary>
    [Test]
    public void ApiMemberTable_RenderMethods_WithProtected()
    {
        // Get a type without protected methods
        var vekAutocomplete = ApiDocumentation.GetType("Vek.VekAutocomplete`1");
        using var comp = Context.Render<ApiMemberTable>(parameters => parameters
            .Add(x => x.Type, vekAutocomplete)
            .Add(x => x.Mode, ApiMemberTableMode.Methods)
            .Add(x => x.ShowProtected, true));

        comp.Markup.Should().NotContain("<div class=\"vek-alert-message\">No members match the current filters.</div>", "There should NOT be a message saying no members are found");

        comp.Markup.Should().Contain("<span class=\"vek-typography vek-typography-body1\">Show Protected</span>", "There should be a switch for protected properties");

        comp.FindAll("td#BeginValidateAsync").Should().ContainSingle("The \"BeginValidateAsync\" protected method should be visible");
    }

    /// <summary>
    /// Renders <see cref="ApiMemberTableMode.Methods"/> with protected methods hidden.
    /// </summary>
    [Test]
    public void ApiMemberTable_RenderMethods_WithoutProtected()
    {
        // Get a type without protected methods
        var vekAutocomplete = ApiDocumentation.GetType("Vek.VekAutocomplete`1");
        using var comp = Context.Render<ApiMemberTable>(parameters => parameters
            .Add(x => x.Type, vekAutocomplete)
            .Add(x => x.Mode, ApiMemberTableMode.Methods)
            .Add(x => x.ShowProtected, false));

        comp.Markup.Should().NotContain("<div class=\"vek-alert-message\">No members match the current filters.</div>", "There should NOT be a message saying no members are found");

        comp.Markup.Should().Contain("<span class=\"vek-typography vek-typography-body1\">Show Protected</span>", "There should be a switch for protected properties");

        comp.FindAll("td#BeginValidateAsync").Should().BeEmpty("The \"BeginValidateAsync\" protected method should NOT be visible");
    }

    /// <summary>
    /// Renders <see cref="ApiMemberTableMode.Fields"/> with protected fields shown.
    /// </summary>
    [Test]
    public void ApiMemberTable_RenderFields_WithProtected()
    {
        // Get a type with protected fields
        var vekBaseDatePicker = ApiDocumentation.GetType("Vek.VekBaseDatePicker");
        using var comp = Context.Render<ApiMemberTable>(parameters => parameters
            .Add(x => x.Type, vekBaseDatePicker)
            .Add(x => x.Mode, ApiMemberTableMode.Fields)
            .Add(x => x.ShowProtected, true));

        comp.Markup.Should().NotContain("<div class=\"vek-alert-message\">No members match the current filters.</div>", "There should NOT be a message saying no members are found");

        comp.Markup.Should().Contain("<span class=\"vek-typography vek-typography-body1\">Show Protected</span>", "There should be a switch for protected properties");

        comp.FindAll("td#CurrentView").Should().ContainSingle("The \"CurrentView\" protected field should be visible");
    }

    /// <summary>
    /// Renders <see cref="ApiMemberTableMode.Fields"/> with protected fields hidden.
    /// </summary>
    [Test]
    public void ApiMemberTable_RenderFields_WithoutProtected()
    {
        // Get a type without protected fields
        var vekBaseDatePicker = ApiDocumentation.GetType("Vek.VekBaseDatePicker");
        using var comp = Context.Render<ApiMemberTable>(parameters => parameters
            .Add(x => x.Type, vekBaseDatePicker)
            .Add(x => x.Mode, ApiMemberTableMode.Fields)
            .Add(x => x.ShowProtected, false));

        comp.Markup.Should().Contain("<div class=\"vek-alert-message\">No members match the current filters.</div>", "There should be a message saying no members are found  (since the protected field was the ONLY field)");

        comp.Markup.Should().Contain("<span class=\"vek-typography vek-typography-body1\">Show Protected</span>", "There should be a switch for protected properties");

        comp.FindAll("td#CurrentView").Should().BeEmpty("The \"CurrentView\" protected field should NOT be visible");
    }

    /// <summary>
    /// Renders <see cref="ApiMemberTableMode.Events"/> for a type with events.
    /// </summary>
    /// <remarks>
    /// At the time of writing this test, there are no protected events in the entire Vek library.
    /// </remarks>
    [Test]
    public void ApiMemberTable_RenderEvents()
    {
        // Get a type with events
        var vekDataGrid = ApiDocumentation.GetType("Vek.VekDataGrid`1");
        using var comp = Context.Render<ApiMemberTable>(parameters => parameters
            .Add(x => x.Type, vekDataGrid)
            .Add(x => x.Mode, ApiMemberTableMode.Events));

        comp.Markup.Should().NotContain("<div class=\"vek-alert-message\">No members match the current filters.</div>", "There should NOT be a message saying no members are found");
    }

    /// <summary>
    /// Renders <see cref="ApiMemberTableMode.Globals"/> for a type with globals.
    /// </summary>
    /// <remarks>
    /// At the time of writing this test, there are globals for <see cref="VekMenu"/>.
    /// </remarks>
    [Test]
    public void ApiMemberTable_RenderGlobals_WhenExisting()
    {
        // Get a type with globals
        var vekMenu = ApiDocumentation.GetType("Vek.VekMenu");
        using var comp = Context.Render<ApiMemberTable>(parameters => parameters
            .Add(x => x.Type, vekMenu)
            .Add(x => x.Mode, ApiMemberTableMode.Globals));

        comp.Markup.Should().NotContain("<div class=\"vek-alert-message\">No members match the current filters.</div>", "There should NOT be a message saying no members are found");
    }

    /// <summary>
    /// Renders the empty state in <see cref="ApiMemberTableMode.Globals"/> when no globals exist.
    /// </summary>
    /// <remarks>
    /// At the time of writing this test, there are NO globals for <see cref="VekAlert"/>.
    /// </remarks>
    [Test]
    public void ApiMemberTable_RenderGlobals_WhenNotExisting()
    {
        // Get a type with no globals
        var vekAlert = ApiDocumentation.GetType("Vek.VekAlert");
        using var comp = Context.Render<ApiMemberTable>(parameters => parameters
            .Add(x => x.Type, vekAlert)
            .Add(x => x.Mode, ApiMemberTableMode.Globals));

        comp.Markup.Should().Contain("<div class=\"vek-alert-message\">No members match the current filters.</div>", "There should be a message saying no members are found");
    }
}
