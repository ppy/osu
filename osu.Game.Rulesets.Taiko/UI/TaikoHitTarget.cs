// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osuTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.UI
{
    /// <summary>
    /// A component that is displayed at the hit position in the taiko playfield.
    /// </summary>
    internal class TaikoHitTarget : Container
    {
        /// <summary>
        /// Thickness of all drawn line pieces.
        /// </summary>
        private const float border_thickness = 2.5f;

        public TaikoHitTarget()
        {
            RelativeSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                new Box
                {
                    Name = "Bar Upper",
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.Y,
                    Size = new Vector2(border_thickness, (1 - TaikoStrongableHitObject.DEFAULT_STRONG_SIZE) / 2f),
                    Alpha = 0.1f
                },
                new CircularContainer
                {
                    Name = "Strong Hit Ring",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(TaikoStrongableHitObject.DEFAULT_STRONG_SIZE),
                    Masking = true,
                    BorderColour = Color4.White,
                    BorderThickness = border_thickness,
                    Alpha = 0.1f,
                    Children = new[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                            AlwaysPresent = true
                        }
                    }
                },
                new CircularContainer
                {
                    Name = "Normal Hit Ring",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(TaikoHitObject.DEFAULT_SIZE),
                    Masking = true,
                    BorderColour = Color4.White,
                    BorderThickness = border_thickness,
                    Alpha = 0.5f,
                    Children = new[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                            AlwaysPresent = true
                        }
                    }
                },
                new Box
                {
                    Name = "Bar Lower",
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.Y,
                    Size = new Vector2(border_thickness, (1 - TaikoStrongableHitObject.DEFAULT_STRONG_SIZE) / 2f),
                    Alpha = 0.1f
                },
            };
        }
    }
}
