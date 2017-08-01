// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Framework.Threading;
using osu.Game.Overlays.Toolbar;

namespace osu.Game.Overlays.Settings
{
    public class Sidebar : Container, IStateful<ExpandedState>
    {
        private readonly FillFlowContainer content;
        internal const float DEFAULT_WIDTH = ToolbarButton.WIDTH;
        internal const int EXPANDED_WIDTH = 200;
        protected override Container<Drawable> Content => content;

        public Sidebar()
        {
            RelativeSizeAxes = Axes.Y;
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = Color4.Black,
                    RelativeSizeAxes = Axes.Both,
                },
                new SidebarScrollContainer
                {
                    Children = new[]
                    {
                        content = new FillFlowContainer
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

        protected override bool OnHover(InputState state)
        {
            queueExpandIfHovering();
            return true;
        }

        protected override void OnHoverLost(InputState state)
        {
            expandEvent?.Cancel();
            lastHoveredButton = null;
            State = ExpandedState.Contracted;

            base.OnHoverLost(state);
        }

        protected override bool OnMouseMove(InputState state)
        {
            queueExpandIfHovering();
            return base.OnMouseMove(state);
        }

        private class SidebarScrollContainer : ScrollContainer
        {
            public SidebarScrollContainer()
            {
                Content.Anchor = Anchor.CentreLeft;
                Content.Origin = Anchor.CentreLeft;
                RelativeSizeAxes = Axes.Both;
            }
        }

        public ExpandedState State
        {
            get { return state; }
            set
            {
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