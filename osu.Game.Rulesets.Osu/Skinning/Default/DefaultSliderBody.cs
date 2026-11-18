// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Osu.Objects;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Default
{
    public partial class DefaultSliderBody : SliderBody
    {
        protected override DrawableSliderPath CreateSliderPath() => new DefaultDrawableSliderPath();

        private partial class DefaultDrawableSliderPath : DrawableSliderPath
        {
            private const float opacity_at_centre = 0.3f;
            private const float opacity_at_edge = 0.8f;

            protected override Color4 ColourAt(float position)
            {
                const float actual_to_visual_radius = OsuHitObject.OBJECT_RADIUS / OsuHitObject.VISUAL_OBJECT_RADIUS;

                position = 1 - (1 - position) * actual_to_visual_radius;

                if (position < 0.0)
                    return Color4.Transparent;

                if (CalculatedBorderPortion != 0f && position <= CalculatedBorderPortion)
                    return BorderColour;

                position -= CalculatedBorderPortion;
                return new Color4(AccentColour.R, AccentColour.G, AccentColour.B, (opacity_at_edge - (opacity_at_edge - opacity_at_centre) * position / GRADIENT_PORTION) * AccentColour.A);
            }
        }
    }
}
