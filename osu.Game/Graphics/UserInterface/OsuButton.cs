//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuButton : Button
    {
        public OsuButton()
        {
            Height = 40;
        }

        protected override SpriteText CreateText() => new OsuSpriteText
        {
            Depth = -1,
            Origin = Anchor.Centre,
            Anchor = Anchor.Centre,
            Font = @"Exo2.0-Bold",
        };

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