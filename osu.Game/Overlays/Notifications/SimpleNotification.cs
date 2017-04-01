﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.Notifications
{
    public class SimpleNotification : Notification
    {
        private string text;
        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                textDrawable.Text = text;
            }
        }

        private FontAwesome icon = FontAwesome.fa_info_circle;
        public FontAwesome Icon
        {
            get { return icon; }
            set
            {
                icon = value;
                iconDrawable.Icon = icon;
            }
        }

        private readonly SpriteText textDrawable;
        private readonly TextAwesome iconDrawable;

        protected Box IconBackgound;

        public SimpleNotification()
        {
            IconContent.Add(new Drawable[]
            {
                IconBackgound = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    ColourInfo = ColourInfo.GradientVertical(OsuColour.Gray(0.2f), OsuColour.Gray(0.6f))
                },
                iconDrawable = new TextAwesome
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Icon = icon,
                    TextSize = 20
                }
            });

            Content.Add(textDrawable = new OsuSpriteText
            {
                TextSize = 16,
                Colour = OsuColour.Gray(128),
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
                Text = text
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Light.Colour = colours.Green;
        }

        public override bool Read
        {
            get
            {
                return base.Read;
            }

            set
            {
                base.Read = value;
                Light.FadeTo(value ? 1 : 0, 100);
            }
        }
    }
}