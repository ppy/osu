﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
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

        private readonly CountCircle countDisplay;

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
                Origin = Anchor.Centre,
                Position = new Vector2(0.7f, 0.25f),
            });
        }

        [BackgroundDependencyLoader(true)]
        private void load(NotificationOverlay notificationOverlay)
        {
            StateContainer = notificationOverlay;

            if (notificationOverlay != null)
                NotificationCount.BindTo(notificationOverlay.UnreadCount);

            NotificationCount.ValueChanged += count =>
            {
                if (count == 0)
                    countDisplay.FadeOut(200, Easing.OutQuint);
                else
                {
                    countDisplay.Count = count;
                    countDisplay.FadeIn(200, Easing.OutQuint);
                }
            };
        }

        private class CountCircle : CompositeDrawable
        {
            private readonly OsuSpriteText countText;
            private readonly Circle circle;

            private int count;

            public int Count
            {
                get { return count; }
                set
                {
                    if (count == value)
                        return;

                    if (value > count)
                    {
                        circle.FlashColour(Color4.White, 600, Easing.OutQuint);
                        this.ScaleTo(1.1f).Then().ScaleTo(1, 600, Easing.OutElastic);
                    }

                    count = value;
                    countText.Text = value.ToString("#,0");
                }
            }

            public CountCircle()
            {
                AutoSizeAxes = Axes.X;

                InternalChildren = new Drawable[]
                {
                    circle = new Circle
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Red
                    },
                    countText = new OsuSpriteText
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
