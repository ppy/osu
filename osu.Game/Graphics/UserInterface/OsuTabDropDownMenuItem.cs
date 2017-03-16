// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.UserInterface;
using OpenTK.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuTabDropDownMenuItem<T> : DropDownMenuItem<T>
    {
        public OsuTabDropDownMenuItem(string text, T value) : base(text, value)
        {
            Foreground.Padding = new MarginPadding { Top = 4, Bottom = 4 };
            Foreground.Margin = new MarginPadding { Left = 7 };

            Masking = true;
            CornerRadius = 6;
            Foreground.Add(new OsuSpriteText
            {
                Text = text,
                Origin = Anchor.CentreLeft,
                Anchor = Anchor.CentreLeft,
            });
        }

        private Color4? accentColour;
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
            BackgroundColour = Color4.Black.Opacity(0f);
            ForegroundColourHover = Color4.Black;
            ForegroundColourSelected = Color4.Black;
            if (accentColour == null)
                AccentColour = colours.Blue;
        }
    }
}
