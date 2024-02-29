// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Skinning
{
    public abstract partial class FollowCircle : CompositeDrawable
    {
        [Resolved]
        protected DrawableHitObject? ParentObject { get; private set; }

        protected FollowCircle()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            ((DrawableSlider?)ParentObject)?.Tracking.BindValueChanged(tracking => onTrackingChange(tracking), true);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (ParentObject != null)
            {
                ParentObject.HitObjectApplied += onHitObjectApplied;
                onHitObjectApplied(ParentObject);

                ParentObject.ApplyCustomUpdateState += updateStateTransforms;
                updateStateTransforms(ParentObject, ParentObject.State.Value);
            }
        }

        private void onHitObjectApplied(DrawableHitObject drawableObject)
        {
            this.ScaleTo(1f)
                .FadeOut();
        }

        private void onTrackingChange(ValueChangedEvent<bool> tracking)
        {
            Debug.Assert(ParentObject != null);

            if (ParentObject.Judged)
                return;

            if (ParentObject.HitObject?.StartTime is null)
                return;

            using (BeginAbsoluteSequence(Math.Max(Time.Current, ParentObject.HitObject?.StartTime ?? 0)))
            {
                if (tracking.NewValue)
                    OnSliderPress();
                else
                    OnSliderRelease();

                // calling one of the above after OnSliderEnd() would result in a lack of a slider end animation
                // thus, ensure OnSliderEnd() is called again if the tracking value changed after the first call
                if (Time.Current >= ParentObject.HitStateUpdateTime + SliderEventGenerator.TAIL_LENIENCY)
                {
                    FinishTransforms();
                    OnSliderEnd();
                }
            }
        }

        private void updateStateTransforms(DrawableHitObject drawableObject, ArmedState state)
        {
            Debug.Assert(ParentObject != null);

            switch (state)
            {
                case ArmedState.Hit:
                    switch (drawableObject)
                    {
                        case DrawableSliderTail:
                            using (BeginAbsoluteSequence(drawableObject.HitStateUpdateTime))
                                OnSliderEnd();
                            break;

                        case DrawableSliderTick:
                        case DrawableSliderRepeat:
                            using (BeginAbsoluteSequence(drawableObject.HitStateUpdateTime))
                                OnSliderTick();
                            break;
                    }

                    break;

                case ArmedState.Miss:
                    switch (drawableObject)
                    {
                        case DrawableSliderTail:
                            // Use ParentObject instead of drawableObject because slider tail's
                            // HitStateUpdateTime is ~36ms before the actual slider end (aka slider
                            // tail leniency)
                            using (BeginAbsoluteSequence(ParentObject.HitStateUpdateTime))
                                OnSliderTailBreak();
                            break;
                        case DrawableSliderTick:
                        case DrawableSliderRepeat:
                            using (BeginAbsoluteSequence(drawableObject.HitStateUpdateTime))
                                OnSliderBreak();
                            break;
                    }

                    break;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (ParentObject != null)
            {
                ParentObject.HitObjectApplied -= onHitObjectApplied;
                ParentObject.ApplyCustomUpdateState -= updateStateTransforms;
            }
        }

        protected abstract void OnSliderPress();

        protected abstract void OnSliderRelease();

        protected abstract void OnSliderEnd();

        protected abstract void OnSliderTick();

        protected abstract void OnSliderBreak();

        protected virtual void OnSliderTailBreak() => this.FadeTo(0f);
    }
}
