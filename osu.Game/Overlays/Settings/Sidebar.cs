// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Framework.Threading;
using osu.Game.Overlays.Toolbar;

namespace osu.Game.Overlays.Settings
{
    public class Sidebar : Container
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
                    Children = new []
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

        protected override bool OnHover(InputState state)
        {
            expandEvent = Scheduler.AddDelayed(() =>
            {
                expandEvent = null;
                ResizeTo(new Vector2(EXPANDED_WIDTH, Height), 150, EasingTypes.OutQuad);
            }, 750);
            return true;
        }

        protected override void OnHoverLost(InputState state)
        {
            expandEvent?.Cancel();
            ResizeTo(new Vector2(DEFAULT_WIDTH, Height), 150, EasingTypes.OutQuad);
            base.OnHoverLost(state);
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
    }
}