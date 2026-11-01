using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using Vek.Core.DataGrid;
using Vek.WPF.DataGrid.Components;

namespace Vek.WPF.Demo.Tests;

public sealed class VekWpfDataGridAdapterTests
{
    /// <summary>Native row-edit completion and cancellation also finish the authoritative shared edit session.</summary>
    [Theory]
    [InlineData(DataGridEditAction.Commit)]
    [InlineData(DataGridEditAction.Cancel)]
    public async Task NativeEditEndingFinishesSharedSession(DataGridEditAction action)
    {
        await await WpfTestDispatcher.RunAsync(async () =>
        {
            var item = new TestRow("one");
            var grid = new TestableVekDataGrid { ItemsSource = new[] { item } };
            grid.AddColumn(new VekDataGridColumn<TestRow, DataTemplate>("Key", row => row.Key,
                editable: true, editTemplate: _ => new DataTemplate()));
            await grid.EditItemAsync(item);
            Assert.True(grid.Engine.IsEditing);
            grid.TriggerRowEditEnding(new DataGridRowEditEndingEventArgs(new DataGridRow { Item = item }, action));
            Assert.False(grid.Engine.IsEditing);
        });
    }

    /// <summary>Sorting an ordinary native text column does not make its editor enter the shared-template editing path.</summary>
    [Fact]
    public async Task NativeTextEditingAfterSortingDoesNotEnterSharedEditing()
    {
        await await WpfTestDispatcher.RunAsync(async () =>
        {
            var item = new TestRow("one");
            var grid = new TestableVekDataGrid { ItemsSource = new[] { item } };
            var column = new DataGridTextColumn { SortMemberPath = "Key", Header = "Key" };
            grid.Columns.Add(column);
            grid.TriggerSorting(new DataGridSortingEventArgs(column));
            Exception? failure = null;
            System.Windows.Threading.DispatcherUnhandledExceptionEventHandler handler = (_, args) =>
            {
                failure = args.Exception;
                args.Handled = true;
            };
            grid.Dispatcher.UnhandledException += handler;
            try
            {
                grid.TriggerBeginningEdit(new DataGridBeginningEditEventArgs(column, new DataGridRow { Item = item }, null));
                await grid.Dispatcher.InvokeAsync(() => { }, System.Windows.Threading.DispatcherPriority.Background);
                Assert.Null(failure);
                Assert.False(grid.Engine.IsEditing);
            }
            finally { grid.Dispatcher.UnhandledException -= handler; }
        });
    }

    /// <summary>Native selection rejects rows disallowed by the same shared eligibility policy as selection commands.</summary>
    [Theory]
    [InlineData(DataGridSelectionMode.Single)]
    [InlineData(DataGridSelectionMode.Extended)]
    public async Task NativeSelectionHonorsEligibility(DataGridSelectionMode mode)
    {
        await WpfTestDispatcher.RunAsync(() =>
        {
            var item = new TestRow("disabled");
            var allowed = new TestRow("allowed");
            var grid = new VekDataGrid { ItemsSource = new[] { item, allowed }, IsRowSelectable = row => ((TestRow)row).Key != "disabled", SelectionMode = mode };
            grid.SelectedItem = allowed;
            grid.SelectedItem = item;
            Assert.Same(allowed, grid.SelectedItem);
            Assert.Equal(grid.Engine.GetItemKey(allowed), Assert.Single(grid.Engine.SelectedKeys));
            return true;
        });
    }

    /// <summary>Disabling row activation prevents mouse-driven selection while retaining programmatic selection.</summary>
    [Fact]
    public async Task NativeRowActivationHonorsRowClickPolicy()
    {
        await await WpfTestDispatcher.RunAsync(async () =>
        {
            var item = new TestRow("one");
            var grid = new TestableVekDataGrid { ItemsSource = new[] { item }, RowClickSelection = false };
            var args = new System.Windows.Input.MouseButtonEventArgs(System.Windows.Input.Mouse.PrimaryDevice, 0, System.Windows.Input.MouseButton.Left)
            {
                RoutedEvent = UIElement.PreviewMouseLeftButtonDownEvent,
                Source = new DataGridRow { Item = item }
            };
            grid.TriggerPreviewMouseLeftButtonDown(args);
            grid.SelectedItem = item;
            Assert.Null(grid.SelectedItem);
            await grid.Dispatcher.InvokeAsync(() => { }, System.Windows.Threading.DispatcherPriority.Background);
            grid.SelectedItem = item;
            Assert.Same(item, grid.SelectedItem);
        });
    }

    /// <summary>Shared paging commands slice the original source and clamp requests to the final native page.</summary>
    [Fact]
    public async Task SharedPagingCommandsSliceTheNativeSource()
    {
        await await WpfTestDispatcher.RunAsync(async () =>
        {
            var rows = new[] { new TestRow("one"), new TestRow("two"), new TestRow("three") };
            var grid = new VekDataGrid { ItemsSource = rows };
            await grid.SetPageSizeAsync(1);
            Assert.Same(rows[0], Assert.Single(grid.Items.Cast<TestRow>()));
            await grid.SetCurrentPageAsync(100);
            Assert.Equal(3, grid.CurrentPage);
            Assert.Same(rows[2], Assert.Single(grid.Items.Cast<TestRow>()));
        });
    }

    /// <summary>Native editing retains rejected shared drafts and publishes accepted drafts without mutating the original row.</summary>
    [Fact]
    public async Task SharedEditRetainsRejectedDraftAndPublishesCommittedValue()
    {
        await await WpfTestDispatcher.RunAsync(async () =>
        {
            var original = new TestRow("original");
            var accepted = false;
            object? committed = null;
            object? cancelled = null;
            var grid = new VekDataGrid { ItemsSource = new[] { original }, CommitEditHandler = _ => Task.FromResult(accepted) };
            grid.ItemCommitted += item => committed = item;
            grid.EditCancelled += item => cancelled = item;
            grid.AddColumn(new VekDataGridColumn<TestRow, DataTemplate>("Key", row => row.Key,
                editable: true, editTemplate: _ => new DataTemplate()));
            await grid.EditItemAsync(original);
            Assert.NotSame(original, grid.Engine.DraftEditItem);
            var draft = new TestRow("edited");
            grid.UpdateEditDraft(draft);
            Assert.False(await grid.CommitEditAsync());
            Assert.True(grid.Engine.IsEditing);
            accepted = true;
            Assert.True(await grid.CommitEditAsync());
            Assert.Same(draft, committed);
            Assert.Equal("original", original.Key);
            Assert.False(grid.Engine.IsEditing);
            await grid.EditItemAsync(original);
            await grid.CancelEditAsync();
            Assert.Same(original, cancelled);
            Assert.False(grid.Engine.IsEditing);
        });
    }

    /// <summary>Shared column commands update native visibility, width, and display order.</summary>
    [Fact]
    public async Task SharedColumnCommandsUpdateNativeColumns()
    {
        await await WpfTestDispatcher.RunAsync(async () =>
        {
            var grid = new VekDataGrid();
            var first = grid.AddColumn(new VekDataGridColumn<TestRow, DataTemplate>("Key", row => row.Key));
            grid.AddColumn(new VekDataGridColumn<TestRow, DataTemplate>("Other", row => row.Key));
            await grid.ResizeColumnAsync("Key", 123);
            await grid.SetColumnVisibilityAsync("Key", false);
            await grid.ReorderColumnAsync("Key", 1);
            Assert.Equal(123, first.Width.Value);
            Assert.Equal(Visibility.Collapsed, first.Visibility);
            Assert.Equal(1, first.DisplayIndex);
        });
    }

    /// <summary>Shared selection updates the native selection collection and selected-key state together.</summary>
    [Fact]
    public async Task SharedSelectionCommandsUpdateNativeSelection()
    {
        await await WpfTestDispatcher.RunAsync(async () =>
        {
            var rows = new[] { new TestRow("one"), new TestRow("two") };
            var grid = new VekDataGrid { ItemsSource = rows };
            await grid.SelectItemAsync(rows[0]);
            await grid.SelectItemAsync(rows[1]);
            Assert.Equal(rows, grid.SelectedItems.Cast<TestRow>().ToArray());
            Assert.Equal(2, grid.Engine.SelectedKeys.Count);
            await grid.SelectAllAsync(false);
            Assert.Empty(grid.SelectedItems.Cast<object>());
            Assert.Empty(grid.Engine.SelectedKeys);
        });
    }

    /// <summary>Shared detail expansion is supplied to the native detail template through its canonical context.</summary>
    [Fact]
    public async Task SharedExpansionUpdatesNativeDetailContext()
    {
        await await WpfTestDispatcher.RunAsync(async () =>
        {
            var row = new TestRow("one");
            var template = new DataTemplate();
            VekDataGridRowDetailContext<TestRow>? context = null;
            var grid = new VekDataGrid { ItemsSource = new[] { row } };
            grid.AddColumn(new VekDataGridColumn<TestRow, DataTemplate>("details", item => item.Key,
                isHierarchyColumn: true, detailTemplate: value => { context = value; return template; }));
            await grid.ToggleRowDetailsAsync(row);
            Assert.Same(template, grid.RowDetailsTemplateSelector!.SelectTemplate(row, grid));
            Assert.True(context!.Expanded);
            await grid.CollapseAllAsync();
            Assert.False(grid.IsRowExpanded(row));
        });
    }

    /// <summary>Shared sorting resets the page and refreshes the native rows and direction indicator.</summary>
    [Fact]
    public async Task SharedSortResetsPageAndRefreshesNativeProjection()
    {
        await await WpfTestDispatcher.RunAsync(async () =>
        {
            var rows = new[] { new TestRow("row-3"), new TestRow("row-1"), new TestRow("row-2") };
            var grid = new VekDataGrid { ItemsSource = rows, PageSize = 1, CurrentPage = 2 };
            var column = grid.AddColumn(new VekDataGridColumn<TestRow, DataTemplate>("Key", item => item.Key));

            await grid.SortAsync("Key", VekDataGridSortMode.Single);

            Assert.Equal(1, grid.CurrentPage);
            Assert.Same(rows[1], Assert.Single(grid.Items.Cast<TestRow>()));
            Assert.Equal(ListSortDirection.Ascending, column.SortDirection);

            await grid.SortAsync("Key", VekDataGridSortMode.Single);

            Assert.Same(rows[0], Assert.Single(grid.Items.Cast<TestRow>()));
            Assert.Equal(ListSortDirection.Descending, column.SortDirection);
        });
    }

    /// <summary>Clearing shared sorts restores native rows to source order and removes the native sort indicator.</summary>
    [Fact]
    public async Task ClearSortRestoresNativeRowsAndSortIndicator()
    {
        await await WpfTestDispatcher.RunAsync(async () =>
        {
            var rows = new[] { new TestRow("row-2"), new TestRow("row-1") };
            var grid = new TestableVekDataGrid { ItemsSource = rows };
            var column = grid.AddColumn(new VekDataGridColumn<TestRow, DataTemplate>("Key", item => item.Key));
            grid.TriggerSorting(new DataGridSortingEventArgs(column));
            Assert.Equal(new[] { rows[1], rows[0] }, grid.Items.Cast<TestRow>().ToArray());
            Assert.Equal(VekSortDirection.Ascending, Assert.Single(grid.Engine.Sorts).Direction);
            column.SortDirection = ListSortDirection.Ascending;

            await grid.ClearSortAsync();

            Assert.Empty(grid.Engine.Sorts);
            Assert.Equal(rows, grid.Items.Cast<TestRow>().ToArray());
            Assert.Null(column.SortDirection);
        });
    }

    /// <summary>Ensures WPF test windows are laid out without becoming visible or stealing focus.</summary>
    [Fact]
    public async Task TestWindowsAreShownOffscreenWithoutActivation()
    {
        await WpfTestDispatcher.RunAsync(() =>
        {
            var window = new Window { Width = 200, Height = 100 };
            try
            {
                WpfTestDispatcher.ShowTestWindow(window);

                Assert.True(window.IsVisible);
                Assert.Equal(0, window.Opacity);
                Assert.False(window.ShowActivated);
                Assert.False(window.ShowInTaskbar);
                Assert.True(window.Left + window.Width < SystemParameters.VirtualScreenLeft
                    || window.Top + window.Height < SystemParameters.VirtualScreenTop);
            }
            finally
            {
                window.Close();
            }

            return true;
        });
    }

    /// <summary>
    /// Ensures that each WPF DataGrid instance creates its own isolated Core engine instance.
    /// </summary>
    [Fact]
    public async Task CreatesIsolatedCoreEnginePerGridInstance()
    {
        await WpfTestDispatcher.RunAsync(() =>
        {
            var grid1 = new VekDataGrid();
            var grid2 = new VekDataGrid();

            Assert.NotNull(grid1.Engine);
            Assert.NotNull(grid2.Engine);
            Assert.NotSame(grid1.Engine, grid2.Engine);
            return true;
        });
    }

    /// <summary>
    /// Synchronizes native sort changes directly to the underlying Core engine.
    /// </summary>
    [Fact]
    public async Task SynchronizesNativeSortingToCoreEngine()
    {
        await WpfTestDispatcher.RunAsync(() =>
        {
            var grid = new TestableVekDataGrid();
            var column = new DataGridTextColumn
            {
                Header = "Name",
                SortMemberPath = "Name",
                SortDirection = ListSortDirection.Ascending
            };

            grid.TriggerSorting(new DataGridSortingEventArgs(column));

            Assert.NotNull(grid.Engine);
            var sort = Assert.Single(grid.Engine.Sorts);
            Assert.Equal("Name", sort.ColumnKey);
            Assert.Equal(VekSortDirection.Ascending, sort.Direction);

            grid.TriggerSorting(new DataGridSortingEventArgs(column));
            Assert.Equal(VekSortDirection.Descending, Assert.Single(grid.Engine.Sorts).Direction);
            return true;
        });
    }

    /// <summary>
    /// Copies native WPF selection into the shared Core selected-key state.
    /// </summary>
    [Fact]
    public async Task SynchronizesNativeSelectionToCoreEngine()
    {
        await WpfTestDispatcher.RunAsync(() =>
        {
            var item = new TestRow("row-1");
            var grid = new VekDataGrid { ItemsSource = new[] { item } };
            grid.SelectedItem = item;

            var key = item.GetHashCode().ToString(System.Globalization.CultureInfo.InvariantCulture);
            Assert.Contains(key, grid.Engine.SelectedKeys);
            return true;
        });
    }

    /// <summary>Verifies a configured item key keeps Core selection across refreshed WPF row instances.</summary>
    [Fact]
    public async Task ItemKeySelectorPreservesSelectionAcrossEquivalentRows()
    {
        await WpfTestDispatcher.RunAsync(() =>
        {
            var grid = new VekDataGrid
            {
                ItemsSource = new[] { new TestRow("row-1") },
                ItemKeySelector = item => ((TestRow)item).Key
            };
            grid.SelectedItem = new TestRow("row-1");

            Assert.True(grid.Engine.IsSelected(new TestRow("row-1")));
            return true;
        });
    }

    /// <summary>
    /// Applies shared Core filtering to the WPF grid's displayed rows and restores the full source when cleared.
    /// </summary>
    [Fact]
    public async Task AppliesAndClearsCoreFiltersForDisplayedRows()
    {
        await await WpfTestDispatcher.RunAsync(async () =>
        {
            var rows = new[] { new TestRow("row-1"), new TestRow("row-2") };
            var grid = new VekDataGrid { ItemsSource = rows };
            grid.AddColumn(new VekDataGridColumn<TestRow, DataTemplate>("Key", item => item.Key));

            await grid.ApplyFilterAsync("Key", VekDataGridFilterOperator.Equals, "row-2");

            Assert.Equal(new[] { rows[1] }, grid.Items.Cast<TestRow>().ToArray());

            await grid.RemoveFilterAsync();

            Assert.Equal(rows, grid.Items.Cast<TestRow>().ToArray());
        });
    }

    /// <summary>
    /// Registers the shared generic column contract as a native WPF template column and preserves its template slots.
    /// </summary>
    [Fact]
    public async Task RegistersSharedColumnContractAsNativeTemplateColumn()
    {
        await WpfTestDispatcher.RunAsync(() =>
        {
            var cellTemplate = new DataTemplate();
            var filterTemplate = new DataTemplate();
            var groupTemplate = new DataTemplate();
            var editTemplate = new DataTemplate();
            var detailTemplate = new DataTemplate();
            var column = new VekDataGridColumn<TestRow, DataTemplate>(
                "Key",
                item => item.Key,
                title: "Identifier",
                cellTemplate: _ => cellTemplate,
                filterTemplate: _ => filterTemplate,
                groupTemplate: _ => groupTemplate,
                editTemplate: _ => editTemplate,
                detailTemplate: _ => detailTemplate,
                editable: true);
            var grid = new TestableVekDataGrid();

            var nativeColumn = grid.AddColumn(column);

            Assert.IsType<DataGridTemplateColumn>(nativeColumn);
            Assert.Same(column, grid.GetColumn<TestRow>("Key"));
            Assert.Equal("Identifier", nativeColumn.Header);
            Assert.Equal("Key", nativeColumn.SortMemberPath);
            Assert.False(nativeColumn.IsReadOnly);
            Assert.Same(cellTemplate, nativeColumn.CellTemplateSelector!.SelectTemplate(new TestRow("row-1"), grid));
            Assert.Same(editTemplate, nativeColumn.CellEditingTemplateSelector!.SelectTemplate(new TestRow("row-1"), grid));

            var coreColumn = Assert.Single(grid.Engine.Columns);
            Assert.Same(cellTemplate, coreColumn.CellTemplate!(new TestRow("row-1")));
            Assert.Same(filterTemplate, coreColumn.FilterTemplate!(new VekDataGridFilterContext(null, _ => { }, () => { })));
            Assert.Same(groupTemplate, coreColumn.GroupTemplate!(new VekDataGridGroupContext<object>(
                "Key", "Identifier", "row-1", 0, [new TestRow("row-1")], true, () => Task.CompletedTask, [])));
            Assert.Same(editTemplate, coreColumn.EditTemplate!(new VekDataGridEditTemplateContext<object>(
                new TestRow("row-1"), System.Globalization.CultureInfo.InvariantCulture, _ => { }, () => Task.FromResult(true), () => Task.CompletedTask)));
            Assert.Same(detailTemplate, coreColumn.DetailTemplate!(new VekDataGridRowDetailContext<object>(
                new TestRow("row-1"), false, () => Task.CompletedTask)));

            grid.TriggerSorting(new DataGridSortingEventArgs(nativeColumn));

            Assert.NotNull(Assert.Single(grid.Engine.Columns).CellTemplate);
            return true;
        });
    }

    /// <summary>
    /// Keeps the first sort key as the primary order when the WPF adapter is configured for multiple sorting.
    /// </summary>
    [Fact]
    public async Task MultipleSortModeAppliesOrderedCoreSortsToDisplayedRows()
    {
        await WpfTestDispatcher.RunAsync(() =>
        {
            var rows = new[]
            {
                new TestSortRow("A", 2),
                new TestSortRow("B", 2),
                new TestSortRow("A", 1)
            };
            var grid = new TestableVekDataGrid
            {
                ItemsSource = rows,
                SortMode = VekDataGridSortMode.Multiple
            };
            var nameColumn = new DataGridTextColumn { Header = "Name", SortMemberPath = "Name" };
            var valueColumn = new DataGridTextColumn { Header = "Value", SortMemberPath = "Value" };

            grid.TriggerSorting(new DataGridSortingEventArgs(nameColumn));
            grid.TriggerSorting(new DataGridSortingEventArgs(valueColumn));

            Assert.Equal(new[] { "Name", "Value" }, grid.Engine.Sorts.OrderBy(sort => sort.Priority).Select(sort => sort.ColumnKey));
            Assert.Equal(
                new[] { "A:1", "A:2", "B:2" },
                grid.Items.Cast<TestSortRow>().Select(item => $"{item.Name}:{item.Value}"));
            return true;
        });
    }

    /// <summary>
    /// Shows the requested page from the original local source when WPF page state changes.
    /// </summary>
    [Fact]
    public async Task PageSizeAndCurrentPageSliceDisplayedRows()
    {
        await WpfTestDispatcher.RunAsync(() =>
        {
            var rows = new[] { new TestRow("row-1"), new TestRow("row-2"), new TestRow("row-3") };
            var grid = new VekDataGrid { ItemsSource = rows, PageSize = 1, CurrentPage = 2 };

            Assert.Equal(new[] { rows[1] }, grid.Items.Cast<TestRow>().ToArray());

            grid.CurrentPage = 3;

            Assert.Equal(new[] { rows[2] }, grid.Items.Cast<TestRow>().ToArray());
            return true;
        });
    }

    /// <summary>Verifies shared Core group definitions create native WPF collection-view groups.</summary>
    [Fact]
    public async Task GroupByCreatesNativeCollectionViewGroups()
    {
        await WpfTestDispatcher.RunAsync(() =>
        {
            var grid = new VekDataGrid
            {
                ItemsSource = new[]
                {
                    new TestGroupRow("Furniture", "Chair"),
                    new TestGroupRow("Furniture", "Desk"),
                    new TestGroupRow("Office", "Notebook")
                }
            };
            grid.AddColumn(new VekDataGridColumn<TestGroupRow, DataTemplate>("Category", item => item.Category));

            grid.GroupBy = new[] { new VekDataGridGroupDefinition("Category") };

            var view = Assert.IsAssignableFrom<CollectionView>(grid.ItemsSource);
            var groups = view.Groups!.Cast<CollectionViewGroup>().ToArray();
            Assert.Equal(new[] { "Furniture", "Office" }, groups.Select(group => group.Name));
            Assert.Equal(new[] { 2, 1 }, groups.Select(group => group.ItemCount));
            return true;
        });
    }

    private sealed record TestRow(string Key);

    private sealed record TestSortRow(string Name, int Value);

    private sealed record TestGroupRow(string Category, string Name);

    private sealed class TestableVekDataGrid : VekDataGrid
    {
        public void TriggerRowEditEnding(DataGridRowEditEndingEventArgs args) => OnRowEditEnding(args);
        public void TriggerBeginningEdit(DataGridBeginningEditEventArgs args) => OnBeginningEdit(args);
        public void TriggerPreviewMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs args) => OnPreviewMouseLeftButtonDown(args);
        public void TriggerSorting(DataGridSortingEventArgs args)
        {
            OnSorting(args);
        }
    }
}
