// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Threading;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.Game.Overlays.Settings
{
    public class Sidebar : Container<SidebarButton>, IStateful<ExpandedState>
    {
        private readonly FillFlowContainer<SidebarButton> content;
        public const float DEFAULT_WIDTH = Toolbar.Toolbar.HEIGHT * 1.4f;
        public const int EXPANDED_WIDTH = 200;

        public event Action<ExpandedState> StateChanged;

        protected override Container<SidebarButton> Content => content;

        public Sidebar()
        {
            RelativeSizeAxes = Axes.Y;
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = OsuColour.Gray(0.02f),
                    RelativeSizeAxes = Axes.Both,
                },
                new SidebarScrollContainer
                {
                    Children = new[]
                    {
                        content = new FillFlowContainer<SidebarButton>
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
                Content.Anchor = Anchor.CentreLeft;
                Content.Origin = Anchor.CentreLeft;
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
                        this.ResizeTo(new Vector2(DEFAULT_WIDTH, Height), 500, Easing.OutQuint);
                        break;

                    case ExpandedState.Expanded:
                        this.ResizeTo(new Vector2(EXPANDED_WIDTH, Height), 500, Easing.OutQuint);
                        break;
                }

                StateChanged?.Invoke(State);
            }
        }

        private Drawable lastHoveredButton;

        private Drawable hoveredButton => content.Children.FirstOrDefault(c => c.IsHovered);

        private void queueExpandIfHovering()
        {
            // only expand when we hover a different button.
            if (lastHoveredButton == hoveredButton) return;

            if (!IsHovered) return;

            if (State != ExpandedState.Expanded)
            {
                expandEvent?.Cancel();
                expandEvent = Scheduler.AddDelayed(() => State = ExpandedState.Expanded, 750);
            }

            lastHoveredButton = hoveredButton;
        }
    }

    public enum ExpandedState
    {
        Contracted,
        Expanded,
    }
}
