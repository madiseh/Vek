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

## Parameters

| Parameter | Description |
| --- | --- |
| `Items` | Full collection displayed by the grid. |
| `PageSize` | Number of rows per page. Defaults to `10`. |
| `Class` | Optional CSS class applied to the outer grid container. |
| `ChildContent` | The `PropertyColumn` declarations. |

## Sorting and pagination

Click a column heading to sort ascending; click it again to sort descending. Only one column is sorted at a time. Choosing another heading starts an ascending sort and returns to the first page. Sorting and paging are performed in memory over the `Items` collection. The pager provides previous/next buttons and a page indicator.

The initial `PropertyColumn` is intended for values supported by `Comparer<TValue>.Default`, such as strings, numbers, dates, and enums. Custom cell templates, server-side paging, filtering, and multi-column sorting are not included yet.
