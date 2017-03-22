// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.Sprites;
using OpenTK.Graphics;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuDropDownMenuItem<U> : DropDownMenuItem<U>
    {
        public OsuDropDownMenuItem(string text, U value) : base(text, value)
        {
            Foreground.Padding = new MarginPadding(2);

            Masking = true;
            CornerRadius = 6;

            Children = new[]
            {
                new FillFlowContainer
                {
                    Direction = FillDirection.Horizontal,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        chevron = new TextAwesome
                        {
                            AlwaysPresent = true,
                            Icon = FontAwesome.fa_chevron_right,
                            UseFullGlyphHeight = false,
                            Colour = Color4.Black,
                            Alpha = 0.5f,
                            TextSize = 8,
                            Margin = new MarginPadding { Left = 3, Right = 3 },
                            Origin = Anchor.CentreLeft,
                            Anchor = Anchor.CentreLeft,
                        },
                        new OsuSpriteText {
                            Text = text,
                            Origin = Anchor.CentreLeft,
                            Anchor = Anchor.CentreLeft,
                        }
                    }
                }
            };
        }

        private Color4? accentColour;

        private TextAwesome chevron;

        protected override void FormatForeground(bool hover = false)
        {
            base.FormatForeground(hover);
            chevron.Alpha = hover ? 1 : 0;
        }

        public Color4 AccentColour
        {
            get { return accentColour.GetValueOrDefault(); }
            set
            {
                accentColour = value;
                BackgroundColourHover = BackgroundColourSelected = value;
                FormatBackground();
                FormatForeground();
            }
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            BackgroundColour = Color4.Transparent;
            BackgroundColourHover = accentColour ?? colours.PinkDarker;
            BackgroundColourSelected = Color4.Black.Opacity(0.5f);
        }
    }
}