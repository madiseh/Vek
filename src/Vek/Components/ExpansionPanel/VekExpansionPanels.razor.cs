using Microsoft.AspNetCore.Components;
using Vek.Interfaces;
using Vek.State;
using Vek.Utilities;

namespace Vek
{
    /// <summary>
    /// A container which manages <see cref="VekExpansionPanel"/> components such that when one panel is expanded the others are collapsed automatically.
    /// </summary>
    /// <seealso cref="VekExpansionPanel"/>
    /// <seealso cref="VekCollapse"/>
    public partial class VekExpansionPanels : VekComponentBase
    {
        private readonly List<VekExpansionPanel> _panels = new();
        private readonly ParameterState<AccordionExpandMode> _expandModeState;

        public VekExpansionPanels()
        {
            using var registerScope = CreateRegisterScope();
            registerScope.RegisterParameter<int>(nameof(Elevation))
                .WithParameter(() => Elevation)
                .WithChangeHandler(RenderPanels);
            _expandModeState = registerScope.RegisterParameter<AccordionExpandMode>(nameof(ExpandMode))
                .WithParameter(() => ExpandMode)
                .WithChangeHandler(OnExpandModeChangedAsync);
            registerScope.RegisterParameter<bool>(nameof(Dense))
                .WithParameter(() => Dense)
                .WithChangeHandler(RenderPanels);
            registerScope.RegisterParameter<bool>(nameof(Gutters))
                .WithParameter(() => Gutters)
                .WithChangeHandler(RenderPanels);
            registerScope.RegisterParameter<bool>(nameof(Outlined))
                .WithParameter(() => Outlined)
                .WithChangeHandler(RenderPanels);
        }

        protected string Classname =>
            new CssBuilder("vek-expansion-panels")
                .AddClass("vek-expansion-panels-square", Square)
                .AddClass(Class)
                .Build();

        /// <summary>
        /// Uses square corners for the panel.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.ExpansionPanel.Appearance)]
        public bool Square { get; set; }

        /// <summary>
        /// Controls how many panels can be expanded at the same time.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="AccordionExpandMode.SingleOrNone"/>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.ExpansionPanel.Behavior)]
        public AccordionExpandMode ExpandMode { get; set; } = AccordionExpandMode.SingleOrNone;

        /// <summary>
        /// Allows multiple panels to be expanded at the same time.
        /// </summary>
        /// <remarks>
        /// Use <see cref="ExpandMode"/> instead. Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.ExpansionPanel.Behavior)]
        public bool MultiExpansion { get; set; }

        /// <summary>
        /// The size of the drop shadow.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>1</c>.  A higher number creates a heavier drop shadow.  Use a value of <c>0</c> for no shadow.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.ExpansionPanel.Appearance)]
        public int Elevation { set; get; } = 1;

        /// <summary>
        /// Uses compact padding for all panels.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.ExpansionPanel.Appearance)]
        public bool Dense { get; set; }

        /// <summary>
        /// Adds left and right padding to all panels.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>true</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.ExpansionPanel.Appearance)]
        public bool Gutters { get; set; } = true;

        /// <summary>
        /// Shows borders around each panel.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>true</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.ExpansionPanel.Appearance)]
        public bool Outlined { get; set; } = true;

        /// <summary>
        /// The content within this component.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.ExpansionPanel.Behavior)]
        public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// A read-only list of the panels within this component. 
        /// </summary>
        /// <remarks>
        /// Expansion panels are controlled by adding more <see cref="VekExpansionPanel"/> components in the Razor page.
        /// </remarks>
        public IReadOnlyList<VekExpansionPanel> Panels => _panels;

        /// <summary>
        /// Renders every panel after a parameter that panels read from this component has changed.
        /// </summary>
        /// <remarks>
        /// Panels read these values through a fixed cascade, so a panel without content of its own is not re-rendered when they change.
        /// </remarks>
        private void RenderPanels()
        {
            foreach (IVekStateHasChanged panel in _panels)
                panel.StateHasChanged();
        }

        private AccordionExpandMode EffectiveExpandMode => MultiExpansion ? AccordionExpandMode.Multiple : _expandModeState.Value;

        internal bool CanCollapse(VekExpansionPanel panel) =>
            EffectiveExpandMode != AccordionExpandMode.Single || _panels.Any(other => other != panel && other._expandedState.Value);

        internal async Task AddPanelAsync(VekExpansionPanel panel)
        {
            var hasExpandedPanel = _panels.Any(existing => existing._expandedState.Value);
            _panels.Add(panel);

            if (EffectiveExpandMode != AccordionExpandMode.Multiple && hasExpandedPanel && panel._expandedState.Value)
            {
                await panel.CollapseAsync();
                return;
            }

            if (EffectiveExpandMode == AccordionExpandMode.Single && !hasExpandedPanel && !panel.Disabled)
            {
                await panel.ExpandAsync();
            }
        }

        internal void RemovePanel(VekExpansionPanel panel)
        {
            _panels.Remove(panel);
            try
            {
                StateHasChanged();
            }
            catch (InvalidOperationException) { /* this happens on page reload, probably a Blazor bug */ }
        }

        internal async Task NotifyPanelsChanged(VekExpansionPanel panel)
        {
            if (EffectiveExpandMode == AccordionExpandMode.Single && !panel._expandedState.Value && !_panels.Any(existing => existing._expandedState.Value))
            {
                await panel.ExpandAsync();
                return;
            }

            if (EffectiveExpandMode != AccordionExpandMode.Multiple && panel._expandedState.Value)
            {
                await CollapseAllExceptAsync(panel);
                return;
            }

            await UpdateAllAsync();
        }

        /// <summary>
        /// Refreshes the expansion state of all panels.
        /// </summary>
        public Task UpdateAllAsync()
        {
            VekExpansionPanel? last = null;
            foreach (var panel in _panels)
            {
                if (last is not null)
                {
                    last.NextPanelExpanded = panel._expandedState.Value;
                }

                last = panel;
            }
            RenderPanels();
            StateHasChanged();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Collapses all panels except the given one.
        /// </summary>
        /// <param name="panel">The panel to keep expanded.</param>
        public async Task CollapseAllExceptAsync(VekExpansionPanel panel)
        {
            foreach (var expansionPanel in _panels.Where(expansionPanel => expansionPanel != panel))
            {
                await expansionPanel.CollapseAsync();
            }

            await InvokeAsync(UpdateAllAsync);
        }

        /// <summary>
        /// Hides the content of all panels.
        /// </summary>
        public async Task CollapseAllAsync()
        {
            if (EffectiveExpandMode == AccordionExpandMode.Single)
            {
                return;
            }

            foreach (var expansionPanel in _panels)
            {
                await expansionPanel.CollapseAsync();
            }
            await InvokeAsync(UpdateAllAsync);
        }

        /// <summary>
        /// Shows the content of all panels.
        /// </summary>
        public async Task ExpandAllAsync()
        {
            foreach (var expansionPanel in _panels)
            {
                await expansionPanel.ExpandAsync();
            }
            await InvokeAsync(UpdateAllAsync);
        }

        private async Task OnExpandModeChangedAsync(ParameterChangedEventArgs<AccordionExpandMode> args)
        {
            if (args.Value == AccordionExpandMode.Single)
            {
                var expandedPanel = _panels.FirstOrDefault(panel => panel._expandedState.Value);
                var panelToKeep = expandedPanel ?? _panels.FirstOrDefault(panel => !panel.Disabled);

                foreach (var panel in _panels)
                {
                    if (panel == panelToKeep)
                    {
                        continue;
                    }

                    await panel.CollapseAsync();
                }

                if (panelToKeep is not null && !panelToKeep._expandedState.Value)
                {
                    await panelToKeep.ExpandAsync();
                }
            }

            RenderPanels();
        }
    }
}
