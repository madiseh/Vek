#nullable enable
using AwesomeAssertions;
using Bunit;
using NUnit.Framework;

namespace Vek.UnitTests.Components.TreeView
{
    [TestFixture]
    public class TreeViewLazyLoadTests : BunitTest
    {
        [Test]
        public async Task LoadChildrenAsync_LoadsChildrenOnExpand()
        {
            var callCount = 0;
            var rootItem = new TreeItemData<string>("Root")
            {
                Text = "Root Node",
                HasChildren = true
            };

            var comp = Context.Render<VekTreeView<string>>(parameters => parameters
                .Add(p => p.Items, new List<TreeItemData<string>> { rootItem })
                .Add(p => p.LoadChildrenAsync, node =>
                {
                    callCount++;
                    return Task.FromResult<IEnumerable<TreeItemData<string>>>(new List<TreeItemData<string>>
                    {
                        new("Child 1") { Text = "Child One" },
                        new("Child 2") { Text = "Child Two" }
                    });
                })
                .Add<VekTreeViewItem<string>, ITreeItemData<string>>(p => p.ItemTemplate, item => itemParams => itemParams
                    .Add(p => p.Value, item.Value)
                    .Add(p => p.Text, item.Text)
                    .Add(p => p.Items, item.Children)
                    .Add(p => p.Expanded, item.Expanded)));

            // Initially root should have expand button because HasChildren is true
            var buttons = comp.FindAll("button.vek-treeview-item-expand-button");
            buttons.Count.Should().Be(1);

            // Click expand
            await buttons[0].ClickAsync();

            // Verify children loaded and rendered
            callCount.Should().Be(1);
            comp.FindAll("li.vek-treeview-item").Count.Should().Be(3);
            comp.FindAll("p.vek-typography").Select(x => x.TextContent).Should().Contain("Child One");
            comp.FindAll("p.vek-typography").Select(x => x.TextContent).Should().Contain("Child Two");
        }

        [Test]
        public async Task LoadChildrenAsync_ShowsLoadingSpinnerWhileFetching()
        {
            var tcs = new TaskCompletionSource<IEnumerable<TreeItemData<string>>>();
            var rootItem = new TreeItemData<string>("Folder")
            {
                Text = "Folder Node",
                HasChildren = true
            };

            var comp = Context.Render<VekTreeView<string>>(parameters => parameters
                .Add(p => p.Items, new List<TreeItemData<string>> { rootItem })
                .Add(p => p.LoadChildrenAsync, _ => tcs.Task)
                .Add<VekTreeViewItem<string>, ITreeItemData<string>>(p => p.ItemTemplate, item => itemParams => itemParams
                    .Add(p => p.Value, item.Value)
                    .Add(p => p.Text, item.Text)
                    .Add(p => p.Items, item.Children)));

            var expandButton = comp.Find("button.vek-treeview-item-expand-button");

            // Click expand while task is incomplete
            var clickTask = expandButton.ClickAsync();

            // Loading state should be active (arrow-load class present)
            comp.FindAll("button.vek-treeview-item-arrow-load").Count.Should().Be(1);

            // Complete loading task
            tcs.SetResult(new List<TreeItemData<string>>
            {
                new("File 1") { Text = "Async File" }
            });
            await clickTask;

            // Loading class should be removed, child rendered
            comp.FindAll("button.vek-treeview-item-arrow-load").Count.Should().Be(0);
            comp.FindAll("li.vek-treeview-item").Count.Should().Be(2);
        }

        [Test]
        public async Task LoadChildrenAsync_CachesChildrenOnCollapseAndReexpand()
        {
            var callCount = 0;
            var rootItem = new TreeItemData<string>("Root")
            {
                Text = "Root Node",
                HasChildren = true
            };

            var comp = Context.Render<VekTreeView<string>>(parameters => parameters
                .Add(p => p.Items, new List<TreeItemData<string>> { rootItem })
                .Add(p => p.LoadChildrenAsync, node =>
                {
                    callCount++;
                    return Task.FromResult<IEnumerable<TreeItemData<string>>>(new List<TreeItemData<string>>
                    {
                        new("Child") { Text = "Child Node" }
                    });
                })
                .Add<VekTreeViewItem<string>, ITreeItemData<string>>(p => p.ItemTemplate, item => itemParams => itemParams
                    .Add(p => p.Value, item.Value)
                    .Add(p => p.Text, item.Text)
                    .Add(p => p.Items, item.Children)));

            // First expansion -> calls delegate
            await comp.Find("button.vek-treeview-item-expand-button").ClickAsync();
            callCount.Should().Be(1);
            comp.FindAll("li.vek-treeview-item").Count.Should().Be(2);

            // Collapse
            await comp.Find("button.vek-treeview-item-expand-button").ClickAsync();
            callCount.Should().Be(1);

            // Re-expand -> cached, does not call delegate again
            await comp.Find("button.vek-treeview-item-expand-button").ClickAsync();
            callCount.Should().Be(1);
            comp.FindAll("li.vek-treeview-item").Count.Should().Be(2);
        }

        [Test]
        public async Task ReloadAsync_ClearsCacheAndRefetchesChildren()
        {
            var callCount = 0;
            var rootItem = new TreeItemData<string>("Root")
            {
                Text = "Root Node",
                HasChildren = true
            };

            var comp = Context.Render<VekTreeView<string>>(parameters => parameters
                .Add(p => p.Items, new List<TreeItemData<string>> { rootItem })
                .Add(p => p.LoadChildrenAsync, node =>
                {
                    callCount++;
                    return Task.FromResult<IEnumerable<TreeItemData<string>>>(new List<TreeItemData<string>>
                    {
                        new($"Child {callCount}") { Text = $"Child Version {callCount}" }
                    });
                })
                .Add<VekTreeViewItem<string>, ITreeItemData<string>>(p => p.ItemTemplate, item => itemParams => itemParams
                    .Add(p => p.Value, item.Value)
                    .Add(p => p.Text, item.Text)
                    .Add(p => p.Items, item.Children)));

            await comp.Find("button.vek-treeview-item-expand-button").ClickAsync();
            callCount.Should().Be(1);
            comp.FindAll("p.vek-typography").Select(x => x.TextContent).Should().Contain("Child Version 1");

            // Reload via item instance
            var itemInstance = comp.FindComponent<VekTreeViewItem<string>>().Instance;
            await comp.InvokeAsync(() => itemInstance.ReloadAsync());

            callCount.Should().Be(2);
            comp.FindAll("p.vek-typography").Select(x => x.TextContent).Should().Contain("Child Version 2");
        }

        [Test]
        public async Task ReloadNodeAsync_OnTree_ClearsCacheAndRefetchesChildren()
        {
            var callCount = 0;
            var rootItem = new TreeItemData<string>("Root")
            {
                Text = "Root Node",
                HasChildren = true
            };

            var comp = Context.Render<VekTreeView<string>>(parameters => parameters
                .Add(p => p.Items, new List<TreeItemData<string>> { rootItem })
                .Add(p => p.LoadChildrenAsync, node =>
                {
                    callCount++;
                    return Task.FromResult<IEnumerable<TreeItemData<string>>>(new List<TreeItemData<string>>
                    {
                        new($"Child {callCount}") { Text = $"Child Version {callCount}" }
                    });
                })
                .Add<VekTreeViewItem<string>, ITreeItemData<string>>(p => p.ItemTemplate, item => itemParams => itemParams
                    .Add(p => p.Value, item.Value)
                    .Add(p => p.Text, item.Text)
                    .Add(p => p.Items, item.Children)));

            await comp.Find("button.vek-treeview-item-expand-button").ClickAsync();
            callCount.Should().Be(1);

            // Reload via tree root method
            var treeInstance = comp.Instance;
            await comp.InvokeAsync(() => treeInstance.ReloadNodeAsync(rootItem));

            callCount.Should().Be(2);
            comp.FindAll("p.vek-typography").Select(x => x.TextContent).Should().Contain("Child Version 2");
        }

        [Test]
        public void HasChildren_Predicate_ControlsChevronVisibility()
        {
            var folder = new TreeItemData<string>("folder") { Text = "Folder" };
            var file = new TreeItemData<string>("file") { Text = "File.txt" };

            var comp = Context.Render<VekTreeView<string>>(parameters => parameters
                .Add(p => p.Items, new List<TreeItemData<string>> { folder, file })
                .Add(p => p.HasChildren, (Func<TreeItemData<string>, bool>)(node => node.Value == "folder"))
                .Add(p => p.LoadChildrenAsync, (Func<TreeItemData<string>, Task<IEnumerable<TreeItemData<string>>>>)(node => Task.FromResult<IEnumerable<TreeItemData<string>>>([])))
                .Add<VekTreeViewItem<string>, ITreeItemData<string>>(p => p.ItemTemplate, item => itemParams => itemParams
                    .Add(p => p.Value, item.Value)
                    .Add(p => p.Text, item.Text)
                    .Add(p => p.Items, item.Children)));

            var items = comp.FindComponents<VekTreeViewItem<string>>();
            items.Count.Should().Be(2);

            // Only folder should have expand button
            comp.FindAll("button.vek-treeview-item-expand-button").Count.Should().Be(1);
            // Leaf item should have empty arrow spacer
            comp.FindAll("div.vek-treeview-item-arrow:not(:has(button))").Count.Should().Be(1);
        }

        [Test]
        public async Task NodeProvider_LoadsChildrenAndDeterminesChildren()
        {
            var callCount = 0;
            var provider = new DelegateTreeNodeProvider<string>(
                loadChildrenAsync: node =>
                {
                    callCount++;
                    return Task.FromResult<IEnumerable<TreeItemData<string>>>(new List<TreeItemData<string>>
                    {
                        new("Sub 1") { Text = "Sub Node 1" }
                    });
                },
                hasChildren: node => node.Value != "leaf");

            var parent = new TreeItemData<string>("parent") { Text = "Parent" };
            var leaf = new TreeItemData<string>("leaf") { Text = "Leaf" };

            var comp = Context.Render<VekTreeView<string>>(parameters => parameters
                .Add(p => p.Items, new List<TreeItemData<string>> { parent, leaf })
                .Add(p => p.NodeProvider, provider)
                .Add<VekTreeViewItem<string>, ITreeItemData<string>>(p => p.ItemTemplate, item => itemParams => itemParams
                    .Add(p => p.Value, item.Value)
                    .Add(p => p.Text, item.Text)
                    .Add(p => p.Items, item.Children)));

            // Only parent has chevron
            comp.FindAll("button.vek-treeview-item-expand-button").Count.Should().Be(1);

            // Expand parent
            await comp.Find("button.vek-treeview-item-expand-button").ClickAsync();

            callCount.Should().Be(1);
            comp.FindAll("li.vek-treeview-item").Count.Should().Be(3);
            comp.FindAll("p.vek-typography").Select(x => x.TextContent).Should().Contain("Sub Node 1");
        }

        [Test]
        public async Task ItemLevel_LoadChildrenAsync_OverridesTreeLevel()
        {
            var treeCallCount = 0;
            var itemCallCount = 0;

            var comp = Context.Render<VekTreeView<string>>(parameters => parameters
                .Add(p => p.LoadChildrenAsync, (Func<TreeItemData<string>, Task<IEnumerable<TreeItemData<string>>>>)(_ =>
                {
                    treeCallCount++;
                    return Task.FromResult<IEnumerable<TreeItemData<string>>>([]);
                }))
                .AddChildContent<VekTreeViewItem<string>>(item => item
                    .Add(p => p.Value, "Root")
                    .Add(p => p.Text, "Root")
                    .Add(p => p.HasChildren, true)
                    .Add(p => p.LoadChildrenAsync, (Func<TreeItemData<string>, Task<IEnumerable<TreeItemData<string>>>>)(_ =>
                    {
                        itemCallCount++;
                        return Task.FromResult<IEnumerable<TreeItemData<string>>>(new List<TreeItemData<string>>
                        {
                            new("Child") { Text = "Custom Child" }
                        });
                    }))));

            await comp.Find("button.vek-treeview-item-expand-button").ClickAsync();

            treeCallCount.Should().Be(0);
            itemCallCount.Should().Be(1);
            comp.FindAll("p.vek-typography").Select(x => x.TextContent).Should().Contain("Custom Child");
        }

        [Test]
        public void ItemLevel_HasChildren_ExplicitSetting()
        {
            var comp = Context.Render<VekTreeView<string>>(parameters => parameters
                .AddChildContent<VekTreeViewItem<string>>(item1 => item1
                    .Add(p => p.Value, "ExplicitTrue")
                    .Add(p => p.Text, "Has Children True")
                    .Add(p => p.HasChildren, true))
                .AddChildContent<VekTreeViewItem<string>>(item2 => item2
                    .Add(p => p.Value, "ExplicitFalse")
                    .Add(p => p.Text, "Has Children False")
                    .Add(p => p.HasChildren, false)));

            var buttons = comp.FindAll("button.vek-treeview-item-expand-button");
            buttons.Count.Should().Be(1);
        }

        [Test]
        public async Task DelegateTreeNodeProvider_CancellationToken_Overload()
        {
            var cancellationTokenPassed = false;
            var provider = new DelegateTreeNodeProvider<string>(
                loadChildrenAsync: (node, ct) =>
                {
                    cancellationTokenPassed = true;
                    return Task.FromResult<IEnumerable<TreeItemData<string>>>(new List<TreeItemData<string>>
                    {
                        new("Item") { Text = "Loaded" }
                    });
                });

            var node = new TreeItemData<string>("Parent") { HasChildren = true };
            provider.HasChildren(node).Should().BeTrue();

            var children = await provider.LoadChildrenAsync(node, CancellationToken.None);
            cancellationTokenPassed.Should().BeTrue();
            children.Should().HaveCount(1);
        }

        [Test]
        public async Task LoadChildrenAsync_Exception_ResetsLoadingState()
        {
            var rootItem = new TreeItemData<string>("ErrorNode")
            {
                Text = "Error Node",
                HasChildren = true
            };

            var comp = Context.Render<VekTreeView<string>>(parameters => parameters
                .Add(p => p.Items, new List<TreeItemData<string>> { rootItem })
                .Add(p => p.LoadChildrenAsync, (Func<TreeItemData<string>, Task<IEnumerable<TreeItemData<string>>>>)(_ => throw new InvalidOperationException("Network failure")))
                .Add<VekTreeViewItem<string>, ITreeItemData<string>>(p => p.ItemTemplate, item => itemParams => itemParams
                    .Add(p => p.Value, item.Value)
                    .Add(p => p.Text, item.Text)
                    .Add(p => p.Items, item.Children)));

            var expandButton = comp.Find("button.vek-treeview-item-expand-button");

            Func<Task> act = () => expandButton.ClickAsync();
            await act.Should().ThrowAsync<InvalidOperationException>();

            // Spinner should not remain stuck in loading state
            comp.FindAll("button.vek-treeview-item-arrow-load").Count.Should().Be(0);
        }
    }
}
