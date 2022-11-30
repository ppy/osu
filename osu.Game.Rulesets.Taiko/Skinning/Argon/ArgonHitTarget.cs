// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Taiko.Objects;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Taiko.Skinning.Argon
{
    public partial class ArgonHitTarget : CompositeDrawable
    {
        /// <summary>
        /// Thickness of all drawn line pieces.
        /// </summary>
        public ArgonHitTarget()
        {
            RelativeSizeAxes = Axes.Both;
            Masking = true;

            const float border_thickness = 4f;

            InternalChildren = new Drawable[]
            {
                new Circle
                {
                    Name = "Bar Upper",
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Y = -border_thickness,
                    RelativeSizeAxes = Axes.Y,
                    Size = new Vector2(border_thickness, (1 - TaikoStrongableHitObject.DEFAULT_STRONG_SIZE)),
                },
                new Circle
                {
                    Name = "Outer circle",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.White,
                    Blending = BlendingParameters.Additive,
                    Alpha = 0.1f,
                    Size = new Vector2(TaikoHitObject.DEFAULT_SIZE),
                    Masking = true,
                },
                new Circle
                {
                    Name = "Inner circle",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.White,
                    Blending = BlendingParameters.Additive,
                    Alpha = 0.1f,
                    Size = new Vector2(TaikoHitObject.DEFAULT_SIZE * 0.85f),
                    Masking = true,
                },
                new Circle
                {
                    Name = "Bar Lower",
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.Y,
                    Y = border_thickness,
                    Size = new Vector2(border_thickness, (1 - TaikoStrongableHitObject.DEFAULT_STRONG_SIZE)),
                },
            };
        }
    }
}
