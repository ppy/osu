// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Skinning.Default;
using osu.Game.Skinning;
using osu.Game.Utils;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Legacy
{
    public partial class LegacySliderBody : PlaySliderBody
    {
        protected override DrawableSliderPath CreateSliderPath() => new LegacyDrawableSliderPath();

        protected override Color4 GetBodyAccentColour(ISkinSource skin, Color4 hitObjectAccentColour)
        {
            // legacy skins use a constant value for slider track alpha, regardless of the source colour.
            return base.GetBodyAccentColour(skin, hitObjectAccentColour).Opacity(0.7f);
        }

        private partial class LegacyDrawableSliderPath : DrawableSliderPath
        {
            protected override Color4 ColourAt(float position)
            {
                // https://github.com/peppy/osu-stable-reference/blob/3ea48705eb67172c430371dcfc8a16a002ed0d3d/osu!/Graphics/Renderers/MmSliderRendererGL.cs#L99
                const float aa_width = 0.5f / 64f;

                Color4 shadow = new Color4(0, 0, 0, 0.25f);
                Color4 outerColour = AccentColour.Darken(0.1f);
                Color4 innerColour = lighten(AccentColour, 0.5f);

                // https://github.com/peppy/osu-stable-reference/blob/3ea48705eb67172c430371dcfc8a16a002ed0d3d/osu!/Graphics/Renderers/MmSliderRendererGL.cs#L59-L70
                const float shadow_portion = 1 - (OsuLegacySkinTransformer.LEGACY_CIRCLE_RADIUS / OsuHitObject.OBJECT_RADIUS);
                const float border_portion = 0.1875f;

                if (position <= shadow_portion - aa_width)
                    return LegacyUtils.InterpolateNonLinear(position, Color4.Black.Opacity(0f), shadow, 0, shadow_portion - aa_width);

                if (position <= shadow_portion + aa_width)
                    return LegacyUtils.InterpolateNonLinear(position, shadow, BorderColour, shadow_portion - aa_width, shadow_portion + aa_width);

                if (position <= border_portion - aa_width)
                    return BorderColour;

                if (position <= border_portion + aa_width)
                    return LegacyUtils.InterpolateNonLinear(position, BorderColour, outerColour, border_portion - aa_width, border_portion + aa_width);

                return LegacyUtils.InterpolateNonLinear(position, outerColour, innerColour, border_portion + aa_width, 1);
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
