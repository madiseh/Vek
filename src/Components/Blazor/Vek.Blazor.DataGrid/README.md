<!--
Copyright (c) 2026 AMIN SABAGHI
Licensed under the MIT License. See the LICENSE file in the project root for more information.
Created: 2026-11-02
-->

# Vek.Blazor.DataGrid

A Blazor data grid for .NET 10 with sorting, filtering, paging, selection, editing, grouping, aggregation, and server data support.

## Install

```sh
dotnet add package Vek.Blazor.DataGrid
```

## Basic usage

Import the component namespace and provide your data and columns:

```razor
@using Vek.Components.Components.DataGrid

<VekDataGrid TItem="Product" Items="@products" PageSize="10">
    <PropertyColumn Property="product => product.Name" Title="Name" />
    <PropertyColumn Property="product => product.Price" Title="Price" Format="C2" />
</VekDataGrid>
```

The grid does not load data itself. Pass items through `Items`, or configure `ServerData` for asynchronous server-side requests.

## Styles

Add the package stylesheet to the host page or app component:

```html
<link rel="stylesheet" href="_content/Vek.Blazor.DataGrid/vek-components.css" />
```

See the [DataGrid documentation](https://github.com/madiseh/Vek/tree/master/docs/Components/DataGrid.md) for feature details.
