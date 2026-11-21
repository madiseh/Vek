using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Components;
using Vek.Extensions;
using Vek.State;
using Vek.Utilities;

namespace Vek
{
    /// <summary>
    /// An extensively customizable tree view component for displaying hierarchical data, featuring item selection, lazy-loading, and templating support.
    /// </summary>
    /// <typeparam name="T">The type of item to display.</typeparam>
    /// <seealso cref="VekTreeViewItem{T}"/>
    /// <seealso cref="VekTreeViewItemToggleButton"/>
    public partial class VekTreeView<T> : VekComponentBase
    {
        public VekTreeView()
        {
            VekTreeRoot = this;
            using var registerScope = CreateRegisterScope();
            _selectedValueState = registerScope.RegisterParameter<T?>(nameof(SelectedValue))
                .WithParameter(() => SelectedValue)
                .WithEventCallback(() => SelectedValueChanged)
                .WithChangeHandler(OnSelectedValueChangedAsync)
                .WithComparer(() => Comparer);
            _selectedValuesState = registerScope.RegisterParameter<IReadOnlyCollection<T>?>(nameof(SelectedValues))
                .WithParameter(() => SelectedValues)
                .WithEventCallback(() => SelectedValuesChanged)
                .WithChangeHandler(OnSelectedValuesChangedAsync)
                .WithComparer(() => Comparer, comparer => new CollectionComparer<T>(comparer));
            registerScope.RegisterParameter<IEqualityComparer<T?>>(nameof(Comparer))
                .WithParameter(() => Comparer)
                .WithChangeHandler(OnComparerChangedAsync);
            registerScope.RegisterParameter<SelectionMode>(nameof(SelectionMode))
                .WithParameter(() => SelectionMode)
                .WithChangeHandler(OnParameterChangedAsync);
            registerScope.RegisterParameter<bool>(nameof(TriState))
                .WithParameter(() => TriState)
                .WithChangeHandler(OnParameterChangedAsync);
            registerScope.RegisterParameter<bool>(nameof(Disabled))
                .WithParameter(() => Disabled)
                .WithChangeHandler(OnParameterChangedAsync);
            registerScope.RegisterParameter<bool>(nameof(ReadOnly))
                .WithParameter(() => ReadOnly)
                .WithChangeHandler(OnParameterChangedAsync);
            registerScope.RegisterParameter<bool>(nameof(ExpandOnClick))
                .WithParameter(() => ExpandOnClick)
                .WithChangeHandler(OnParameterChangedAsync);
            registerScope.RegisterParameter<bool>(nameof(ExpandOnDoubleClick))
                .WithParameter(() => ExpandOnDoubleClick)
                .WithChangeHandler(OnParameterChangedAsync);
            registerScope.RegisterParameter<bool>(nameof(Ripple))
                .WithParameter(() => Ripple)
                .WithChangeHandler(OnParameterChangedAsync);
            registerScope.RegisterParameter<string>(nameof(CheckedIcon))
                .WithParameter(() => CheckedIcon)
                .WithChangeHandler(OnParameterChangedAsync);
            registerScope.RegisterParameter<string>(nameof(UncheckedIcon))
                .WithParameter(() => UncheckedIcon)
                .WithChangeHandler(OnParameterChangedAsync);
            registerScope.RegisterParameter<string>(nameof(IndeterminateIcon))
                .WithParameter(() => IndeterminateIcon)
                .WithChangeHandler(OnParameterChangedAsync);
            _allowDragDropState = registerScope.RegisterParameter<bool>(nameof(AllowDragDrop))
                .WithParameter(() => AllowDragDrop)
                .WithChangeHandler(OnParameterChangedAsync);
            _allowReparentingState = registerScope.RegisterParameter<bool>(nameof(AllowReparenting))
                .WithParameter(() => AllowReparenting)
                .WithChangeHandler(OnParameterChangedAsync);
            _loadChildrenAsyncState = registerScope.RegisterParameter<Func<TreeItemData<T>, Task<IEnumerable<TreeItemData<T>>>>?>(nameof(LoadChildrenAsync))
                .WithParameter(() => LoadChildrenAsync);
            _hasChildrenState = registerScope.RegisterParameter<Func<TreeItemData<T>, bool>?>(nameof(HasChildren))
                .WithParameter(() => HasChildren);
            _nodeProviderState = registerScope.RegisterParameter<ITreeNodeProvider<T>?>(nameof(NodeProvider))
                .WithParameter(() => NodeProvider);
            _selection = new();
        }

        private readonly ParameterState<T?> _selectedValueState;
        private readonly ParameterState<IReadOnlyCollection<T>?> _selectedValuesState;
        private readonly ParameterState<bool> _allowDragDropState;
        private readonly ParameterState<bool> _allowReparentingState;
        private readonly ParameterState<Func<TreeItemData<T>, Task<IEnumerable<TreeItemData<T>>>>?> _loadChildrenAsyncState;
        private readonly ParameterState<Func<TreeItemData<T>, bool>?> _hasChildrenState;
        private readonly ParameterState<ITreeNodeProvider<T>?> _nodeProviderState;

        internal bool GetAllowDragDrop() => _allowDragDropState.Value;
        internal bool GetAllowReparenting() => _allowReparentingState.Value;
        internal Func<TreeItemData<T>, Task<IEnumerable<TreeItemData<T>>>>? GetLoadChildrenAsync() => _loadChildrenAsyncState.Value;
        internal Func<TreeItemData<T>, bool>? GetHasChildren() => _hasChildrenState.Value;
        internal ITreeNodeProvider<T>? GetNodeProvider() => _nodeProviderState.Value;

        private HashSet<T> _selection;
        private readonly HashSet<VekTreeViewItem<T>> _childItems = new();
        // ServerData load state belongs to the backing node object, not the rendered component instance.
        // When the parent replaces Items with new node objects, the old entries can disappear with them.
        private readonly ConditionalWeakTable<ITreeItemData<T>, ServerDataState> _serverDataStates = new();
        private bool _isFirstRender = true;
        internal bool MultiSelection => SelectionMode == SelectionMode.MultiSelection;
        private bool ToggleSelection => SelectionMode == SelectionMode.ToggleSelection;

        protected string Classname =>
            new CssBuilder("vek-treeview")
                .AddClass("vek-treeview-dense", Dense)
                .AddClass("vek-treeview-hover", !Disabled && Hover && (!ReadOnly || ExpandOnClick))
                .AddClass($"vek-treeview-selected-{Color.ToStringFast(true)}")
                .AddClass($"vek-treeview-checked-{CheckBoxColor.ToStringFast(true)}")
                .AddClass(Class)
                .Build();

        protected string Stylename =>
            new StyleBuilder()
                .AddStyle($"width", Width, !string.IsNullOrWhiteSpace(Width))
                .AddStyle($"height", Height, !string.IsNullOrWhiteSpace(Height))
                .AddStyle($"max-height", MaxHeight, !string.IsNullOrWhiteSpace(MaxHeight))
                .AddStyle(Style)
                .Build();

        [CascadingParameter]
        private VekTreeView<T> VekTreeRoot { get; set; }

        /// <summary>
        /// The color of the selected item.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Color.Primary"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.TreeView.Selecting)]
        public Color Color { get; set; } = Color.Primary;

        /// <summary>
        /// The color of checkboxes.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Color.Default"/>. Only applies when <see cref="SelectionMode"/> is <see cref="SelectionMode.MultiSelection" />.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.TreeView.Selecting)]
        public Color CheckBoxColor { get; set; }

        /// <summary>
        /// Controls how many items can be selected at one time.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="SelectionMode.SingleSelection"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.TreeView.Selecting)]
        public SelectionMode SelectionMode { get; set; } = SelectionMode.SingleSelection;

        /// <summary>
        /// Uses checkboxes which support an undetermined state.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>true</c>. Only applies when <see cref="SelectionMode"/> is <see cref="SelectionMode.MultiSelection"/>. When set, 
        /// an item's checkbox will be in the "undetermined" state if child items have a mix of checked and unchecked states.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.TreeView.Selecting)]
        public bool TriState { get; set; } = true;

        /// <summary>
        /// Automatically checks an item if all children are selected.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>true</c>. Only applies when <see cref="SelectionMode"/> is <see cref="SelectionMode.MultiSelection"/>. 
        /// Items will also be deselected if any children are deselected.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.TreeView.Selecting)]
        public bool AutoSelectParent { get; set; } = true;

        /// <summary>
        /// Expands an item with children if it is clicked anywhere (not just the expand/collapse buttons).
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.TreeView.ClickAction)]
        public bool ExpandOnClick { get; set; }

        /// <summary>
        /// Expands an item with children if it is double-clicked anywhere (not just the expand/collapse buttons).
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.TreeView.ClickAction)]
        public bool ExpandOnDoubleClick { get; set; }

        /// <summary>
        /// Automatically expands items to show selected children.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.TreeView.Selecting)]
        public bool AutoExpand { get; set; }

        /// <summary>
        /// Shows an effect when items are hovered over.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.TreeView.Appearance)]
        public bool Hover { get; set; }

        /// <summary>
        /// Uses compact vertical padding.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.TreeView.Appearance)]
        public bool Dense { get; set; }

        /// <summary>
        /// Sets a fixed height.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>. Can be a CSS value such as <c>500px</c> or <c>30%</c>. When set, items can be scrolled vertically. Otherwise, the height will grow automatically.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.TreeView.Appearance)]
        public string? Height { get; set; }

        /// <summary>
        /// Sets a maximum height.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>. Can be a CSS value such as <c>500px</c> or <c>30%</c>. When set, items can be scrolled vertically. Otherwise, the height will grow automatically.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.TreeView.Appearance)]
        public string? MaxHeight { get; set; }

        /// <summary>
        /// Sets a fixed width.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>. Can be a CSS value such as <c>500px</c> or <c>30%</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.TreeView.Appearance)]
        public string? Width { get; set; }

        /// <summary>
        /// Prevents the user from interacting with any items.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.TreeView.Behavior)]
        public bool Disabled { get; set; }

        /// <summary>
        /// Enables drag-and-drop node reordering and reparenting.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.TreeView.Behavior)]
        public bool AllowDragDrop { get; set; }

        /// <summary>
        /// Determines whether dragging a node into another node as a child (reparenting) is permitted.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>true</c>. When set to <c>false</c>, only reordering before and after sibling nodes is allowed.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.TreeView.Behavior)]
        public bool AllowReparenting { get; set; } = true;

        /// <summary>
        /// An event callback that occurs when a node is dropped onto a target node.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.TreeView.Behavior)]
        public EventCallback<TreeNodeDropEventArgs<T>> OnNodeDrop { get; set; }

        /// <summary>
        /// A predicate function to determine whether a drop operation is valid.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.TreeView.Behavior)]
        public Func<TreeNodeDropEventArgs<T>, bool>? CanDropFunc { get; set; }

        /// <summary>
        /// Coordinates drag-and-drop state across tree view nodes.
        /// </summary>
        internal TreeNodeDragManager DragManager { get; } = new();

        /// <summary>
        /// Programmatically drops a source item onto a target item with a specified drop position.
        /// </summary>
        public async Task<bool> DropNodeAsync(VekTreeViewItem<T> sourceItem, VekTreeViewItem<T> targetItem, TreeItemDropPosition dropPosition)
        {
            if (sourceItem == null || targetItem == null || sourceItem == targetItem)
                return false;

            if (targetItem.IsDescendantOf(sourceItem))
                return false;

            var args = new TreeNodeDropEventArgs<T>
            {
                SourceItem = sourceItem,
                TargetItem = targetItem,
                SourceValue = sourceItem.Value,
                TargetValue = targetItem.Value,
                DropPosition = dropPosition
            };

            if (CanDropFunc != null && !CanDropFunc(args))
                return false;

            await OnNodeDrop.InvokeAsync(args);
            return !args.Cancel;
        }

        /// <summary>
        /// Determines whether items are displayed.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>. The function provides an item and should return <c>true</c> to display the item, or <c>false</c> to hide it.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.TreeView.Behavior)]
        public Func<ITreeItemData<T>, Task<bool>>? FilterFunc { get; set; }

        /// <summary>
        /// Shows a ripple effect when an item is clicked.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>true</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.TreeView.Appearance)]
        public bool Ripple { get; set; } = true;

        /// <summary>
        /// The items being displayed.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.TreeView.Data)]
        public IReadOnlyCollection<ITreeItemData<T>>? Items { get; set; } = Array.Empty<ITreeItemData<T>>();

        /// <summary>
        /// The currently selected value.
        /// </summary>
        /// <remarks>
        /// Applies when <see cref="SelectionMode"/> is <see cref="SelectionMode.SingleSelection"/>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.TreeView.Selecting)]
        public T? SelectedValue { get; set; }

        /// <summary>
        /// Occurs when <see cref="SelectedValue"/> has changed.
        /// </summary>
        [Parameter]
        public EventCallback<T?> SelectedValueChanged { get; set; }

        /// <summary>
        /// The currently selected values.
        /// </summary>
        /// <remarks>
        /// Applies when <see cref="SelectionMode"/> is <see cref="SelectionMode.MultiSelection"/> or <see cref="SelectionMode.ToggleSelection"/>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.TreeView.Selecting)]
        public IReadOnlyCollection<T>? SelectedValues { get; set; }

        /// <summary>
        /// Occurs when <see cref="SelectedValues"/> has changed.
        /// </summary>
        [Parameter]
        public EventCallback<IReadOnlyCollection<T>?> SelectedValuesChanged { get; set; }

        /// <summary>
        /// The content within this component.
        /// </summary>
        /// <remarks>
        /// Applies when <see cref="ItemTemplate"/> and <see cref="Items"/> are both not set.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.TreeView.Data)]
        public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// The template for rendering each item.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.TreeView.Data)]
        public RenderFragment<ITreeItemData<T>>? ItemTemplate { get; set; }

        /// <summary>
        /// The comparer used to check if two items are equal.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.TreeView.Selecting)]
        public IEqualityComparer<T?> Comparer { get; set; } = EqualityComparer<T?>.Default;

        /// <summary>
        /// The function for asynchronously loading items.
        /// </summary>
        /// <remarks>
        /// When set, the function will be called to load the children of a parent item. 
        /// When the parent node is <c>null</c>, top-level items should be returned.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.TreeView.Data)]
        public Func<T?, Task<IReadOnlyCollection<TreeItemData<T>>>>? ServerData { get; set; }

        /// <summary>
        /// An asynchronous delegate called to load child nodes on demand when an item is expanded.
        /// </summary>
        /// <remarks>
        /// Loaded child nodes are cached to avoid refetching on subsequent collapse and re-expand actions.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.TreeView.Data)]
        public Func<TreeItemData<T>, Task<IEnumerable<TreeItemData<T>>>>? LoadChildrenAsync { get; set; }

        /// <summary>
        /// An optional predicate function determining whether a node has children and should display an expand chevron.
        /// </summary>
        /// <remarks>
        /// When supplied, this predicate controls whether the chevron appears before children are fetched.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.TreeView.Behavior)]
        public Func<TreeItemData<T>, bool>? HasChildren { get; set; }

        /// <summary>
        /// A provider for resolving and loading hierarchical tree node data on demand.
        /// </summary>
        [Parameter, ParameterState]
        [Category(CategoryTypes.TreeView.Data)]
        public ITreeNodeProvider<T>? NodeProvider { get; set; }

        /// <summary>
        /// Prevents selections from being changed.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>. When <c>true</c>, selections cannot be changed, but the current
        /// selections will continue to be displayed.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.List.Selecting)]
        public bool ReadOnly { get; set; }

        /// <summary>
        /// The icon displayed for checked items.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Icons.Material.Filled.CheckBox"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.TreeView.Selecting)]
        public string CheckedIcon { get; set; } = Icons.Material.Filled.CheckBox;

        /// <summary>
        /// The icon displayed for unchecked items.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Icons.Material.Filled.CheckBoxOutlineBlank"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.TreeView.Selecting)]
        public string UncheckedIcon { get; set; } = Icons.Material.Filled.CheckBoxOutlineBlank;

        /// <summary>
        /// The icon displayed for indeterminate items.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Icons.Material.Filled.IndeterminateCheckBox"/>. Only applies when <see cref="TriState"/> is <c>true</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.TreeView.Selecting)]
        public string IndeterminateIcon { get; set; } = Icons.Material.Filled.IndeterminateCheckBox;

        /// <inheritdoc />
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && VekTreeRoot == this)
            {
                _isFirstRender = false;
                await UpdateItemsAsync();
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        /// <summary>
        /// Filters all items based on the <see cref="FilterFunc"/> function.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task FilterAsync()
        {
            if (Items is null)
            {
                return;
            }

            if (FilterFunc is null)
            {
                ResetFilter(Items);
                return;
            }

            await TraverseFilterAsync(Items);
        }

        /// <summary>
        /// The internal filter logic that traverses the tree recursively and applies the <see cref="FilterFunc"/> to every item to set the <see cref="VekTreeViewItem{T}.Visible"/> property
        /// </summary>
        /// <param name="items">The hierarchical tree structure to traverse</param>
        /// <returns>A task to represent the asynchronous operation.</returns>
        private async Task TraverseFilterAsync(IEnumerable<ITreeItemData<T>> items)
        {
            foreach (var item in items)
            {
                if (item.HasChildren)
                {
                    /* Recursively traverse the tree. Since HasChildren performs the null check on the children we can use the null-forgiving operator here.
                     * Same goes for the FilterFunc which is checked for null in the public Filter function that invokes this function.
                     */
                    await TraverseFilterAsync(item.Children);
                    item.Expanded = item.Visible = await FilterFunc!(item) || item.Children.Any(c => c.Visible);
                }
                else
                {
                    item.Expanded = item.Visible = await FilterFunc!(item);
                }
            }
        }

        /// <summary>
        /// Resets the filter, so that all <see cref="VekTreeViewItem{T}.Visible"/> are set to true and the entire tree is visible.
        /// </summary>
        /// <param name="items">The items to reset</param>
        private static void ResetFilter(IEnumerable<ITreeItemData<T>> items)
        {
            foreach (var item in items)
            {
                if (item.HasChildren)
                {
                    ResetFilter(item.Children);
                }

                item.Visible = true;
            }
        }

        /// <summary>
        /// Expands all items and their children.
        /// </summary>
        public async Task ExpandAllAsync()
        {
            foreach (var item in _childItems)
            {
                await item.ExpandAllAsync();
            }
        }

        /// <summary>
        /// Collapses all items and their children.
        /// </summary>
        public async Task CollapseAllAsync()
        {
            foreach (var item in _childItems)
            {
                await item.CollapseAllAsync();
            }
        }

        /// <summary>
        /// SingleSelection or ToggleSelection: SelectedValue was updated via binding
        /// </summary>
        private Task OnSelectedValueChangedAsync(ParameterChangedEventArgs<T?> args)
        {
            // on first render the children are not yet initialized, so ignore this update
            if (_isFirstRender)
            {
                return Task.CompletedTask;
            }
            return SetSelectedValueAsync(args.Value);
        }

        /// <summary>
        /// MultiSelection: SelectedValues was updated via binding
        /// </summary>
        private Task OnSelectedValuesChangedAsync(ParameterChangedEventArgs<IReadOnlyCollection<T>?> args)
        {
            if (_isFirstRender)
            {
                // on first render the children are not yet initialized, so just initialize the selection
                _selection = args.Value is not null ? new HashSet<T>(args.Value!, Comparer) : new HashSet<T>(Comparer);
                return Task.CompletedTask;
            }
            return SetSelectedValuesAsync(args.Value ?? Array.Empty<T>());
        }

        private Task OnComparerChangedAsync(ParameterChangedEventArgs<IEqualityComparer<T?>> args)
        {
            if (_isFirstRender)
            {
                return Task.CompletedTask;
            }
            return UpdateItemsAsync();
        }

        private Task OnParameterChangedAsync()
        {
            if (_isFirstRender)
            {
                return Task.CompletedTask;
            }
            // Items read these from the root through a fixed cascade, so walking the tree is the only thing that pushes the new value down to them.
            return UpdateItemsAsync(forceRender: true);
        }

        internal async Task OnItemClickAsync(VekTreeViewItem<T> clickedItem)
        {
            if (ReadOnly)
            {
                return;
            }
            if (MultiSelection)
            {
                var items = clickedItem.GetChildItemsRecursive();
                items.Add(clickedItem!);
                // Items without a value can never be selected, so counting them would keep allSelected false and the selection could never be cleared.
                items.RemoveAll(x => x.GetValue() is null);
                var allSelected = items.All(x => x.GetState<bool>(nameof(VekTreeViewItem<T>.Selected)));
                // toggle selection of the clickedItem and its children
                foreach (var item in items)
                {
                    if (allSelected)
                    {
                        _selection.Remove(item.GetValue()!);
                    }
                    else
                    {
                        _selection.Add(item.GetValue()!);
                    }
                }
                if (AutoSelectParent)
                {
                    UpdateParentItem(clickedItem.Parent);
                }
                await _selectedValuesState.SetValueAsync(_selection.ToList()); // note: .ToList() is essential here!
                await UpdateItemsAsync();
                return;
            }
            var selected = clickedItem.GetState<bool>(nameof(VekTreeViewItem<T>.Selected));
            if (ToggleSelection)
            {
                await SetSelectedValueAsync(selected ? default : clickedItem.GetValue()); // <-- toggle selected value
            }
            else if (!selected)
            {
                // SingleSelection
                await SetSelectedValueAsync(clickedItem.GetValue());
            }
        }

        /// <summary>
        /// This changes the parent item's state based on the selection state of its children in multi-selection mode
        /// But only if the items are clicked, not when the selection is modified via SelectedValues
        /// </summary>
        private void UpdateParentItem(VekTreeViewItem<T>? parentItem)
        {
            while (parentItem is not null)
            {
                var parentValue = parentItem.GetValue();
                if (parentValue is not null)
                {
                    var parentSelected = AreAllChildrenSelected(parentItem, out var hasSelectableChildren) && hasSelectableChildren;
                    if (parentSelected)
                    {
                        _selection.Add(parentValue);
                    }
                    else
                    {
                        _selection.Remove(parentValue);
                    }
                }
                parentItem = parentItem.Parent;
            }
        }

        private bool AreAllChildrenSelected(VekTreeViewItem<T> parentItem, out bool hasSelectableChildren)
        {
            hasSelectableChildren = false;
            foreach (var child in parentItem.ChildItems)
            {
                var childValue = child.GetValue();
                if (childValue is not null)
                {
                    hasSelectableChildren = true;
                    if (!_selection.Contains(childValue))
                    {
                        return false;
                    }
                }

                if (!AreAllChildrenSelected(child, out var hasSelectableDescendants))
                {
                    hasSelectableChildren |= hasSelectableDescendants;
                    return false;
                }

                hasSelectableChildren |= hasSelectableDescendants;
            }

            return true;
        }

        internal async Task AddChildAsync(VekTreeViewItem<T> item)
        {
            _childItems.Add(item);
            // this is to ensure that setting Selected="true" on the item will update the single/multiselection.
            // Note: Setting Selected="false" has no effect however because it would cancel the initialization of the SelectedValue or SelectedValues !
            var value = item.GetValue();
            if (value is not null && item.GetState<bool>(nameof(VekTreeViewItem<T>.Selected)))
            {
                await SelectAsync(value);
            }
            await item.UpdateSelectionStateAsync(GetSelection());
        }

        internal void RemoveChild(VekTreeViewItem<T> item)
        {
            _childItems.Remove(item);
        }

        internal async Task SelectAsync(T value)
        {
            if (MultiSelection)
            {
                _selection.Add(value);
                if (!_isFirstRender)
                {
                    await _selectedValuesState.SetValueAsync(_selection.ToList()); // note: .ToList() is essential here!
                    await UpdateItemsAsync();
                }
                return;
            }
            // single and toggle selection
            await _selectedValueState.SetValueAsync(value);
            if (!_isFirstRender)
            {
                await UpdateItemsAsync();
            }
        }

        internal async Task UnselectAsync(T value)
        {
            if (_isFirstRender || !MultiSelection)
            {
                return;
            }
            _selection.Remove(value);
            await _selectedValuesState.SetValueAsync(_selection.ToList()); // note: .ToList() is essential here!
        }

        ///  <summary>
        ///  Sets the selected value of the tree view in Single- and ToggleSelection mode.
        ///  If the value is found, the corresponding item is selected; 
        ///  otherwise, selected value is set default.
        ///  If the selected item is valid it sets the corresponding tree item to selected.
        ///  </summary>
        ///  <param name="value">The value to be set as the selected value.</param>
        internal async Task SetSelectedValueAsync(T? value)
        {
            var isValid = value != null && GetSelectableValues().Contains(value);
            // note: if there is no item that corresponds to the value, the value is reset to default!
            await _selectedValueState.SetValueAsync(isValid ? value : default);
            await UpdateItemsAsync();
        }

        ///  <summary>
        ///  Sets the selected values of the tree view in MultiSelection mode.
        /// Discard any values which are not represented by child values.
        ///  </summary>
        private async Task SetSelectedValuesAsync(IReadOnlyCollection<T> newValues)
        {
            var allChildValues = GetSelectableValues();
            var newSelection = new HashSet<T>(newValues.Where(x => allChildValues.Contains(x)), Comparer);
            if (_selection.SetEquals(newSelection))
            {
                return;
            }
            _selection = newSelection;
            await _selectedValuesState.SetValueAsync(newSelection);
            await UpdateItemsAsync();
        }

        /// <summary>
        /// Let the items update their selection state visualization and state according to
        /// the selection in the tree view
        /// </summary>
        /// <param name="forceRender">Renders every item regardless of whether its state changed.</param>
        private async Task UpdateItemsAsync(bool forceRender = false)
        {
            var selection = GetSelection();
            foreach (var item in _childItems)
            {
                await item.UpdateSelectionStateAsync(selection, forceRender);
            }
        }

        private HashSet<T> GetSelection()
        {
            HashSet<T> selection;
            if (MultiSelection)
            {
                selection = new HashSet<T>(_selection, Comparer);
            }
            else
            {
                selection = new HashSet<T>(Comparer);
                if (_selectedValueState.Value != null)
                {
                    selection.Add(_selectedValueState.Value);
                }
            }
            return selection;
        }

        private HashSet<T> GetSelectableValues()
        {
            if (ItemTemplate is not null && Items is not null)
            {
                return GetItemValuesRecursive(Items);
            }

            return GetChildValuesRecursive();
        }

        // TODO: speed this up with caching
        private HashSet<T> GetItemValuesRecursive(IEnumerable<ITreeItemData<T>> items, HashSet<T>? values = null)
        {
            values ??= new HashSet<T>(Comparer);

            foreach (var item in items)
            {
                if (item.Value is not null)
                {
                    values.Add(item.Value);
                }

                if (item.Children is not null && item.Children.Count > 0)
                {
                    GetItemValuesRecursive(item.Children, values);
                }
            }

            return values;
        }

        // TODO: speed this up with caching
        private HashSet<T> GetChildValuesRecursive(IEnumerable<VekTreeViewItem<T>>? children = null, HashSet<T>? values = null)
        {
            values ??= new HashSet<T>(Comparer);
            children ??= _childItems;

            foreach (var item in children)
            {
                var value = item.GetValue();
                if (value is not null)
                {
                    values.Add(value);
                }

                if (item.ChildItems.Count > 0)
                {
                    GetChildValuesRecursive(item.ChildItems, values);
                }
            }

            return values;
        }


        /// <summary>
        /// Clears cached children and reloads the child nodes for the specified data item.
        /// </summary>
        /// <param name="item">The data item whose children should be reloaded.</param>
        public async Task ReloadNodeAsync(ITreeItemData<T> item)
        {
            ArgumentNullException.ThrowIfNull(item);
            SetServerDataLoaded(item, false);
            item.Children = null;

            foreach (var rootItem in _childItems)
            {
                if (ReferenceEquals(rootItem.CurrentItemData, item) || Equals(rootItem.GetValue(), item.Value))
                {
                    await rootItem.ReloadAsync();
                    return;
                }

                foreach (var child in rootItem.GetChildItemsRecursive())
                {
                    if (ReferenceEquals(child.CurrentItemData, item) || Equals(child.GetValue(), item.Value))
                    {
                        await child.ReloadAsync();
                        return;
                    }
                }
            }
        }

        internal bool GetServerDataLoaded(ITreeItemData<T> item) => _serverDataStates.GetOrCreateValue(item).IsLoaded;

        internal void SetServerDataLoaded(ITreeItemData<T> item, bool isLoaded) => _serverDataStates.GetOrCreateValue(item).IsLoaded = isLoaded;

        private sealed class ServerDataState
        {
            public bool IsLoaded { get; set; }
        }
    }
}
