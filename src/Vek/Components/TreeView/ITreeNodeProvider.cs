// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Vek;

/// <summary>
/// Provides an abstraction for on-demand asynchronous child node loading and hierarchy resolution in <see cref="VekTreeView{T}"/>.
/// </summary>
/// <typeparam name="T">The type of item or value represented by the tree node.</typeparam>
public interface ITreeNodeProvider<T>
{
    /// <summary>
    /// Determines whether the specified parent node has children to expand.
    /// </summary>
    /// <param name="parent">The parent item data.</param>
    /// <returns><c>true</c> if the parent node has or can load children; otherwise, <c>false</c>.</returns>
    bool HasChildren(TreeItemData<T> parent);

    /// <summary>
    /// Asynchronously fetches or loads child nodes for the specified parent node on demand.
    /// </summary>
    /// <param name="parent">The parent item data whose children should be loaded.</param>
    /// <param name="cancellationToken">An optional token to monitor for cancellation requests.</param>
    /// <returns>A task returning the loaded child nodes.</returns>
    Task<IEnumerable<TreeItemData<T>>> LoadChildrenAsync(TreeItemData<T> parent, CancellationToken cancellationToken = default);
}

/// <summary>
/// A delegate-based implementation of <see cref="ITreeNodeProvider{T}"/>.
/// </summary>
/// <typeparam name="T">The type of item or value represented by the tree node.</typeparam>
public class DelegateTreeNodeProvider<T> : ITreeNodeProvider<T>
{
    private readonly Func<TreeItemData<T>, CancellationToken, Task<IEnumerable<TreeItemData<T>>>> _loadChildrenAsync;
    private readonly Func<TreeItemData<T>, bool>? _hasChildren;

    /// <summary>
    /// Initializes a new instance of the <see cref="DelegateTreeNodeProvider{T}"/> class with a standard async delegate.
    /// </summary>
    /// <param name="loadChildrenAsync">The delegate used to load child nodes asynchronously.</param>
    /// <param name="hasChildren">An optional predicate determining whether a node has children.</param>
    public DelegateTreeNodeProvider(
        Func<TreeItemData<T>, Task<IEnumerable<TreeItemData<T>>>> loadChildrenAsync,
        Func<TreeItemData<T>, bool>? hasChildren = null)
    {
        ArgumentNullException.ThrowIfNull(loadChildrenAsync);
        _loadChildrenAsync = (node, _) => loadChildrenAsync(node);
        _hasChildren = hasChildren;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DelegateTreeNodeProvider{T}"/> class with cancellation token support.
    /// </summary>
    /// <param name="loadChildrenAsync">The delegate used to load child nodes asynchronously.</param>
    /// <param name="hasChildren">An optional predicate determining whether a node has children.</param>
    public DelegateTreeNodeProvider(
        Func<TreeItemData<T>, CancellationToken, Task<IEnumerable<TreeItemData<T>>>> loadChildrenAsync,
        Func<TreeItemData<T>, bool>? hasChildren = null)
    {
        ArgumentNullException.ThrowIfNull(loadChildrenAsync);
        _loadChildrenAsync = loadChildrenAsync;
        _hasChildren = hasChildren;
    }

    /// <inheritdoc />
    public bool HasChildren(TreeItemData<T> parent)
    {
        if (_hasChildren is not null)
        {
            return _hasChildren(parent);
        }

        return parent.HasChildren;
    }

    /// <inheritdoc />
    public Task<IEnumerable<TreeItemData<T>>> LoadChildrenAsync(TreeItemData<T> parent, CancellationToken cancellationToken = default)
    {
        return _loadChildrenAsync(parent, cancellationToken);
    }
}
