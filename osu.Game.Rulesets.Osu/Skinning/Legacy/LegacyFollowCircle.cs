// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Skinning.Legacy
{
    public class LegacyFollowCircle : CompositeDrawable
    {
        [Resolved(canBeNull: true)]
        private DrawableHitObject? parentObject { get; set; }

        public LegacyFollowCircle(Drawable animationContent)
        {
            // follow circles are 2x the hitcircle resolution in legacy skins (since they are scaled down from >1x
            animationContent.Scale *= 0.5f;
            animationContent.Anchor = Anchor.Centre;
            animationContent.Origin = Anchor.Centre;

            RelativeSizeAxes = Axes.Both;
            InternalChild = animationContent;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (parentObject != null)
            {
                var slider = (DrawableSlider)parentObject;
                slider.Tracking.BindValueChanged(trackingChanged, true);
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (parentObject != null)
            {
                parentObject.HitObjectApplied += onHitObjectApplied;
                onHitObjectApplied(parentObject);

                parentObject.ApplyCustomUpdateState += updateStateTransforms;
                updateStateTransforms(parentObject, parentObject.State.Value);
            }
        }

        private void trackingChanged(ValueChangedEvent<bool> tracking)
        {
            Debug.Assert(parentObject != null);

            if (parentObject.Judged)
                return;

            const float scale_duration = 180f;
            const float fade_duration = 90f;

            double maxScaleDuration = parentObject.HitStateUpdateTime - Time.Current;
            double realScaleDuration = scale_duration;
            if (tracking.NewValue && maxScaleDuration < realScaleDuration && maxScaleDuration >= 0)
                realScaleDuration = maxScaleDuration;
            double realFadeDuration = fade_duration * realScaleDuration / fade_duration;

            this.ScaleTo(tracking.NewValue ? DrawableSliderBall.FOLLOW_AREA : 1f, realScaleDuration, Easing.OutQuad)
                .FadeTo(tracking.NewValue ? 1f : 0f, realFadeDuration, Easing.OutQuad);
        }

        private void onHitObjectApplied(DrawableHitObject drawableObject)
        {
            this.ScaleTo(1f)
                .FadeOut();
        }

        private void updateStateTransforms(DrawableHitObject drawableObject, ArmedState state)
        {
            // see comment in LegacySliderBall.updateStateTransforms
            if (drawableObject is not DrawableSlider)
                return;

            const float shrink_duration = 200f;
            const float fade_delay = 175f;
            const float fade_duration = 35f;

            using (BeginAbsoluteSequence(drawableObject.HitStateUpdateTime))
            {
                this.ScaleTo(DrawableSliderBall.FOLLOW_AREA * 0.75f, shrink_duration, Easing.OutQuad)
                    .Delay(fade_delay)
                    .FadeOut(fade_duration);
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (parentObject != null)
            {
                parentObject.HitObjectApplied -= onHitObjectApplied;
                parentObject.ApplyCustomUpdateState -= updateStateTransforms;
            }
        }
    }
}
