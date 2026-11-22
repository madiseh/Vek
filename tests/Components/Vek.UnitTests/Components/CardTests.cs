// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components;
using NUnit.Framework;
using Vek.UnitTests.TestComponents.Card;

namespace Vek.UnitTests.Components
{
    [TestFixture]
    public class CardTests : BunitTest
    {
        [Test]
        public async Task CardChildContent()
        {
            //Card header with child content should be render successfully
            var comp = Context.Render<CardChildContentTest>();
            var button = comp.FindComponent<VekButton>();
            var numeric = comp.FindComponent<VekNumericField<int>>();
            await comp.WaitForAssertionAsync(() => numeric.Instance.Value.Should().Be(0));
            await comp.InvokeAsync(() => button.Instance.OnClick.InvokeAsync());
            await comp.WaitForAssertionAsync(() => numeric.Instance.Value.Should().Be(1));
        }

        /// <summary>
        /// A caller-supplied title used to be erased by the trailing null literal.
        /// </summary>
        [Test]
        public void CardMedia_Should_KeepUserSuppliedTitle()
        {
            var comp = Context.Render<VekCardMedia>(parameters => parameters
                .Add(p => p.UserAttributes!, new Dictionary<string, object> { { "title", "Cover art" } }));

            comp.Find("div").GetAttribute("title").Should().Be("Cover art");
        }

        /// <summary>
        /// Aspect ratio controls the media box sizing without changing the default fixed height.
        /// </summary>
        [TestCase("16 / 9")]
        [TestCase("4 / 3")]
        [TestCase("1 / 1")]
        public void CardMedia_AspectRatio_SetsNativeRatio(string ratio)
        {
            var comp = Context.Render<VekCardMedia>(parameters => parameters.Add(p => p.AspectRatio, ratio));

            comp.Find("div").GetAttribute("style").Should().Contain($"aspect-ratio: {ratio};").And.NotContain("height:");
        }

        /// <summary>
        /// The header toggle updates the expanded state and its accessible state.
        /// </summary>
        [Test]
        public async Task CollapsibleCard_Toggle_UpdatesExpandedState()
        {
            bool? changed = null;
            var content = (RenderFragment)(builder =>
            {
                builder.OpenComponent<VekCardHeader>(0);
                builder.CloseComponent();
                builder.OpenComponent<VekCardContent>(1);
                builder.AddAttribute(2, "ChildContent", (RenderFragment)(body => body.AddContent(0, "Card body")));
                builder.CloseComponent();
            });
            var comp = Context.Render<VekCard>(parameters => parameters
                .Add(card => card.Collapsible, true)
                .Add(card => card.ExpandedChanged, EventCallback.Factory.Create<bool>(this, value => changed = value))
                .Add(card => card.ChildContent, content));

            comp.Find("button").GetAttribute("aria-expanded").Should().Be("true");
            comp.FindComponent<VekCollapse>().Markup.Should().Contain("Card body");

            await comp.Find("button").ClickAsync();

            changed.Should().Be(false);
            comp.Find("button").GetAttribute("aria-expanded").Should().Be("false");
            comp.FindComponent<VekCollapse>().Markup.Should().Contain("vek-collapse-exiting");
        }

        /// <summary>
        /// A card without collapsible behavior keeps its content and omits the toggle.
        /// </summary>
        [Test]
        public void Card_IsNotCollapsibleByDefault()
        {
            var comp = Context.Render<VekCard>(parameters => parameters.AddChildContent("Card body"));

            comp.Find(".vek-card").TextContent.Should().Be("Card body");
            comp.FindAll("button").Should().BeEmpty();
            comp.FindComponents<VekCollapse>().Should().BeEmpty();
        }
    }
}
