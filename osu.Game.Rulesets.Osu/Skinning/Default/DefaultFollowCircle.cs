// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Default
{
    public partial class DefaultFollowCircle : FollowCircle
    {
        public DefaultFollowCircle()
        {
            InternalChild = new CircularContainer
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                BorderThickness = 5,
                BorderColour = Color4.Orange,
                Blending = BlendingParameters.Additive,
                Child = new Box
                {
                    Colour = Color4.Orange,
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.2f,
                }
            };
        }

        protected override void OnSliderPress()
        {
            const float duration = 300f;

            if (Precision.AlmostEquals(0, Alpha))
                this.ScaleTo(1);

            this.ScaleTo(FollowAreaScale, duration, Easing.OutQuint)
                .FadeIn(duration, Easing.OutQuint);
        }

        protected override void OnSliderRelease()
        {
            const float duration = 150;

            this.ScaleTo(FollowAreaScale * 1.2f, duration, Easing.OutQuint)
                .FadeTo(0, duration, Easing.OutQuint);
        }

        protected override void OnSliderEnd()
        {
            const float duration = 300;

            this.ScaleTo(1, duration, Easing.OutQuint)
                .FadeOut(duration / 2, Easing.OutQuint);
        }

        protected override void OnSliderTick()
        {
            if (Scale.X >= FollowAreaScale * 0.98f)
            {
                this.ScaleTo(FollowAreaScale * 1.08f, 40, Easing.OutQuint)
                    .Then()
                    .ScaleTo(FollowAreaScale, 200f, Easing.OutQuint);
            }
        }

        protected override void OnSliderBreak()
        {
        }
    }
}
