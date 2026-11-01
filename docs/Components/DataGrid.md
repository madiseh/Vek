# VekDataGrid

`VekDataGrid<TItem>` displays a collection of items in a sortable, paginated table. It is part of `Vek.Components` and does not load data itself.

## Basic usage

Pass the full item collection to `Items`, then declare a `PropertyColumn` for each property to display:

```razor
<VekDataGrid TItem="Product" Items="@products" PageSize="10">
    <PropertyColumn Property="product => product.Name" Title="Product name" />
    <PropertyColumn Property="product => product.Category" />
    <PropertyColumn Property="product => product.Price" Title="Price" />
</VekDataGrid>
```

`TItem` is the row model type. `Property` is a strongly typed expression, so an invalid property is caught at compile time. `Title` is optional; when omitted, the selected property's name is used.

## Shared behavior and adapters

`Vek.Core.DataGrid` is the platform-neutral source of DataGrid state and behavior. The Blazor and WPF controls each own an independent `VekDataGridEngine<TItem, TTemplate>` instance and adapt native UI input to the shared Core contract. Core owns the shared `IVekDataGridColumn<TItem, TTemplate>` contract and the common sorting, filtering, paging, grouping, aggregation, selection, edit state, server request descriptors, and state snapshots. Each adapter supplies its native template type (`RenderFragment` or `DataTemplate`); `VekDataGridTemplate<TContext, TTemplate>` preserves strongly typed contexts for cell, filter, group, edit, and detail templates. Concrete rendering, native event arguments, editor controls, and platform-specific visual features remain in each adapter.

Both adapters use the same Core names for common state: `Items`, `Page`, `PageSize`, `Sorts`, `Filters`, `Groups`, and `SelectedKeys`. For remote data, the host invokes its configured provider with Core's neutral state and cancellation token, then returns the data result to the grid. Core has no dependency on Blazor, WPF, or the data provider.

## Parameters

| Parameter | Description |
| --- | --- |
| `Items` | Full collection displayed by the grid. |
| `PageSize` | Number of rows per page. Defaults to `10`. |
| `Class` | Optional CSS class applied to the outer grid container. |
| `ChildContent` | The `PropertyColumn` declarations. |

## Sorting and pagination

Click a column heading to sort ascending; click it again to reverse direction. The configured sort mode controls single or multi-column sorting. Changing sort, filter, or page state updates the Core engine. Local data is filtered and sorted before Core applies page slicing. With a remote provider, the host loads the requested page from Core's neutral query state.

`PropertyColumn` supports custom value selectors and comparers for sorting, filtering, grouping, and aggregates. Templates and other visual options remain in the Blazor adapter.
