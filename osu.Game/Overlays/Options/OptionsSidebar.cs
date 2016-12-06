//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Input;
using osu.Framework.Threading;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Options
{
    public class OptionsSidebar : Container
    {
        private FlowContainer content;
        internal const int default_width = 60, expanded_width = 200;
        protected override Container<Drawable> Content => content;

        public OptionsSidebar()
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
                        content = new FlowContainer
                        {
                            Origin = Anchor.CentreLeft,
                            Anchor = Anchor.CentreLeft,
                            AutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.X,
                            Direction = FlowDirection.VerticalOnly
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
                ResizeTo(new Vector2(expanded_width, Height), 150, EasingTypes.OutQuad);
            }, 750);
            return true;
        }
        
        protected override void OnHoverLost(InputState state)
        {
            expandEvent?.Cancel();
            ResizeTo(new Vector2(default_width, Height), 150, EasingTypes.OutQuad);
            base.OnHoverLost(state);
        }

        private class SidebarScrollContainer : ScrollContainer
        {
            public SidebarScrollContainer()
            {
                Content.Anchor = Anchor.CentreLeft;
                Content.Origin = Anchor.CentreLeft;
            }
        }
    }
}