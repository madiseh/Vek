// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components;
using NUnit.Framework;

namespace Vek.UnitTests.Components
{
    [TestFixture]
    public class ContextMenuTests : BunitTest
    {
        /// <summary>
        /// A selector target registers the browser listeners with the configured long-press delay.
        /// </summary>
        [Test]
        public void TargetSelector_RegistersContextMenuListener()
        {
            Context.Render<VekContextMenu>(parameters => parameters
                .Add(p => p.TargetSelector, "#project-actions")
                .Add(p => p.LongPressDelay, 650));

            var invocation = Context.JSInterop.Invocations.Should().ContainSingle(x => x.Identifier == "vekElementRef.addContextMenuListenerForSelector").Subject;
            invocation.Arguments[0].Should().Be("#project-actions");
            invocation.Arguments[2].Should().Be(650);
        }

        /// <summary>
        /// An element reference takes precedence over a selector when both targets are provided.
        /// </summary>
        [Test]
        public void TargetElement_RegistersElementListener()
        {
            var target = new ElementReference("project-actions");

            Context.Render<VekContextMenu>(parameters => parameters
                .Add(p => p.TargetSelector, "#ignored")
                .Add(p => p.TargetElement, target));

            var invocation = Context.JSInterop.Invocations.Should().ContainSingle(x => x.Identifier == "vekElementRef.addContextMenuListenerForElement").Subject;
            invocation.Arguments[0].Should().Be(target);
        }

        /// <summary>
        /// A disabled context menu does not register browser listeners.
        /// </summary>
        [Test]
        public void Disabled_DoesNotRegisterContextMenuListener()
        {
            Context.Render<VekContextMenu>(parameters => parameters
                .Add(p => p.TargetSelector, "#project-actions")
                .Add(p => p.Disabled, true));

            Context.JSInterop.Invocations.Should().NotContain(x => x.Identifier.StartsWith("vekElementRef.addContextMenuListener", StringComparison.Ordinal));
        }
    }
}
