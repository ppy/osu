//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Overlays;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuButton : Button
    {
        public OsuButton()
        {
            Height = 40;
            SpriteText.TextSize = OptionsOverlay.FONT_SIZE;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = colours.BlueDark;
            Masking = true;
            CornerRadius = 5;

            Add(new Triangles
            {
                RelativeSizeAxes = Axes.Both,
                ColourDark = colours.BlueDarker,
                ColourLight = colours.Blue,
            });
        }
    }
}