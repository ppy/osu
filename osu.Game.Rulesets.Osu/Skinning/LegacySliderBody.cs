// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.MathUtils;
using osu.Game.Rulesets.Osu.Objects.Drawables.Pieces;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning
{
    public class LegacySliderBody : SnakingSliderBody
    {
        protected override DrawableSliderPath CreateSliderPath() => new LegacyDrawableSliderPath();

        private class LegacyDrawableSliderPath : DrawableSliderPath
        {
            public new Color4 AccentColour => new Color4(base.AccentColour.R, base.AccentColour.G, base.AccentColour.B, 0.70f);

            protected override Color4 ColourAt(float position)
            {
                if (CalculatedBorderPortion != 0f && position <= CalculatedBorderPortion)
                    return BorderColour;

                position -= BORDER_PORTION;

                Color4 outerColour = AccentColour.Darken(0.1f);
                Color4 innerColour = lighten(AccentColour, 0.5f);

                return Interpolation.ValueAt(position / GRADIENT_PORTION, outerColour, innerColour, 0, 1);
            }

            /// <summary>
            /// Lightens a colour in a way more friendly to dark or strong colours.
            /// </summary>
            private static Color4 lighten(Color4 color, float amount)
            {
                amount *= 0.5f;
                return new Color4(
                    Math.Min(1, color.R * (1 + 0.5f * amount) + 1 * amount),
                    Math.Min(1, color.G * (1 + 0.5f * amount) + 1 * amount),
                    Math.Min(1, color.B * (1 + 0.5f * amount) + 1 * amount),
                    color.A);
            }
        }
    }
}
