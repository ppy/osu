// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using OpenTK.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Select.Tab
{
    public class FilterTabDropDownMenuItem<T> : DropDownMenuItem<T>
    {
        public FilterTabDropDownMenuItem(string text, T value) : base(text, value)
        {
            Foreground.Padding = new MarginPadding(5);
            Background.Colour = Color4.Red;
            Foreground.Margin = new MarginPadding { Left = 7 };
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
            BackgroundColour = Color4.Black.Opacity(0.8f);
            BackgroundColourHover = new Color4(124, 200, 253, 255);
            BackgroundColourSelected = new Color4(124, 200, 253, 255);
            //BackgroundColourSelected = new Color4(163, 196, 36, 255); // Green
        }
    }
}
