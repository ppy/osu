// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Pieces
{
    public class SpinnerBackground : CircularContainer, IHasAccentColour
    {
        public override bool HandleNonPositionalInput => false;
        public override bool HandlePositionalInput => false;

        protected Box Disc;

        public Color4 AccentColour
        {
            get
            {
                return Disc.Colour;
            }
            set
            {
                Disc.Colour = value;

                EdgeEffect = new EdgeEffectParameters
                {
                    Hollow = true,
                    Type = EdgeEffectType.Glow,
                    Radius = 40,
                    Colour = value,
                };
            }
        }

        public SpinnerBackground()
        {
            RelativeSizeAxes = Axes.Both;
            Masking = true;

            Children = new Drawable[]
            {
                Disc = new Box
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 1,
                },
            };
        }
    }
}
