// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using OpenTK.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Select.Filter;

namespace osu.Game.Screens.Select.Tab
{
    public class FilterTabDropDownMenuItem<T> : DropDownMenuItem<T>
    {
        public FilterTabDropDownMenuItem(string text, T value) : base(text, value)
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

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            BackgroundColour = Color4.Black.Opacity(0f);
            ForegroundColourHover = Color4.Black;
            ForegroundColourSelected = Color4.Black;

            if (typeof(T) == typeof(SortMode))
            {
                BackgroundColourHover = new Color4(163, 196, 36, 255);
                BackgroundColourSelected = new Color4(163, 196, 36, 255);
            }
            else
            {
                BackgroundColourHover = new Color4(124, 200, 253, 255);
                BackgroundColourSelected = new Color4(124, 200, 253, 255);
            }
        }
    }
}
