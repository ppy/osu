// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Rulesets.Osu.Skinning.Default;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Argon
{
    public class ArgonSliderBody : PlaySliderBody
    {
        protected override void LoadComplete()
        {
            base.LoadComplete();

            AccentColourBindable.BindValueChanged(accent =>
            {
                BorderColour = accent.NewValue;
            }, true);
            ScaleBindable.BindValueChanged(scale => PathRadius = ArgonMainCirclePiece.OUTER_GRADIENT_SIZE / 2 * scale.NewValue, true);

            BorderSize = ArgonMainCirclePiece.BORDER_THICKNESS / 4;
        }

        protected override Default.DrawableSliderPath CreateSliderPath() => new DrawableSliderPath();

        private class DrawableSliderPath : Default.DrawableSliderPath
        {
            protected override Color4 ColourAt(float position)
            {
                if (CalculatedBorderPortion != 0f && position <= CalculatedBorderPortion)
                    return BorderColour;

                return AccentColour.Darken(4);
            }
        }
    }
}
