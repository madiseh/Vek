using System.ComponentModel;
using System.Windows.Controls;
using Vek.Core.DataGrid;
using Vek.WPF.DataGrid.Components;

namespace Vek.WPF.Demo.Tests;

public sealed class VekWpfDataGridAdapterTests
{
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

    private sealed record TestRow(string Key);

    private sealed class TestableVekDataGrid : VekDataGrid
    {
        public void TriggerSorting(DataGridSortingEventArgs args)
        {
            OnSorting(args);
        }
    }
}
