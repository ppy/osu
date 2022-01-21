// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Testing;
using osu.Framework.Threading;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays.Settings;

namespace osu.Game.Overlays
{
    /// <summary>
    /// Represents a <see cref="Container"/> with the ability to expand/contract when hovering the controls within it.
    /// </summary>
    /// <typeparam name="TControl">The type of UI control to lookup for hover expansion.</typeparam>
    public class ExpandingControlContainer<TControl> : Container, IExpandingContainer
        where TControl : class, IDrawable
    {
        private readonly float contractedWidth;
        private readonly float expandedWidth;

        public BindableBool Expanded { get; } = new BindableBool();

        /// <summary>
        /// Delay before the container switches to expanded state from hover.
        /// </summary>
        protected virtual double HoverExpansionDelay => 0;

        protected override Container<Drawable> Content => FillFlow;

        protected FillFlowContainer FillFlow { get; }

        protected ExpandingControlContainer(float contractedWidth, float expandedWidth)
        {
            this.contractedWidth = contractedWidth;
            this.expandedWidth = expandedWidth;

            RelativeSizeAxes = Axes.Y;
            Width = contractedWidth;

            InternalChild = new OsuScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                ScrollbarVisible = false,
                Child = FillFlow = new FillFlowContainer
                {
                    Origin = Anchor.CentreLeft,
                    Anchor = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                },
            };
        }

        private ScheduledDelegate hoverExpandEvent;
        private TControl activeControl;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Expanded.BindValueChanged(v =>
            {
                this.ResizeWidthTo(v.NewValue ? expandedWidth : contractedWidth, 500, Easing.OutQuint);
            }, true);
        }

        protected override void Update()
        {
            base.Update();

            // if the container was expanded from hovering over a control, we have to check per-frame whether we can contract it back.
            // that's because contracting the container depends not only on whether it's no longer hovered,
            // but also on whether the hovered control is no longer in a dragged state (if it was).
            if (hoverExpandEvent != null && !IsHovered && (activeControl == null || !isControlActive(activeControl)))
            {
                hoverExpandEvent?.Cancel();

                Expanded.Value = false;
                hoverExpandEvent = null;
                activeControl = null;
            }
        }

        protected override bool OnHover(HoverEvent e)
        {
            queueExpandIfHovering();
            return true;
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            queueExpandIfHovering();
            return base.OnMouseMove(e);
        }

        private void queueExpandIfHovering()
        {
            // if the same control is hovered or dragged, let the scheduled expand play out..
            if (activeControl != null && isControlActive(activeControl))
                return;

            // ..otherwise check whether a new control is hovered, and if so, queue a new hover operation.
            hoverExpandEvent?.Cancel();

            // usually we wouldn't use ChildrenOfType in implementations, but this is the simplest way
            // to handle cases like the editor where the controls may be nested within a child hierarchy.
            activeControl = FillFlow.ChildrenOfType<TControl>().FirstOrDefault(isControlActive);

            if (activeControl != null && !Expanded.Value)
                hoverExpandEvent = Scheduler.AddDelayed(() => Expanded.Value = true, HoverExpansionDelay);
        }

        /// <summary>
        /// Whether the given control is currently active, by checking whether it's hovered or dragged.
        /// </summary>
        private bool isControlActive(TControl control) => control.IsHovered || control.IsDragged || (control is ISettingsItem item && item.IsControlDragged);
    }
}
