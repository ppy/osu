// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.Game.Overlays.Notifications
{
    public class SimpleNotification : Notification
    {
        private string text = string.Empty;

        public string Text
        {
            get => text;
            set
            {
                text = value;
                textDrawable.Text = text;
            }
        }

        private IconUsage icon = FontAwesome.Solid.InfoCircle;

        public IconUsage Icon
        {
            get => icon;
            set
            {
                icon = value;
                iconDrawable.Icon = icon;
            }
        }

        private readonly TextFlowContainer textDrawable;
        private readonly SpriteIcon iconDrawable;

        protected Box IconBackgound;

        public SimpleNotification()
        {
            IconContent.AddRange(new Drawable[]
            {
                IconBackgound = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = ColourInfo.GradientVertical(OsuColour.Gray(0.2f), OsuColour.Gray(0.6f))
                },
                iconDrawable = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Icon = icon,
                    Size = new Vector2(20),
                }
            });

            Content.Add(textDrawable = new OsuTextFlowContainer(t => t.Font = t.Font.With(size: 14))
            {
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
            get => base.Read;
            set
            {
                if (value == base.Read) return;

                base.Read = value;
                Light.FadeTo(value ? 0 : 1, 100);
            }
        }
    }
}
