// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Skinning
{
    public abstract partial class FollowCircle : CompositeDrawable
    {
        protected DrawableSlider? DrawableObject { get; private set; }

        protected FollowCircle()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject? hitObject)
        {
            DrawableObject = hitObject as DrawableSlider;

            DrawableObject?.Tracking.BindValueChanged(tracking =>
            {
                Debug.Assert(DrawableObject != null);

                if (DrawableObject.Judged)
                    return;

                using (BeginAbsoluteSequence(Math.Max(Time.Current, DrawableObject.HitObject?.StartTime ?? 0)))
                {
                    if (tracking.NewValue)
                        OnSliderPress();
                    else
                        OnSliderRelease();
                }
            }, true);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (DrawableObject != null)
            {
                DrawableObject.HitObjectApplied += onHitObjectApplied;
                onHitObjectApplied(DrawableObject);

                DrawableObject.ApplyCustomUpdateState += updateStateTransforms;
                updateStateTransforms(DrawableObject, DrawableObject.State.Value);
            }
        }

        private void onHitObjectApplied(DrawableHitObject drawableObject)
        {
            this.ScaleTo(1f)
                .FadeOut();
        }

        private void updateStateTransforms(DrawableHitObject d, ArmedState state)
        {
            Debug.Assert(DrawableObject != null);

            switch (state)
            {
                case ArmedState.Hit:
                    switch (d)
                    {
                        case DrawableSliderTail:
                            // Use DrawableObject instead of local object because slider tail's
                            // HitStateUpdateTime is ~36ms before the actual slider end (aka slider
                            // tail leniency)
                            using (BeginAbsoluteSequence(DrawableObject.HitStateUpdateTime))
                                OnSliderEnd();
                            break;

                        case DrawableSliderTick:
                        case DrawableSliderRepeat:
                            using (BeginAbsoluteSequence(d.HitStateUpdateTime))
                                OnSliderTick();
                            break;
                    }

                    break;

                case ArmedState.Miss:
                    switch (d)
                    {
                        case DrawableSliderTail:
                        case DrawableSliderTick:
                        case DrawableSliderRepeat:
                            // Despite above comment, ok to use d.HitStateUpdateTime
                            // here, since on stable, the break anim plays right when the tail is
                            // missed, not when the slider ends
                            using (BeginAbsoluteSequence(d.HitStateUpdateTime))
                                OnSliderBreak();
                            break;
                    }

                    break;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (DrawableObject != null)
            {
                DrawableObject.HitObjectApplied -= onHitObjectApplied;
                DrawableObject.ApplyCustomUpdateState -= updateStateTransforms;
            }
        }

        protected abstract void OnSliderPress();

        protected abstract void OnSliderRelease();

        protected abstract void OnSliderEnd();

        protected abstract void OnSliderTick();

        protected abstract void OnSliderBreak();
    }
}
