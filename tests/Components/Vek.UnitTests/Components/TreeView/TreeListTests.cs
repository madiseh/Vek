// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable enable
using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using NUnit.Framework;

namespace Vek.UnitTests.Components.TreeView
{
    [TestFixture]
    public class TreeListTests : BunitTest
    {
        private sealed record ProjectNode(string Name, string Owner, int Tasks, long SizeBytes);

        private static List<TreeItemData<ProjectNode>> CreateSampleTree()
        {
            return
            [
                new TreeItemData<ProjectNode>(new ProjectNode("Platform", "Alice", 12, 4096))
                {
                    Text = "Platform",
                    Icon = Icons.Material.Filled.Folder,
                    Expanded = true,
                    Children =
                    [
                        new TreeItemData<ProjectNode>(new ProjectNode("Auth Service", "Bob", 5, 1024))
                        {
                            Text = "Auth Service",
                            Icon = Icons.Material.Filled.Code,
                            HasChildren = false
                        },
                        new TreeItemData<ProjectNode>(new ProjectNode("Gateway", "Cara", 7, 2048))
                        {
                            Text = "Gateway",
                            Icon = Icons.Material.Filled.Code,
                            HasChildren = false
                        }
                    ]
                },
                new TreeItemData<ProjectNode>(new ProjectNode("Analytics", "Dan", 4, 8192))
                {
                    Text = "Analytics",
                    Icon = Icons.Material.Filled.Folder,
                    Expanded = false,
                    Children =
                    [
                        new TreeItemData<ProjectNode>(new ProjectNode("ETL Pipeline", "Eve", 4, 512))
                        {
                            Text = "ETL Pipeline",
                            Icon = Icons.Material.Filled.Description,
                            HasChildren = false
                        }
                    ]
                }
            ];
        }

        private IRenderedComponent<VekTreeList<ProjectNode>> RenderSampleTreeList(
            List<TreeItemData<ProjectNode>>? items = null,
            Action<ComponentParameterCollectionBuilder<VekTreeList<ProjectNode>>>? configure = null)
        {
            items ??= CreateSampleTree();
            return Context.Render<VekTreeList<ProjectNode>>(p =>
            {
                p.Add(x => x.Items, items);
                p.Add(x => x.Columns, (RenderFragment)(col =>
                {
                    col.OpenComponent<VekTreeListColumn<ProjectNode>>(0);
                    col.AddAttribute(1, nameof(VekTreeListColumn<ProjectNode>.Title), "Name");
                    col.AddAttribute(2, nameof(VekTreeListColumn<ProjectNode>.Field), (Func<ProjectNode, object?>)(n => n.Name));
                    col.AddAttribute(3, nameof(VekTreeListColumn<ProjectNode>.Width), "240px");
                    col.CloseComponent();

                    col.OpenComponent<VekTreeListColumn<ProjectNode>>(4);
                    col.AddAttribute(5, nameof(VekTreeListColumn<ProjectNode>.Title), "Owner");
                    col.AddAttribute(6, nameof(VekTreeListColumn<ProjectNode>.Field), (Func<ProjectNode, object?>)(n => n.Owner));
                    col.AddAttribute(7, nameof(VekTreeListColumn<ProjectNode>.Width), "140px");
                    col.CloseComponent();

                    col.OpenComponent<VekTreeListColumn<ProjectNode>>(8);
                    col.AddAttribute(9, nameof(VekTreeListColumn<ProjectNode>.Title), "Tasks");
                    col.AddAttribute(10, nameof(VekTreeListColumn<ProjectNode>.Field), (Func<ProjectNode, object?>)(n => n.Tasks));
                    col.AddAttribute(11, nameof(VekTreeListColumn<ProjectNode>.Format), "N0");
                    col.AddAttribute(12, nameof(VekTreeListColumn<ProjectNode>.Width), "100px");
                    col.AddAttribute(13, nameof(VekTreeListColumn<ProjectNode>.Alignment), HorizontalAlignment.End);
                    col.CloseComponent();
                }));
                configure?.Invoke(p);
            });
        }

        [Test]
        public void Renders_Columns_TabularCells_And_HierarchyIndent()
        {
            var comp = RenderSampleTreeList();

            var headers = comp.FindAll("th[role='columnheader']");
            headers.Count.Should().Be(3);
            headers[0].TextContent.Should().Contain("Name");
            headers[1].TextContent.Should().Contain("Owner");
            headers[2].TextContent.Should().Contain("Tasks");

            // Platform (expanded) + 2 children + Analytics (collapsed) = 4 visible rows
            var rows = comp.FindAll("tbody tr[role='row']");
            rows.Count.Should().Be(4);

            // Primary column has indentation at depth 1 (20px default)
            var depthZeroCell = rows[0].QuerySelector(".vek-treelist-tree-cell");
            depthZeroCell!.GetAttribute("style").Should().BeNullOrEmpty();

            var depthOneCell = rows[1].QuerySelector(".vek-treelist-tree-cell");
            depthOneCell!.GetAttribute("style").Should().Contain("padding-inline-start: 20px");

            // Tabular values align across columns
            var secondRowCells = rows[1].QuerySelectorAll("td[role='gridcell']");
            secondRowCells.Length.Should().Be(3);
            secondRowCells[0].TextContent.Should().Contain("Auth Service");
            secondRowCells[1].TextContent.Trim().Should().Be("Bob");
            secondRowCells[2].TextContent.Trim().Should().Be("5");
        }

        [Test]
        public void Explicit_IsTreeColumn_Moves_Indent_And_Toggle_To_Target_Column()
        {
            var items = CreateSampleTree();
            var comp = Context.Render<VekTreeList<ProjectNode>>(p =>
            {
                p.Add(x => x.Items, items);
                p.Add(x => x.Columns, (RenderFragment)(col =>
                {
                    col.OpenComponent<VekTreeListColumn<ProjectNode>>(0);
                    col.AddAttribute(1, nameof(VekTreeListColumn<ProjectNode>.Title), "Owner");
                    col.AddAttribute(2, nameof(VekTreeListColumn<ProjectNode>.Field), (Func<ProjectNode, object?>)(n => n.Owner));
                    col.CloseComponent();

                    col.OpenComponent<VekTreeListColumn<ProjectNode>>(3);
                    col.AddAttribute(4, nameof(VekTreeListColumn<ProjectNode>.Title), "Project");
                    col.AddAttribute(5, nameof(VekTreeListColumn<ProjectNode>.Field), (Func<ProjectNode, object?>)(n => n.Name));
                    col.AddAttribute(6, nameof(VekTreeListColumn<ProjectNode>.IsTreeColumn), true);
                    col.CloseComponent();
                }));
            });

            var firstRowCells = comp.FindAll("tbody tr[role='row']")[0].QuerySelectorAll("td[role='gridcell']");
            firstRowCells[0].QuerySelector(".vek-treelist-tree-cell").Should().BeNull();
            firstRowCells[1].QuerySelector(".vek-treelist-tree-cell").Should().NotBeNull();
            firstRowCells[1].QuerySelector(".vek-treelist-toggle-btn").Should().NotBeNull();
        }

        [Test]
        public void Column_Templates_And_Visibility_Render_Correctly()
        {
            var items = CreateSampleTree();
            var comp = Context.Render<VekTreeList<ProjectNode>>(p =>
            {
                p.Add(x => x.Items, items);
                p.Add(x => x.Columns, (RenderFragment)(col =>
                {
                    col.OpenComponent<VekTreeListColumn<ProjectNode>>(0);
                    col.AddAttribute(1, nameof(VekTreeListColumn<ProjectNode>.Title), "Name");
                    col.AddAttribute(2, nameof(VekTreeListColumn<ProjectNode>.HeaderTemplate),
                        (RenderFragment<VekTreeListColumn<ProjectNode>>)(c => b => b.AddContent(0, $"Custom:{c.Title}")));
                    col.AddAttribute(3, nameof(VekTreeListColumn<ProjectNode>.CellTemplate),
                        (RenderFragment<ITreeItemData<ProjectNode>>)(item => b => b.AddContent(0, $"[{item.Value?.Name}]")));
                    col.CloseComponent();

                    col.OpenComponent<VekTreeListColumn<ProjectNode>>(4);
                    col.AddAttribute(5, nameof(VekTreeListColumn<ProjectNode>.Title), "HiddenCol");
                    col.AddAttribute(6, nameof(VekTreeListColumn<ProjectNode>.Visible), false);
                    col.CloseComponent();
                }));
            });

            comp.FindAll("th[role='columnheader']").Count.Should().Be(1);
            comp.Find("th[role='columnheader']").TextContent.Should().Contain("Custom:Name");
            comp.Find("tbody tr td .vek-treelist-cell-content").TextContent.Should().Be("[Platform]");
        }

        [Test]
        public void EmptyContent_Renders_When_Items_Empty()
        {
            var comp = Context.Render<VekTreeList<ProjectNode>>(p =>
            {
                p.Add(x => x.Items, Array.Empty<ITreeItemData<ProjectNode>>());
                p.Add(x => x.EmptyContent, (RenderFragment)(b => b.AddContent(0, "No nodes found")));
                p.Add(x => x.Columns, (RenderFragment)(col =>
                {
                    col.OpenComponent<VekTreeListColumn<ProjectNode>>(0);
                    col.AddAttribute(1, nameof(VekTreeListColumn<ProjectNode>.Title), "Name");
                    col.CloseComponent();
                }));
            });

            comp.Find(".vek-treelist-empty-cell").TextContent.Trim().Should().Be("No nodes found");
        }

        [Test]
        public async Task ToggleButton_Expands_And_Collapses_Without_Changing_Selection()
        {
            ProjectNode? selected = null;
            var comp = RenderSampleTreeList(configure: p => p
                .Add(x => x.SelectionMode, SelectionMode.SingleSelection)
                .Add(x => x.SelectedValueChanged, EventCallback.Factory.Create<ProjectNode?>(this, v => selected = v)));

            comp.FindAll("tbody tr[role='row']").Count.Should().Be(4);

            // Expand Analytics (2nd top-level row, index 3)
            var rows = comp.FindAll("tbody tr[role='row']");
            var analyticsToggle = rows[3].QuerySelector("button.vek-treelist-toggle-btn");
            analyticsToggle.Should().NotBeNull();

            await analyticsToggle!.ClickAsync();

            comp.FindAll("tbody tr[role='row']").Count.Should().Be(5);
            comp.FindAll("tbody tr[role='row']")[4].TextContent.Should().Contain("ETL Pipeline");
            selected.Should().BeNull();

            // Collapse Platform
            await comp.FindAll("tbody tr[role='row']")[0].QuerySelector("button.vek-treelist-toggle-btn")!.ClickAsync();
            comp.FindAll("tbody tr[role='row']").Count.Should().Be(3);
        }

        [Test]
        public async Task ExpandAllAsync_And_CollapseAllAsync_Work_Recursively()
        {
            var comp = RenderSampleTreeList();

            await comp.InvokeAsync(() => comp.Instance.ExpandAllAsync());
            comp.FindAll("tbody tr[role='row']").Count.Should().Be(5);

            await comp.InvokeAsync(() => comp.Instance.CollapseAllAsync());
            comp.FindAll("tbody tr[role='row']").Count.Should().Be(2);
        }

        [Test]
        public async Task ExpandOnClick_And_ExpandOnDoubleClick_Behave_Correctly()
        {
            var comp = RenderSampleTreeList(configure: p => p
                .Add(x => x.ExpandOnClick, true)
                .Add(x => x.ExpandOnDoubleClick, true));

            // Click alone should NOT expand because ExpandOnDoubleClick takes precedence
            await comp.FindAll("tbody tr[role='row']")[3].ClickAsync();
            comp.FindAll("tbody tr[role='row']").Count.Should().Be(4);

            // Double-click expands Analytics
            await comp.FindAll("tbody tr[role='row']")[3].DoubleClickAsync();
            comp.FindAll("tbody tr[role='row']").Count.Should().Be(5);
        }

        [Test]
        public async Task LoadChildrenAsync_Fetches_On_Expand_And_Caches()
        {
            var calls = 0;
            var items = new List<TreeItemData<ProjectNode>>
            {
                new(new ProjectNode("Root", "Alice", 1, 100)) { Text = "Root", HasChildren = true }
            };

            var comp = RenderSampleTreeList(items, p => p.Add(x => x.LoadChildrenAsync, node =>
            {
                calls++;
                return Task.FromResult<IEnumerable<ITreeItemData<ProjectNode>>>(
                [
                    new TreeItemData<ProjectNode>(new ProjectNode("Lazy Child", "Bob", 2, 200))
                    {
                        Text = "Lazy Child",
                        HasChildren = false
                    }
                ]);
            }));

            comp.FindAll("tbody tr[role='row']").Count.Should().Be(1);

            await comp.Find("button.vek-treelist-toggle-btn").ClickAsync();
            calls.Should().Be(1);
            comp.FindAll("tbody tr[role='row']").Count.Should().Be(2);

            // Collapse and re-expand -> cached
            await comp.Find("button.vek-treelist-toggle-btn").ClickAsync();
            comp.FindAll("tbody tr[role='row']").Count.Should().Be(1);
            await comp.Find("button.vek-treelist-toggle-btn").ClickAsync();
            calls.Should().Be(1);
            comp.FindAll("tbody tr[role='row']").Count.Should().Be(2);
        }

        [Test]
        public async Task HeaderSort_Sorts_Each_Hierarchy_Level_And_Cycles_Directions()
        {
            TreeListSortEventArgs<ProjectNode>? lastSort = null;
            var comp = RenderSampleTreeList(configure: p => p
                .Add(x => x.OnSortChanged, EventCallback.Factory.Create<TreeListSortEventArgs<ProjectNode>>(this, e => lastSort = e)));

            // Sort by Name ascending: Analytics before Platform, and within Platform: Auth Service before Gateway
            await comp.FindAll("button.vek-treelist-sort-btn")[0].ClickAsync();

            lastSort.Should().NotBeNull();
            lastSort!.SortDirection.Should().Be(SortDirection.Ascending);
            comp.FindAll("th[role='columnheader']")[0].GetAttribute("aria-sort").Should().Be("ascending");

            var rowsAsc = comp.FindAll("tbody tr[role='row']");
            rowsAsc[0].TextContent.Should().Contain("Analytics");
            rowsAsc[1].TextContent.Should().Contain("Platform");
            rowsAsc[2].TextContent.Should().Contain("Auth Service");
            rowsAsc[3].TextContent.Should().Contain("Gateway");

            // Click again -> Descending: Platform first, Gateway before Auth Service
            await comp.FindAll("button.vek-treelist-sort-btn")[0].ClickAsync();
            comp.FindAll("th[role='columnheader']")[0].GetAttribute("aria-sort").Should().Be("descending");

            var rowsDesc = comp.FindAll("tbody tr[role='row']");
            rowsDesc[0].TextContent.Should().Contain("Platform");
            rowsDesc[1].TextContent.Should().Contain("Gateway");
            rowsDesc[2].TextContent.Should().Contain("Auth Service");
            rowsDesc[3].TextContent.Should().Contain("Analytics");

            // Click third time -> None (restores original order: Platform, Auth Service, Gateway, Analytics)
            await comp.FindAll("button.vek-treelist-sort-btn")[0].ClickAsync();
            comp.FindAll("th[role='columnheader']")[0].GetAttribute("aria-sort").Should().Be("none");

            var rowsReset = comp.FindAll("tbody tr[role='row']");
            rowsReset[0].TextContent.Should().Contain("Platform");
            rowsReset[1].TextContent.Should().Contain("Auth Service");
            rowsReset[2].TextContent.Should().Contain("Gateway");
            rowsReset[3].TextContent.Should().Contain("Analytics");
        }

        [Test]
        public async Task MultiColumnSort_Shows_SortIndex_And_Clears_With_ClearSortAsync()
        {
            var comp = RenderSampleTreeList(configure: p => p.Add(x => x.SortMode, SortMode.Multiple));

            await comp.FindAll("button.vek-treelist-sort-btn")[1].ClickAsync();
            await comp.FindAll("button.vek-treelist-sort-btn")[2].ClickAsync(new MouseEventArgs { CtrlKey = true });

            var indices = comp.FindAll(".vek-treelist-sort-index");
            indices.Count.Should().Be(2);
            indices[0].TextContent.Should().Be("1");
            indices[1].TextContent.Should().Be("2");

            await comp.InvokeAsync(() => comp.Instance.ClearSortAsync());
            comp.FindAll(".vek-treelist-sort-index").Should().BeEmpty();
        }

        [Test]
        public async Task ColumnResize_ContainerMode_Updates_ColWidth_And_Respects_MinWidth()
        {
            TreeListColumnResizeEventArgs<ProjectNode>? resizeArgs = null;
            var comp = RenderSampleTreeList(configure: p => p
                .Add(x => x.ColumnResizeMode, ResizeMode.Container)
                .Add(x => x.OnColumnResized, EventCallback.Factory.Create<TreeListColumnResizeEventArgs<ProjectNode>>(this, e => resizeArgs = e)));

            var firstResizer = comp.FindAll(".vek-treelist-resizer")[0];
            await firstResizer.PointerDownAsync(new PointerEventArgs { ClientX = 240, PointerId = 1, Detail = 1 });
            await firstResizer.PointerMoveAsync(new PointerEventArgs { ClientX = 320, PointerId = 1 });
            await firstResizer.PointerUpAsync(new PointerEventArgs { ClientX = 320, PointerId = 1 });

            resizeArgs.Should().NotBeNull();
            resizeArgs!.OldWidth.Should().Be(240);
            resizeArgs.NewWidth.Should().Be(320);
            comp.FindAll("colgroup col")[0].GetAttribute("style").Should().Contain("320px");

            // Shrinking below MinWidth (60) clamps to 60
            firstResizer = comp.FindAll(".vek-treelist-resizer")[0];
            await firstResizer.PointerDownAsync(new PointerEventArgs { ClientX = 320, PointerId = 1, Detail = 1 });
            await firstResizer.PointerUpAsync(new PointerEventArgs { ClientX = 20, PointerId = 1 });

            resizeArgs.NewWidth.Should().Be(60);
            comp.FindAll("colgroup col")[0].GetAttribute("style").Should().Contain("60px");

            // Double click resets runtime width back to configured Width (240px)
            firstResizer = comp.FindAll(".vek-treelist-resizer")[0];
            await firstResizer.DoubleClickAsync();
            resizeArgs.NewWidth.Should().BeNull();
            comp.FindAll("colgroup col")[0].GetAttribute("style").Should().Contain("240px");
        }

        [Test]
        public async Task ColumnResize_ColumnMode_Adjusts_Adjacent_Column()
        {
            var comp = RenderSampleTreeList(configure: p => p.Add(x => x.ColumnResizeMode, ResizeMode.Column));

            var firstResizer = comp.FindAll(".vek-treelist-resizer")[0];
            await firstResizer.PointerDownAsync(new PointerEventArgs { ClientX = 240, PointerId = 1, Detail = 1 });
            await firstResizer.PointerUpAsync(new PointerEventArgs { ClientX = 280, PointerId = 1 });

            var cols = comp.FindAll("colgroup col");
            cols[0].GetAttribute("style").Should().Contain("280px");
            cols[1].GetAttribute("style").Should().Contain("100px");
        }

        [Test]
        public async Task ColumnResize_Keyboard_Adjusts_And_Resets_Width()
        {
            var comp = RenderSampleTreeList();

            var resizer = comp.FindAll(".vek-treelist-resizer")[0];
            await resizer.KeyDownAsync(new KeyboardEventArgs { Key = "ArrowRight" });
            comp.FindAll("colgroup col")[0].GetAttribute("style").Should().Contain("256px");

            resizer = comp.FindAll(".vek-treelist-resizer")[0];
            await resizer.KeyDownAsync(new KeyboardEventArgs { Key = "ArrowLeft" });
            comp.FindAll("colgroup col")[0].GetAttribute("style").Should().Contain("240px");

            resizer = comp.FindAll(".vek-treelist-resizer")[0];
            await resizer.KeyDownAsync(new KeyboardEventArgs { Key = "ArrowRight" });
            resizer = comp.FindAll(".vek-treelist-resizer")[0];
            await resizer.KeyDownAsync(new KeyboardEventArgs { Key = "Home" });
            comp.FindAll("colgroup col")[0].GetAttribute("style").Should().Contain("240px");
        }

        [Test]
        public async Task Selection_Single_Toggle_And_MultiSelection_TriState_Work()
        {
            // Single selection -> Toggle selection
            ProjectNode? selected = null;
            var singleComp = RenderSampleTreeList(configure: p => p
                .Add(x => x.SelectionMode, SelectionMode.ToggleSelection)
                .Add(x => x.SelectedValueChanged, EventCallback.Factory.Create<ProjectNode?>(this, v => selected = v)));

            await singleComp.FindAll("tbody tr[role='row']")[1].ClickAsync();
            selected!.Name.Should().Be("Auth Service");
            singleComp.FindAll("tbody tr[role='row']")[1].GetAttribute("aria-selected").Should().Be("true");

            await singleComp.FindAll("tbody tr[role='row']")[1].ClickAsync();
            selected.Should().BeNull();
            singleComp.FindAll("tbody tr[role='row']")[1].GetAttribute("aria-selected").Should().Be("false");

            // MultiSelection with TriState
            IReadOnlyCollection<ProjectNode>? selectedValues = null;
            var multiComp = RenderSampleTreeList(configure: p => p
                .Add(x => x.SelectionMode, SelectionMode.MultiSelection)
                .Add(x => x.SelectedValuesChanged, EventCallback.Factory.Create<IReadOnlyCollection<ProjectNode>?>(this, v => selectedValues = v)));

            // Check Auth Service (child of Platform) -> Platform becomes indeterminate ("mixed")
            var checkboxes = multiComp.FindAll("button[role='checkbox']");
            await checkboxes[1].ClickAsync();

            checkboxes = multiComp.FindAll("button[role='checkbox']");
            checkboxes[0].GetAttribute("aria-checked").Should().Be("mixed");
            checkboxes[1].GetAttribute("aria-checked").Should().Be("true");

            // Check Gateway -> Platform becomes checked ("true") via AutoSelectParent
            await checkboxes[2].ClickAsync();
            checkboxes = multiComp.FindAll("button[role='checkbox']");
            checkboxes[0].GetAttribute("aria-checked").Should().Be("true");
            selectedValues.Should().HaveCount(3);
        }

        [Test]
        public async Task Disabled_And_ReadOnly_States_Enforce_Constraints()
        {
            ProjectNode? selected = null;
            var readOnlyComp = RenderSampleTreeList(configure: p => p
                .Add(x => x.ReadOnly, true)
                .Add(x => x.SelectedValueChanged, EventCallback.Factory.Create<ProjectNode?>(this, v => selected = v)));

            // ReadOnly blocks selection but allows expand
            await readOnlyComp.FindAll("tbody tr[role='row']")[3].ClickAsync();
            selected.Should().BeNull();
            await readOnlyComp.FindAll("tbody tr[role='row']")[3].QuerySelector("button.vek-treelist-toggle-btn")!.ClickAsync();
            readOnlyComp.FindAll("tbody tr[role='row']").Count.Should().Be(5);

            // Disabled blocks expand, sort, and marks disabled
            var disabledComp = RenderSampleTreeList(configure: p => p.Add(x => x.Disabled, true));
            disabledComp.Find(".vek-treelist").ClassList.Should().Contain("vek-treelist-disabled");
            disabledComp.FindAll("tbody tr[role='row']")[0].GetAttribute("aria-disabled").Should().Be("true");
            disabledComp.Find("button.vek-treelist-sort-btn").HasAttribute("disabled").Should().BeTrue();
        }

        [Test]
        public async Task Aria_TreeGrid_And_Keyboard_Navigation_Follow_APG()
        {
            ProjectNode? selected = null;
            var comp = RenderSampleTreeList(configure: p => p
                .Add(x => x.SelectionMode, SelectionMode.SingleSelection)
                .Add(x => x.SelectedValueChanged, EventCallback.Factory.Create<ProjectNode?>(this, v => selected = v)));

            var table = comp.Find("table[role='treegrid']");
            table.GetAttribute("aria-label").Should().Be("Tree list");

            var rows = comp.FindAll("tbody tr[role='row']");
            rows[0].GetAttribute("aria-level").Should().Be("1");
            rows[0].GetAttribute("aria-posinset").Should().Be("1");
            rows[0].GetAttribute("aria-setsize").Should().Be("2");
            rows[0].GetAttribute("aria-expanded").Should().Be("true");
            rows[0].GetAttribute("tabindex").Should().Be("0");

            rows[1].GetAttribute("aria-level").Should().Be("2");
            rows[1].GetAttribute("aria-posinset").Should().Be("1");
            rows[1].GetAttribute("aria-setsize").Should().Be("2");
            rows[1].HasAttribute("aria-expanded").Should().BeFalse();

            // ArrowLeft collapses expanded parent row
            await rows[0].KeyDownAsync(new KeyboardEventArgs { Key = "ArrowLeft" });
            comp.FindAll("tbody tr[role='row']").Count.Should().Be(2);

            // ArrowRight expands it back
            await comp.FindAll("tbody tr[role='row']")[0].KeyDownAsync(new KeyboardEventArgs { Key = "ArrowRight" });
            comp.FindAll("tbody tr[role='row']").Count.Should().Be(4);

            // ArrowDown moves roving tabindex to second row, Enter selects it
            await comp.FindAll("tbody tr[role='row']")[0].KeyDownAsync(new KeyboardEventArgs { Key = "ArrowDown" });
            rows = comp.FindAll("tbody tr[role='row']");
            rows[1].GetAttribute("tabindex").Should().Be("0");

            await rows[1].KeyDownAsync(new KeyboardEventArgs { Key = "Enter" });
            selected!.Name.Should().Be("Auth Service");
        }
    }
}
