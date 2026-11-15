using System;
using System.Collections.Generic;
using Vek.Docs.Models;

namespace Vek.Docs.Services;

#nullable enable
public interface IMenuService
{
    IEnumerable<VekComponent> Components { get; }

    IEnumerable<VekComponent> Api { get; }

    VekComponent? GetParent(Type? type);

    VekComponent? GetComponent(Type? type);

    /// <summary>
    /// Gets a component by its type name.
    /// </summary>
    /// <param name="typeName">The name of the type, such as <c>VekAlert</c>.</param>
    /// <returns>The matching component, or <c>null</c> if none was found.</returns>
    string? GetComponentName(string typeName);

    IEnumerable<DocsLink> Features { get; }

    IEnumerable<DocsLink> Customization { get; }

    IEnumerable<DocsLink> Utilities { get; }

    /// <summary>
    /// Gets the menu for example for the specified type.
    /// </summary>
    /// <param name="type">The type to examine.</param>
    /// <returns>When <c>true</c>, the menu service has a record of this type.</returns>
    VekComponent? GetExample(DocumentedType type);
}
