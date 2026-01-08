// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Threading;

namespace osu.Game.Graphics.Containers
{
    /// <summary>
    /// Represents a <see cref="Container"/> with the ability to expand/contract on hover.
    /// </summary>
    public partial class ExpandingContainer : Container, IExpandingContainer
    {
        public const double TRANSITION_DURATION = 500;

        private readonly float contractedWidth;
        private readonly float expandedWidth;

        public BindableBool Expanded { get; } = new BindableBool();

        /// <summary>
        /// Delay before the container switches to expanded state from hover.
        /// </summary>
        protected virtual double HoverExpansionDelay => 0;

        protected virtual bool ExpandOnHover => true;

        protected override Container<Drawable> Content => FillFlow;

        protected FillFlowContainer FillFlow { get; }

        protected ExpandingContainer(float contractedWidth, float expandedWidth)
        {
            this.contractedWidth = contractedWidth;
            this.expandedWidth = expandedWidth;

            RelativeSizeAxes = Axes.Y;
            Width = contractedWidth;

            InternalChild = CreateScrollContainer().With(s =>
            {
                s.RelativeSizeAxes = Axes.Both;
                s.ScrollbarVisible = false;
            }).WithChild(
                FillFlow = new FillFlowContainer
                {
                    Origin = Anchor.CentreLeft,
                    Anchor = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                }
            );
        }

        protected virtual OsuScrollContainer CreateScrollContainer() => new OsuScrollContainer();

        private InputManager inputManager = null!;

        /// <summary>
        /// Tracks whether the mouse was in bounds of this expanding container in the last frame.
        /// </summary>
        private bool? lastMouseInBounds;

        /// <summary>
        /// Tracks whether the last expansion of the container was caused by the mouse moving into its bounds
        /// (as opposed to an external set of `Expanded`, in which case moving the mouse outside of its bounds should not contract).
        /// </summary>
        private bool? expandedByMouse;

        private ScheduledDelegate? hoverExpandEvent;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Expanded.BindValueChanged(v =>
            {
                this.ResizeWidthTo(v.NewValue ? expandedWidth : contractedWidth, TRANSITION_DURATION, Easing.OutQuint);
            }, true);

            inputManager = GetContainingInputManager()!;
        }

        protected override void Update()
        {
            base.Update();

            bool mouseInBounds = Contains(inputManager.CurrentState.Mouse.Position);

            if (lastMouseInBounds != mouseInBounds)
                updateExpansionState(mouseInBounds);

            lastMouseInBounds = mouseInBounds;
        }

        private void updateExpansionState(bool mouseInBounds)
        {
            if (!ExpandOnHover)
                return;

            hoverExpandEvent?.Cancel();
            hoverExpandEvent = null;

            if (mouseInBounds && !Expanded.Value)
            {
                hoverExpandEvent = Scheduler.AddDelayed(() => Expanded.Value = true, HoverExpansionDelay);
                expandedByMouse = true;
            }

            if (!mouseInBounds && Expanded.Value)
            {
                if (expandedByMouse == true)
                    Expanded.Value = false;

                expandedByMouse = false;
            }
        }
    }
}
