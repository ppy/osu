// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
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
            const float scale_duration = 300f;
            const float fade_duration = 300f;

            this.ScaleTo(tracking.NewValue ? DrawableSliderBall.FOLLOW_AREA : 1f, scale_duration, Easing.OutQuint)
                .FadeTo(tracking.NewValue ? 1f : 0, fade_duration, Easing.OutQuint);
        }

        protected override void OnSliderEnd()
        {
            const float fade_duration = 450f;

            // intentionally pile on an extra FadeOut to make it happen much faster
            this.FadeOut(fade_duration / 4, Easing.Out);
        }
    }
}
