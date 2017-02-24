// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
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

            Children = new[]
            {
                new FlowContainer
                {
                    Direction = FlowDirections.Horizontal,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        new TextAwesome
                        {
                            Icon = FontAwesome.fa_chevron_right,
                            Colour = Color4.Black,
                            TextSize = 12,
                            Margin = new MarginPadding { Right = 3 },
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

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            BackgroundColour = Color4.Black.Opacity(0.5f);
            BackgroundColourHover = colours.PinkDarker;
            BackgroundColourSelected = Color4.Black.Opacity(0.5f);
        }
    }
}