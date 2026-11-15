// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Components;
using Vek.Utilities;

namespace Vek;

/// <summary>
/// The content within a <see cref="VekPicker{T}"/>.
/// </summary>
/// <seealso cref="VekPicker{T}" />
/// <seealso cref="VekPickerToolbar" />
public partial class VekPickerContent : VekComponentBase
{
    protected string Classname =>
        new CssBuilder("vek-picker-content")
            .AddClass(Class)
            .Build();

    /// <summary>
    /// The content to display.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.Picker.Behavior)]
    public RenderFragment? ChildContent { get; set; }
}
