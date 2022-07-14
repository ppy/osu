// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Skinning.Legacy
{
    public class LegacyFollowCircle : FollowCircle
    {
        public LegacyFollowCircle(Drawable animationContent)
        {
            // follow circles are 2x the hitcircle resolution in legacy skins (since they are scaled down from >1x
            animationContent.Scale *= 0.5f;
            animationContent.Anchor = Anchor.Centre;
            animationContent.Origin = Anchor.Centre;

            RelativeSizeAxes = Axes.Both;
            InternalChild = animationContent;
        }

        protected override void OnTrackingChanged(ValueChangedEvent<bool> tracking)
        {
            Debug.Assert(ParentObject != null);

            if (ParentObject.Judged)
                return;

            const float scale_duration = 180f;
            const float fade_duration = 90f;

            double maxScaleDuration = ParentObject.HitStateUpdateTime - Time.Current;
            double realScaleDuration = scale_duration;
            if (tracking.NewValue && maxScaleDuration < realScaleDuration && maxScaleDuration >= 0)
                realScaleDuration = maxScaleDuration;
            double realFadeDuration = fade_duration * realScaleDuration / fade_duration;

            this.ScaleTo(tracking.NewValue ? DrawableSliderBall.FOLLOW_AREA : 1f, realScaleDuration, Easing.OutQuad)
                .FadeTo(tracking.NewValue ? 1f : 0f, realFadeDuration, Easing.OutQuad);
        }

        protected override void OnSliderEnd()
        {
            const float shrink_duration = 200f;
            const float fade_delay = 175f;
            const float fade_duration = 35f;

            this.ScaleTo(DrawableSliderBall.FOLLOW_AREA * 0.75f, shrink_duration, Easing.OutQuad)
                .Delay(fade_delay)
                .FadeOut(fade_duration);
        }
    }
}
