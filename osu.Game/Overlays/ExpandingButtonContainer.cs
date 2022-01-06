// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Testing;
using osu.Framework.Threading;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osuTK;

namespace osu.Game.Overlays
{
    public abstract class ExpandingButtonContainer : Container, IStateful<ExpandedState>
    {
        private readonly float contractedWidth;
        private readonly float expandedWidth;

        public event Action<ExpandedState> StateChanged;

        protected override Container<Drawable> Content => FillFlow;

        protected FillFlowContainer FillFlow { get; }

        protected ExpandingButtonContainer(float contractedWidth, float expandedWidth)
        {
            this.contractedWidth = contractedWidth;
            this.expandedWidth = expandedWidth;

            RelativeSizeAxes = Axes.Y;
            Width = contractedWidth;

            InternalChildren = new Drawable[]
            {
                new SidebarScrollContainer
                {
                    Children = new[]
                    {
                        FillFlow = new FillFlowContainer
                        {
                            Origin = Anchor.CentreLeft,
                            Anchor = Anchor.CentreLeft,
                            AutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.X,
                            Direction = FillDirection.Vertical,
                        }
                    }
                },
            };
        }

        private ScheduledDelegate expandEvent;
        private ExpandedState state;

        protected override bool OnHover(HoverEvent e)
        {
            queueExpandIfHovering();
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            expandEvent?.Cancel();
            lastHoveredButton = null;
            State = ExpandedState.Contracted;

            base.OnHoverLost(e);
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            queueExpandIfHovering();
            return base.OnMouseMove(e);
        }

        private class SidebarScrollContainer : OsuScrollContainer
        {
            public SidebarScrollContainer()
            {
                RelativeSizeAxes = Axes.Both;
                ScrollbarVisible = false;
            }
        }

        public ExpandedState State
        {
            get => state;
            set
            {
                expandEvent?.Cancel();

                if (state == value) return;

                state = value;

                switch (state)
                {
                    default:
                        this.ResizeTo(new Vector2(contractedWidth, Height), 500, Easing.OutQuint);
                        break;

                    case ExpandedState.Expanded:
                        this.ResizeTo(new Vector2(expandedWidth, Height), 500, Easing.OutQuint);
                        break;
                }

                StateChanged?.Invoke(State);
            }
        }

        private Drawable lastHoveredButton;

        private Drawable hoveredButton => FillFlow.ChildrenOfType<OsuButton>().FirstOrDefault(c => c.IsHovered);

        private void queueExpandIfHovering()
        {
            // only expand when we hover a different button.
            if (lastHoveredButton == hoveredButton) return;

            if (State != ExpandedState.Expanded)
            {
                expandEvent?.Cancel();
                expandEvent = Scheduler.AddDelayed(() => State = ExpandedState.Expanded, 750);
            }

            lastHoveredButton = hoveredButton;
        }
    }
}
