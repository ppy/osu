// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Utils;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Skinning.Default;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Legacy
{
    public class LegacySliderBody : PlaySliderBody
    {
        protected override DrawableSliderPath CreateSliderPath() => new LegacyDrawableSliderPath();

        private class LegacyDrawableSliderPath : DrawableSliderPath
        {
            private const float shadow_portion = 1 - (OsuLegacySkinTransformer.LEGACY_CIRCLE_RADIUS / OsuHitObject.OBJECT_RADIUS);

            protected new float CalculatedBorderPortion
                // Roughly matches osu!stable's slider border portions.
                => base.CalculatedBorderPortion * 0.77f;

            public new Color4 AccentColour => new Color4(base.AccentColour.R, base.AccentColour.G, base.AccentColour.B, base.AccentColour.A * 0.70f);

            protected override Color4 ColourAt(float position)
            {
                float realBorderPortion = shadow_portion + CalculatedBorderPortion;
                float realGradientPortion = 1 - realBorderPortion;

                if (position <= shadow_portion)
                    return new Color4(0f, 0f, 0f, 0.25f * position / shadow_portion);

                if (position <= realBorderPortion)
                    return BorderColour;

                position -= realBorderPortion;

                Color4 outerColour = AccentColour.Darken(0.1f);
                Color4 innerColour = lighten(AccentColour, 0.5f);

                return Interpolation.ValueAt(position / realGradientPortion, outerColour, innerColour, 0, 1);
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
