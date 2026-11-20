#nullable enable
using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components.Web;
using NUnit.Framework;

namespace Vek.UnitTests.Components
{
    [TestFixture]
    public class TreeViewDragDropTests : BunitTest
    {
        [Test]
        public void AllowDragDrop_FalseByDefault_NoDraggableAttribute()
        {
            var comp = Context.Render<VekTreeView<string>>(parameters => parameters
                .AddChildContent<VekTreeViewItem<string>>(item => item
                    .Add(p => p.Text, "Node 1")
                    .Add(p => p.Value, "1")));

            var li = comp.Find("li.vek-treeview-item");
            li.HasAttribute("draggable").Should().BeFalse();
        }

        [Test]
        public void AllowDragDrop_True_RendersDraggableAttribute()
        {
            var comp = Context.Render<VekTreeView<string>>(parameters => parameters
                .Add(p => p.AllowDragDrop, true)
                .AddChildContent<VekTreeViewItem<string>>(item => item
                    .Add(p => p.Text, "Node 1")
                    .Add(p => p.Value, "1")));

            var li = comp.Find("li.vek-treeview-item");
            li.GetAttribute("draggable").Should().Be("true");
        }

        [Test]
        public void AllowDragDrop_DisabledItem_NotDraggable()
        {
            var comp = Context.Render<VekTreeView<string>>(parameters => parameters
                .Add(p => p.AllowDragDrop, true)
                .AddChildContent<VekTreeViewItem<string>>(item => item
                    .Add(p => p.Text, "Node 1")
                    .Add(p => p.Value, "1")
                    .Add(p => p.Disabled, true)));

            var li = comp.Find("li.vek-treeview-item");
            li.HasAttribute("draggable").Should().BeFalse();
        }

        [Test]
        public async Task DropNodeAsync_InvokesOnNodeDrop_WithCorrectPositions()
        {
            TreeNodeDropEventArgs<string>? dropArgs = null;

            var comp = Context.Render<VekTreeView<string>>(parameters => parameters
                .Add(p => p.AllowDragDrop, true)
                .Add(p => p.OnNodeDrop, (TreeNodeDropEventArgs<string> e) => dropArgs = e)
                .AddChildContent(builder =>
                {
                    builder.OpenComponent<VekTreeViewItem<string>>(0);
                    builder.AddAttribute(1, "Text", "Item A");
                    builder.AddAttribute(2, "Value", "A");
                    builder.CloseComponent();

                    builder.OpenComponent<VekTreeViewItem<string>>(3);
                    builder.AddAttribute(4, "Text", "Item B");
                    builder.AddAttribute(5, "Value", "B");
                    builder.CloseComponent();
                }));

            var items = comp.FindComponents<VekTreeViewItem<string>>();
            items.Count.Should().Be(2);

            var itemA = items[0].Instance;
            var itemB = items[1].Instance;

            // Before
            var success = await comp.Instance.DropNodeAsync(itemA, itemB, TreeItemDropPosition.Before);
            success.Should().BeTrue();
            dropArgs.Should().NotBeNull();
            dropArgs!.SourceValue.Should().Be("A");
            dropArgs.TargetValue.Should().Be("B");
            dropArgs.DropPosition.Should().Be(TreeItemDropPosition.Before);

            // Into
            success = await comp.Instance.DropNodeAsync(itemA, itemB, TreeItemDropPosition.Into);
            success.Should().BeTrue();
            dropArgs.DropPosition.Should().Be(TreeItemDropPosition.Into);

            // After
            success = await comp.Instance.DropNodeAsync(itemA, itemB, TreeItemDropPosition.After);
            success.Should().BeTrue();
            dropArgs.DropPosition.Should().Be(TreeItemDropPosition.After);
        }

        [Test]
        public async Task DropNodeAsync_DisallowsDroppingOntoSelfOrDescendant()
        {
            var comp = Context.Render<VekTreeView<string>>(parameters => parameters
                .Add(p => p.AllowDragDrop, true)
                .AddChildContent(builder =>
                {
                    builder.OpenComponent<VekTreeViewItem<string>>(0);
                    builder.AddAttribute(1, "Text", "Parent");
                    builder.AddAttribute(2, "Value", "P");
                    builder.AddAttribute(3, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)(b =>
                    {
                        b.OpenComponent<VekTreeViewItem<string>>(0);
                        b.AddAttribute(1, "Text", "Child");
                        b.AddAttribute(2, "Value", "C");
                        b.CloseComponent();
                    }));
                    builder.CloseComponent();
                }));

            var items = comp.FindComponents<VekTreeViewItem<string>>();
            var parent = items[0].Instance;
            var child = items[1].Instance;

            // Cannot drop onto self
            var success = await comp.Instance.DropNodeAsync(parent, parent, TreeItemDropPosition.Before);
            success.Should().BeFalse();

            // Cannot drop parent into its child (circular reparenting)
            success = await comp.Instance.DropNodeAsync(parent, child, TreeItemDropPosition.Into);
            success.Should().BeFalse();
        }

        [Test]
        public async Task DropNodeAsync_CanDropFunc_CanPreventDrop()
        {
            var comp = Context.Render<VekTreeView<string>>(parameters => parameters
                .Add(p => p.AllowDragDrop, true)
                .Add(p => p.CanDropFunc, (TreeNodeDropEventArgs<string> e) => e.DropPosition != TreeItemDropPosition.Into)
                .AddChildContent(builder =>
                {
                    builder.OpenComponent<VekTreeViewItem<string>>(0);
                    builder.AddAttribute(1, "Text", "Node 1");
                    builder.AddAttribute(2, "Value", "1");
                    builder.CloseComponent();

                    builder.OpenComponent<VekTreeViewItem<string>>(3);
                    builder.AddAttribute(4, "Text", "Node 2");
                    builder.AddAttribute(5, "Value", "2");
                    builder.CloseComponent();
                }));

            var items = comp.FindComponents<VekTreeViewItem<string>>();
            var item1 = items[0].Instance;
            var item2 = items[1].Instance;

            // Into should be disallowed by CanDropFunc
            var success = await comp.Instance.DropNodeAsync(item1, item2, TreeItemDropPosition.Into);
            success.Should().BeFalse();

            // Sibling reorder before should be allowed
            success = await comp.Instance.DropNodeAsync(item1, item2, TreeItemDropPosition.Before);
            success.Should().BeTrue();
        }

        [Test]
        public void DragEvents_TriggerVisualClasses()
        {
            var comp = Context.Render<VekTreeView<string>>(parameters => parameters
                .Add(p => p.AllowDragDrop, true)
                .AddChildContent(builder =>
                {
                    builder.OpenComponent<VekTreeViewItem<string>>(0);
                    builder.AddAttribute(1, "Text", "Node 1");
                    builder.AddAttribute(2, "Value", "1");
                    builder.CloseComponent();

                    builder.OpenComponent<VekTreeViewItem<string>>(3);
                    builder.AddAttribute(4, "Text", "Node 2");
                    builder.AddAttribute(5, "Value", "2");
                    builder.CloseComponent();
                }));

            comp.FindAll("li.vek-treeview-item").Count.Should().Be(2);

            // Start dragging first item
            comp.FindAll("li.vek-treeview-item")[0].DragStart();
            comp.FindAll("li.vek-treeview-item")[0].ClassList.Should().Contain("vek-treeview-item--dragging");

            // Drag over second item at top zone (Before)
            comp.FindAll("li.vek-treeview-item")[1].DragOver(new DragEventArgs { OffsetY = 4 });
            comp.FindAll("li.vek-treeview-item")[1].ClassList.Should().Contain("vek-treeview-item--drag-over-before");

            // Drag over second item at bottom zone (After)
            comp.FindAll("li.vek-treeview-item")[1].DragOver(new DragEventArgs { OffsetY = 28 });
            comp.FindAll("li.vek-treeview-item")[1].ClassList.Should().Contain("vek-treeview-item--drag-over-after");

            // Drag over second item in middle zone (Into)
            comp.FindAll("li.vek-treeview-item")[1].DragOver(new DragEventArgs { OffsetY = 16 });
            comp.FindAll("li.vek-treeview-item")[1].ClassList.Should().Contain("vek-treeview-item--drag-over-into");

            // Drag leave clears drop cues
            comp.FindAll("li.vek-treeview-item")[1].DragLeave();
            comp.FindAll("li.vek-treeview-item")[1].ClassList.Should().NotContain("vek-treeview-item--drag-over-into");

            // Drag end clears dragging state
            comp.FindAll("li.vek-treeview-item")[0].DragEnd();
            comp.FindAll("li.vek-treeview-item")[0].ClassList.Should().NotContain("vek-treeview-item--dragging");
        }
    }
}
