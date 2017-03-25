// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Screens.Select
{
    class DetailsBar : Container
    {
        private Box background;
        private Box bar;

        public float Value
        {
            get
            {
                return bar.Width;
            }
            set
            {
                bar.ResizeTo(new Vector2(value, 1), 200);
            }
        }

        public SRGBColour BackgroundColour
        {
            get
            {
                return background.Colour;
            }
            set
            {
                background.Colour = value;
            }
        }

        public SRGBColour BarColour
        {
            get
            {
                return bar.Colour;
            }
            set
            {
                bar.Colour = value;
            }
        }

        public DetailsBar()
        {
            Children = new []
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                bar = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                }
            };
        }
    }
}
