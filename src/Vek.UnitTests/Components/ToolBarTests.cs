// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using AwesomeAssertions;
using Bunit;
using Vek.UnitTests.TestComponents.ToolBar;
using NUnit.Framework;

namespace Vek.UnitTests.Components
{
    [TestFixture]
    public class ToolBarTests : BunitTest
    {
        [Test]
        public void ToolBarWrapContent()
        {
            var component = Context.Render<ToolBarWrapContentTest>();
            var vekToolBar = component.Find(".vek-toolbar");

            vekToolBar.ClassList.Should().Contain("vek-toolbar-wrap-content");
        }

        /// <summary>
        /// ToolBar's WrapContent should be false by default
        /// </summary>
        [Test]
        public void ToolBar_WrapContent_ShouldBeFalseByDefault()
        {
            var comp = Context.Render<VekToolBar>();
            comp.Instance.WrapContent.Should().Be(false);
        }
    }
}
