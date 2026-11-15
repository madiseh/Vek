// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Vek.Utilities
{
    /// <summary>
    /// Event data for the <see cref="VekDataGrid{T}.HierarchyVisibilityToggled"/> event, carrying the toggled row item and whether it was expanded or collapsed.
    /// </summary>
    /// <typeparam name="T">The item managed by the <see cref="VekDataGrid{T}"/>.</typeparam>
    public class DataGridHierarchyVisibilityToggledEventArgs<T>
    {
        /// <summary>
        /// The item whose visibility was changed.
        /// </summary>
        public T Item { get; }
        /// <summary>
        /// If <c>true</c> item was expanded, otherwise collapsed
        /// </summary>
        public bool Expanded { get; }

        public DataGridHierarchyVisibilityToggledEventArgs(T item, bool expanded)
        {
            Item = item;
            Expanded = expanded;
        }
    }
}
