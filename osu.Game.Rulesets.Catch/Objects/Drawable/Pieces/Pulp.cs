// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Catch.Objects.Drawable.Pieces
{
    public class Pulp : Circle, IHasAccentColour
    {
        public Pulp()
        {
            RelativePositionAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Blending = BlendingMode.Additive;
            Colour = Color4.White.Opacity(0.9f);
        }

        private Color4 accentColour;
        public Color4 AccentColour
        {
            get { return accentColour; }
            set
            {
                accentColour = value;

                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Glow,
                    Radius = 8,
                    Colour = accentColour.Darken(0.2f).Opacity(0.75f)
                };
            }
        }
    }
}
