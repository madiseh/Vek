// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Components;

namespace Vek;

/// <summary>
/// Builds an ordered set of row rules that selects a display template for a data grid cell.
/// </summary>
/// <typeparam name="T">The type of row item evaluated by the rules.</typeparam>
public sealed class DataGridCellTemplateSelector<T>
{
    private readonly List<Rule> _rules = [];
    private RenderFragment<T>? _otherwise;

    /// <summary>
    /// Adds a template to use when the predicate matches a row item.
    /// </summary>
    /// <param name="predicate">The condition that selects the template.</param>
    /// <param name="template">The template to render for matching items.</param>
    /// <returns>This selector so another rule can be added.</returns>
    public DataGridCellTemplateSelector<T> When(Func<T, bool> predicate, RenderFragment<T> template)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(template);

        _rules.Add(new Rule(predicate, template));
        return this;
    }

    /// <summary>
    /// Sets the template to use when no rule matches a row item.
    /// </summary>
    /// <param name="template">The fallback template.</param>
    /// <returns>This selector so it can be built or extended.</returns>
    public DataGridCellTemplateSelector<T> Otherwise(RenderFragment<T> template)
    {
        ArgumentNullException.ThrowIfNull(template);

        _otherwise = template;
        return this;
    }

    /// <summary>
    /// Creates a row-item selector from the rules currently registered.
    /// </summary>
    /// <remarks>
    /// Rules are evaluated in insertion order. The returned selector is a snapshot; rules added later are not included.
    /// If no rule matches and no fallback was set, it returns <c>null</c> so the cell uses its standard value display.
    /// </remarks>
    /// <returns>A selector suitable for <see cref="Column{T}.CellDisplayTemplateSelector"/>.</returns>
    public Func<T, RenderFragment<T>?> Build()
    {
        var rules = _rules.ToArray();
        var otherwise = _otherwise;

        return item =>
        {
            foreach (var rule in rules)
            {
                if (rule.Predicate(item))
                {
                    return rule.Template;
                }
            }

            return otherwise;
        };
    }

    private readonly record struct Rule(Func<T, bool> Predicate, RenderFragment<T> Template);
}
