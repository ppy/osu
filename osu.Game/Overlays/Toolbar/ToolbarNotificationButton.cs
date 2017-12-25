// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarNotificationButton : ToolbarOverlayToggleButton
    {
        protected override Anchor TooltipAnchor => Anchor.TopRight;

        public BindableInt NotificationCount = new BindableInt();

        private CountCircle countDisplay;

        public ToolbarNotificationButton()
        {
            Icon = FontAwesome.fa_bars;
            TooltipMain = "Notifications";
            TooltipSub = "Waiting for 'ya";

            Add(countDisplay = new CountCircle
            {
                Alpha = 0,
                Height = 16,
                RelativePositionAxes = Axes.Both,
                Origin = Anchor.TopCentre,
                Position = new Vector2(0.7f, 0.05f),
            });
        }

        [BackgroundDependencyLoader(true)]
        private void load(NotificationOverlay notificationOverlay)
        {
            StateContainer = notificationOverlay;

            NotificationCount.ValueChanged += count =>
            {
                if (count == 0)
                    countDisplay.FadeOut(200, Easing.OutQuint);
                else
                    countDisplay.FadeIn(200, Easing.OutQuint);

                countDisplay.Count = count;
            };
        }

        private class CountCircle : CompositeDrawable
        {
            private readonly OsuSpriteText count;

            public int Count
            {
                set { count.Text = value.ToString("#,0"); }
            }

            public CountCircle()
            {
                AutoSizeAxes = Axes.X;

                InternalChildren = new Drawable[]
                {
                    new Circle
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Red
                    },
                    count = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Y = -1,
                        TextSize = 14,
                        Padding = new MarginPadding(5),
                        Colour = Color4.White,
                        UseFullGlyphHeight = true,
                        Font = "Exo2.0-Bold",
                    }
                };
            }
        }
    }
}
