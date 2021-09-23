// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Framework.Logging;

namespace osu.Game.Rulesets.Osu.Skinning.Legacy
{
    public class LegacyFollowCircle : CompositeDrawable
    {
        private readonly Drawable animationContent;
        private DrawableSlider slider;

        public LegacyFollowCircle(Drawable animationContent)
        {
            this.animationContent = animationContent;
        }

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject drawableObject)
        {
            slider = (DrawableSlider)drawableObject;

            RelativeSizeAxes = Axes.Both;

            InternalChild = animationContent;
            animationContent.Anchor = Anchor.Centre;
            animationContent.Origin = Anchor.Centre;

            slider.Tracking.BindValueChanged(trackingChanged, true);
            slider.ApplyCustomUpdateState += updateStateTransforms;
        }

        private void trackingChanged(ValueChangedEvent<bool> e)
        {
            if (slider.Judged)
                return;

            bool tracking = e.NewValue;

            if (slider.Ball.InputTracksVisualSize)
            {
                if (tracking)
                    this.ScaleTo(DrawableSliderBall.FOLLOW_AREA, 200, Easing.OutQuint);
                else
                    this.ScaleTo(DrawableSliderBall.FOLLOW_AREA * 2, 100, Easing.OutQuint).Then().ScaleTo(1f);
            }
            else
            {
                // We need to always be tracking the final size, at both endpoints. For now, this is achieved by removing the scale duration.
                this.ScaleTo(tracking ? DrawableSliderBall.FOLLOW_AREA : 1f);
            }

            this.FadeTo(tracking ? 1f : 0f, 100, Easing.OutQuint);
        }

        private void updateStateTransforms(DrawableHitObject obj, ArmedState state)
        {
            if (!(obj is DrawableSlider))
                return;

            const float fade_out_time = 200f;

            using (BeginAbsoluteSequence(slider.HitObject.EndTime))
            {
                this.FadeOut(fade_out_time, Easing.InQuint);
                this.ScaleTo(DrawableSliderBall.FOLLOW_AREA * 0.8f, fade_out_time, Easing.None);
            }
        }
    }
}
