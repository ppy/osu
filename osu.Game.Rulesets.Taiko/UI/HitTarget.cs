// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.UI
{
    /// <summary>
    /// A component that is displayed at the hit position in the taiko playfield.
    /// </summary>
    internal class HitTarget : Container
    {
        /// <summary>
        /// The 1px inner border of the taiko playfield.
        /// </summary>
        private const float border_offset = 1;

        /// <summary>
        /// Thickness of all drawn line pieces.
        /// </summary>
        private const float border_thickness = 2.5f;

        public HitTarget()
        {
            Size = new Vector2(TaikoPlayfield.DEFAULT_PLAYFIELD_HEIGHT);

            Children = new Drawable[]
            {
                new Box
                {
                    Name = "Bar Upper",
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Y = border_offset,
                    Size = new Vector2(border_thickness, (TaikoPlayfield.DEFAULT_PLAYFIELD_HEIGHT - TaikoHitObject.DEFAULT_STRONG_CIRCLE_DIAMETER) / 2f - border_offset),
                    Alpha = 0.1f
                },
                new CircularContainer
                {
                    Name = "Strong Hit Ring",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(TaikoHitObject.DEFAULT_STRONG_CIRCLE_DIAMETER),
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
                    Size = new Vector2(TaikoHitObject.DEFAULT_CIRCLE_DIAMETER),
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
                    Y = -border_offset,
                    Size = new Vector2(border_thickness, (TaikoPlayfield.DEFAULT_PLAYFIELD_HEIGHT - TaikoHitObject.DEFAULT_STRONG_CIRCLE_DIAMETER) / 2f - border_offset),
                    Alpha = 0.1f
                },
            };
        }
    }
}
