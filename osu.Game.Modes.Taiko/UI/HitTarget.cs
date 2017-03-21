// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Modes.Taiko.Objects;

namespace osu.Game.Modes.Taiko.UI
{
    internal class HitTarget : Container
    {
        private const float normal_diameter = TaikoHitObject.CIRCLE_RADIUS * 2 * TaikoPlayfield.PLAYFIELD_SCALE;
        private const float finisher_diameter = normal_diameter * 1.5f;
        private const float border_offset = 1;

        public HitTarget()
        {
            RelativeSizeAxes = Axes.Y;

            Children = new Drawable[]
            {
                new Box
                {
                    Name = "Bar Upper",

                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,

                    Y = border_offset,

                    Size = new Vector2(3, (TaikoPlayfield.PlayfieldHeight - finisher_diameter) / 2f - border_offset),

                    Alpha = 0.1f
                },
                new CircularContainer
                {
                    Name = "Finisher Ring",

                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,

                    Size = new Vector2(finisher_diameter),

                    BorderColour = Color4.White,
                    BorderThickness = 2,
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
                    Name = "Normal Ring",

                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,

                    Size = new Vector2(normal_diameter),

                    BorderColour = Color4.White,
                    BorderThickness = 2,
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

                    Size = new Vector2(3, (TaikoPlayfield.PlayfieldHeight - finisher_diameter) / 2f - border_offset),

                    Alpha = 0.1f
                },
            };
        }
    }
}
