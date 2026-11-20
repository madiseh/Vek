// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using AwesomeAssertions;
using Vek.Docs.Models;
using NUnit.Framework;

namespace Vek.UnitTests.Docs.Documentation;

/// <summary>
/// Tests for <see cref="ApiDocumentation"/> member lookups.
/// </summary>
/// <remarks>
/// Members are built one type at a time, so a lookup by key must build only the type that declares the member.
/// These tests assert that by counting requests for a global build rather than by inspecting what happens to be loaded, which keeps them independent of the order the suite runs in.
/// </remarks>
[TestFixture]
public sealed class ApiDocumentationTests
{
    /// <summary>
    /// Resolves a property reference without building every type.
    /// </summary>
    [Test]
    public void GetMember_Property_BuildsOnlyTheDeclaringType()
    {
        var before = ApiDocumentationMembers.GlobalLoadRequests;

        var member = ApiDocumentation.GetMember("Vek.VekComponentBase.Class");

        member.Should().BeOfType<DocumentedProperty>("There should be a property named Class");
        member.Name.Should().Be("Class");
        member.DeclaringType.Name.Should().Be("VekComponentBase");
        ApiDocumentationMembers.GlobalLoadRequests.Should().Be(before, "A member found by key should not build every type");
    }

    /// <summary>
    /// Resolves a method reference without building every type.
    /// </summary>
    [Test]
    public void GetMember_Method_BuildsOnlyTheDeclaringType()
    {
        var before = ApiDocumentationMembers.GlobalLoadRequests;

        var member = ApiDocumentation.GetMember("Vek.BaseMask.GetCleanText");

        member.Should().BeOfType<DocumentedMethod>("There should be a method named GetCleanText");
        member.Name.Should().Be("GetCleanText");
        member.DeclaringType.Name.Should().Be("BaseMask");
        ApiDocumentationMembers.GlobalLoadRequests.Should().Be(before, "A member found by key should not build every type");
    }

    /// <summary>
    /// Resolves a field reference without building every type.
    /// </summary>
    [Test]
    public void GetMember_Field_BuildsOnlyTheDeclaringType()
    {
        var before = ApiDocumentationMembers.GlobalLoadRequests;

        var member = ApiDocumentation.GetMember("Vek.Adornment.Start");

        member.Should().BeOfType<DocumentedField>("There should be a field named Start");
        member.Name.Should().Be("Start");
        member.DeclaringType.Name.Should().Be("Adornment");
        ApiDocumentationMembers.GlobalLoadRequests.Should().Be(before, "A member found by key should not build every type");
    }

    /// <summary>
    /// Resolves an event reference without building every type.
    /// </summary>
    [Test]
    public void GetMember_Event_BuildsOnlyTheDeclaringType()
    {
        var before = ApiDocumentationMembers.GlobalLoadRequests;

        var member = ApiDocumentation.GetMember("Vek.VekAlert.CloseIconClicked");

        member.Should().BeOfType<DocumentedEvent>("There should be an event named CloseIconClicked");
        member.Name.Should().Be("CloseIconClicked");
        member.DeclaringType.Name.Should().Be("VekAlert");
        ApiDocumentationMembers.GlobalLoadRequests.Should().Be(before, "A member found by key should not build every type");
    }

    /// <summary>
    /// Returns the same instance a type's own collection holds.
    /// </summary>
    [Test]
    public void GetMember_ReturnsTheInstanceTheDeclaringTypeHolds()
    {
        var type = ApiDocumentation.GetType("Vek.VekAlert");

        var member = ApiDocumentation.GetMember("Vek.VekAlert.CloseIconClicked");

        member.Should().BeSameAs(type.Events["CloseIconClicked"], "A member should not be built twice");
    }

    /// <summary>
    /// Links a bindable property and its change event in both directions, wherever the pair is declared.
    /// </summary>
    /// <remarks>
    /// <c>VekPicker`1</c> declares the pair itself.
    /// The date and time pickers inherit it from a closed generic base such as <c>VekPicker&lt;DateTime?&gt;</c>, which is not a documented type.
    /// <c>VekColorPicker</c> declares its own <c>Text</c> but inherits <c>TextChanged</c> from <c>VekPicker&lt;VekColor&gt;</c>.
    /// </remarks>
    [TestCase("Vek.VekPicker`1")]
    [TestCase("Vek.VekDatePicker")]
    [TestCase("Vek.VekDateRangePicker")]
    [TestCase("Vek.VekTimePicker")]
    [TestCase("Vek.VekColorPicker")]
    public void GetType_BindableText_LinksPropertyAndChangeEvent(string typeKey)
    {
        var type = ApiDocumentation.GetType(typeKey);

        var property = type.Properties["Text"];
        var changeEvent = type.Events["TextChanged"];

        property.ChangeEvent.Should().BeSameAs(changeEvent, "Text should be bindable through TextChanged");
        changeEvent.Property.Should().BeSameAs(property, "TextChanged should bind Text");
    }
}
