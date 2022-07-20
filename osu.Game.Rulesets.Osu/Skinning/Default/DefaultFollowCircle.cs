// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Default
{
    public class DefaultFollowCircle : FollowCircle
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

        protected override void OnTrackingChanged(ValueChangedEvent<bool> tracking)
        {
            Debug.Assert(ParentObject != null);

            const float duration = 300f;

            if (ParentObject.Judged)
                return;

            if (tracking.NewValue)
            {
                if (Precision.AlmostEquals(0, Alpha))
                    this.ScaleTo(1);

                this.ScaleTo(DrawableSliderBall.FOLLOW_AREA, duration, Easing.OutQuint)
                    .FadeTo(1f, duration, Easing.OutQuint);
            }
            else
            {
                this.ScaleTo(DrawableSliderBall.FOLLOW_AREA * 1.2f, duration / 2, Easing.OutQuint)
                    .FadeTo(0, duration / 2, Easing.OutQuint);
            }
        }

        protected override void OnSliderEnd()
        {
            const float fade_duration = 300;

            // intentionally pile on an extra FadeOut to make it happen much faster
            this.ScaleTo(1, fade_duration, Easing.OutQuint);
            this.FadeOut(fade_duration / 2, Easing.OutQuint);
        }
    }
}
