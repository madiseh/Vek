// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Components;
using Vek.Interop;

namespace Vek.Services;

/// <summary>
/// Delegate for handling size change events.
/// </summary>
/// <param name="changes">A dictionary containing the elements and their corresponding bounding client rectangles that have changed size.</param>
public delegate void SizeChanged(IDictionary<ElementReference, BoundingClientRect> changes);
