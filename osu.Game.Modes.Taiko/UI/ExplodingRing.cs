// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transforms;

namespace osu.Game.Modes.Taiko.UI
{
    class ExplodingRing : CircularContainer
    {
        private const float offset_min = -0.5f;
        private const float offset_max = 0.5f;

        public ExplodingRing(Color4 fillColour, bool fill)
        {
            Size = new Vector2(128);

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            RelativePositionAxes = Axes.Both;

            BorderColour = Color4.White;
            BorderThickness = 1;

            Alpha = 0.15f;

            Children = new[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,

                    Alpha = fill ? 1f : 0,
                    Colour = fillColour,

                    AlwaysPresent = true
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ScaleTo(5f, 500, EasingTypes.OutQuint);
            FadeOut(500);

            Expire();
        }
    }
}
