<!--
Copyright (c) 2026 AMIN SABAGHI
Licensed under the MIT License. See the LICENSE file in the project root for more information.
Created: 2026-11-02
-->

# Vek.WPF.DataGrid

A native WPF data grid for .NET 10 with shared sorting, filtering, paging, selection, editing, grouping, and aggregation behavior.

## Requirements

- .NET 10 SDK
- A Windows application targeting `net10.0-windows` with WPF enabled

## Install

```sh
dotnet add package Vek.WPF.DataGrid
```

## Basic usage

Reference the control's namespace in XAML and bind it like a WPF `DataGrid`:

```xml
<Window
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:vek="clr-namespace:Vek.WPF.DataGrid.Components;assembly=Vek.WPF.DataGrid">
    <vek:VekDataGrid ItemsSource="{Binding Products}" AutoGenerateColumns="False">
        <DataGrid.Columns>
            <DataGridTextColumn Header="Name" Binding="{Binding Name}" />
            <DataGridTextColumn Header="Price" Binding="{Binding Price}" />
        </DataGrid.Columns>
    </vek:VekDataGrid>
</Window>
```

The control inherits from WPF `DataGrid`; standard bindings and columns remain available. See the [repository](https://github.com/madiseh/Vek) for examples and feature details.
