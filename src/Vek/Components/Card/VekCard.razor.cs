using Microsoft.AspNetCore.Components;
using Vek.State;
using Vek.Utilities;

namespace Vek
{
    /// <summary>
    /// Cards contain actions, text, or media like images or graphics. Keeping a card to a single subject keeps the design clean.
    /// </summary>
    /// <seealso cref="VekCardActions" />
    /// <seealso cref="VekCardContent" />
    /// <seealso cref="VekCardHeader" />
    /// <seealso cref="VekCardMedia" />
    public partial class VekCard : VekComponentBase
    {
        private readonly ParameterState<bool> _expandedState;

        protected string Classname => new CssBuilder("vek-card")
            .AddClass(Class)
            .Build();

        /// <summary>
        /// The size of the drop shadow.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>1</c>.  A higher number creates a heavier drop shadow.  Use a value of <c>0</c> for no shadow.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Card.Appearance)]
        public int Elevation { set; get; } = 1;

        /// <summary>
        /// Disables rounded corners.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Card.Appearance)]
        public bool Square { get; set; }

        /// <summary>
        /// Displays an outline.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.  This property is useful to differentiate cards which are the same color or use images.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Card.Appearance)]
        public bool Outlined { get; set; }

        /// <summary>
        /// Adds visual padding to the content (<see cref="VekCardHeader"/>, <see cref="VekCardContent"/> or <see cref="VekCardActions"/>).
        /// </summary>
        /// <remarks>
        /// Defaults to <c>true</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Card.Appearance)]
        public bool ContentPadding { get; set; } = true;

        /// <summary>
        /// The content within this component.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.Card.Behavior)]
        public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// Allows the card body to be collapsed from its header.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Card.Behavior)]
        public bool Collapsible { get; set; }

        /// <summary>
        /// Displays or hides the card body when <see cref="Collapsible"/> is enabled.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>true</c>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.Card.Behavior)]
        public bool Expanded { get; set; } = true;

        /// <summary>
        /// Occurs when the expanded state changes.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.Card.Behavior)]
        public EventCallback<bool> ExpandedChanged { get; set; }

        internal bool IsExpanded => _expandedState.Value;

        public VekCard()
        {
            using var registerScope = CreateRegisterScope();
            _expandedState = registerScope.RegisterParameter<bool>(nameof(Expanded))
                .WithParameter(() => Expanded)
                .WithEventCallback(() => ExpandedChanged);
        }

        internal Task ToggleExpandedAsync()
        {
            return Collapsible ? _expandedState.SetValueAsync(!_expandedState.Value) : Task.CompletedTask;
        }
    }
}
