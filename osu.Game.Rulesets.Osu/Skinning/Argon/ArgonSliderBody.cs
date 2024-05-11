// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Rulesets.Osu.Skinning.Default;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Argon
{
    public partial class ArgonSliderBody : PlaySliderBody
    {
        protected override void LoadComplete()
        {
            const float path_radius = ArgonMainCirclePiece.OUTER_GRADIENT_SIZE / 2;

            base.LoadComplete();

            AccentColourBindable.BindValueChanged(accent => BorderColour = accent.NewValue, true);
            ScaleBindable.BindValueChanged(scale => PathRadius = path_radius * scale.NewValue, true);

            // This border size thing is kind of weird, hey.
            const float intended_thickness = ArgonMainCirclePiece.GRADIENT_THICKNESS / path_radius;

            BorderSize = intended_thickness / Default.DrawableSliderPath.BORDER_PORTION;
        }

        protected override Default.DrawableSliderPath CreateSliderPath() => new DrawableSliderPath();

        private partial class DrawableSliderPath : Default.DrawableSliderPath
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
