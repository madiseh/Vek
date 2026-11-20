// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components.Web;
using NUnit.Framework;
using Vek.UnitTests.TestComponents.Alert;

namespace Vek.UnitTests.Components
{
    [TestFixture]
    public class AlertTests : BunitTest
    {
        [Test]
        public async Task Alert()
        {
            var comp = Context.Render<VekAlert>(parameters => parameters.Add(x => x.Icon, Icons.Custom.Brands.Vek));

            await comp.InvokeAsync(() => comp.Instance.OnCloseIconClickAsync());
            await comp.WaitForAssertionAsync(() => comp.Instance.Icon.Should().Be(Icons.Custom.Brands.Vek));
        }

        [Test]
        public async Task Alert_Click()
        {
            var comp = Context.Render<AlertClickTest>();
            var alert = comp.FindComponent<VekAlert>();
            var numeric = comp.FindComponent<VekNumericField<int>>();
            await comp.WaitForAssertionAsync(() => numeric.Instance.Value.Should().Be(0));
            await comp.InvokeAsync(() => alert.Instance.OnCloseIconClickAsync());
            await comp.WaitForAssertionAsync(() => numeric.Instance.Value.Should().Be(1));
        }

        [Test]
        public void Alert_DefaultContentAlignment_ShouldBeStart()
        {
            var comp = Context.Render<VekAlert>();
            comp.Instance.ContentAlignment.Should().Be(HorizontalAlignment.Start);
        }

        [TestCase(HorizontalAlignment.Start, false, "justify-start")]
        [TestCase(HorizontalAlignment.Start, true, "justify-start")]
        [TestCase(HorizontalAlignment.Right, false, "justify-end")]
        [TestCase(HorizontalAlignment.Right, true, "justify-start")]
        [TestCase(HorizontalAlignment.Left, false, "justify-start")]
        [TestCase(HorizontalAlignment.Left, true, "justify-end")]
        public void Alert_ContentAlignment_RendersCorrectJustifyClass(HorizontalAlignment contentAlignment, bool rightToLeft, string expectedClass)
        {
            var comp = Context.Render<VekAlert>(parameters =>
            {
                parameters.AddCascadingValue("RightToLeft", rightToLeft);
                parameters.Add(x => x.ContentAlignment, contentAlignment);
            });

            var positionDiv = comp.Find(".vek-alert-position");
            positionDiv.ClassList.Should().Contain(expectedClass);
        }

        [Test]
        public void Alert_RootClass_ShouldReflectAppearanceParameters()
        {
            var comp = Context.Render<VekAlert>(parameters => parameters
                .Add(x => x.Severity, Severity.Warning)
                .Add(x => x.Variant, Variant.Filled)
                .Add(x => x.Dense, true)
                .Add(x => x.Square, true)
                .Add(x => x.Elevation, 4)
                .Add(x => x.Class, "custom-alert"));

            var alert = comp.Find(".vek-alert");

            alert.ClassList.Should().Contain("vek-alert-filled-warning");
            alert.ClassList.Should().Contain("vek-dense");
            alert.ClassList.Should().Contain("vek-square");
            alert.ClassList.Should().Contain("vek-elevation-4");
            alert.ClassList.Should().Contain("custom-alert");
        }

        [Test]
        public void Alert_NoIcon_ShouldNotRenderIcon()
        {
            var comp = Context.Render<VekAlert>(parameters => parameters
                .Add(x => x.NoIcon, true)
                .AddChildContent("Alert content"));

            comp.FindAll(".vek-alert-icon").Should().BeEmpty();
            comp.Find(".vek-alert-message").TextContent.Should().Be("Alert content");
        }

        [Test]
        public async Task Alert_ShowCloseIcon_ShouldRenderButtonAndInvokeCallback()
        {
            var callbackCount = 0;
            VekAlert capturedAlert = null!;
            var comp = Context.Render<VekAlert>(parameters => parameters
                .Add(x => x.ShowCloseIcon, true)
                .Add(x => x.CloseIcon, Icons.Material.Filled.Add)
                .Add(x => x.CloseIconClicked, (VekAlert alert) =>
                {
                    callbackCount++;
                    capturedAlert = alert;
                }));

            var closeButton = comp.Find("button.vek-alert-close-button");

            comp.Markup.Should().Contain(Icons.Material.Filled.Add);
            closeButton.GetAttribute("aria-label").Should().NotBeNullOrEmpty();

            await closeButton.ClickAsync(new MouseEventArgs());

            callbackCount.Should().Be(1);
            capturedAlert.Should().BeSameAs(comp.Instance);
        }

        [Test]
        public async Task Alert_Click_ShouldInvokeOnClickCallback()
        {
            var clickCount = 0;
            MouseEventArgs capturedArgs = null!;
            var comp = Context.Render<VekAlert>(parameters => parameters
                .Add(x => x.OnClick, (MouseEventArgs args) =>
                {
                    clickCount++;
                    capturedArgs = args;
                }));

            await comp.Find(".vek-alert").ClickAsync(new MouseEventArgs { ClientX = 12, ClientY = 24 });

            clickCount.Should().Be(1);
            capturedArgs.Should().NotBeNull();
            capturedArgs.ClientX.Should().Be(12);
            capturedArgs.ClientY.Should().Be(24);
        }

    }
}
