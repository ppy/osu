// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Default
{
    public class SpinnerFill : CircularContainer, IHasAccentColour
    {
        public readonly Box Disc;

        public Color4 AccentColour
        {
            get => Disc.Colour;
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

        public SpinnerFill()
        {
            RelativeSizeAxes = Axes.Both;
            Masking = true;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

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
