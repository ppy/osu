// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Pieces
{
    public class SpinnerBackground : CircularContainer, IHasAccentColour
    {
        private readonly Box disc;

        public Color4 AccentColour
        {
            get => disc.Colour;
            set
            {
                disc.Colour = value;

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
                disc = new Box
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
