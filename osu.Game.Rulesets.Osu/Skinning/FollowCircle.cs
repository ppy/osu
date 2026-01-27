// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.Osu.Skinning
{
    public abstract partial class FollowCircle : CompositeDrawable
    {
        protected DrawableSlider? DrawableObject { get; private set; }

        private readonly IBindable<bool> tracking = new Bindable<bool>();

        protected FollowCircle()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject? hitObject)
        {
            DrawableObject = hitObject as DrawableSlider;

            if (DrawableObject != null)
            {
                tracking.BindTo(DrawableObject.Tracking);
                tracking.BindValueChanged(tracking =>
                {
                    if (DrawableObject.Judged)
                        return;

                    // Don't run this when rewinding, the transforms will be handled by
                    // `applyTransformsWhenRewinding` in such a situation.
                    if ((Clock as IGameplayClock)?.IsRewinding == true)
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
            // Sane defaults when a new hitobject is applied to the drawable slider.
            this.ScaleTo(1f)
                .FadeOut();

            // Immediately play out any pending transforms from press/release
            FinishTransforms(true);

            // The appropriate slider transforms will not be applied if the replay is being
            // played backwards. We apply them here if the follow circle is being instantiated
            // in such a situation.
            if ((Clock as IGameplayClock)?.IsRewinding == true && Time.Current > drawableObject.HitObject.GetEndTime())
                applyTransformsWhenRewinding();
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

        private void applyTransformsWhenRewinding()
        {
            Debug.Assert(DrawableObject != null);

            var trackingHistory = new Queue<(double time, bool tracking)>(DrawableObject.Result.TrackingHistory.Reverse());
            var nested = new Queue<DrawableHitObject>(DrawableObject.NestedHitObjects.Where(dho => dho is DrawableSliderTick or DrawableSliderRepeat));

            // Applying the transforms in the same order as they would have been applied
            // during regular gameplay.
            // - tracking update
            // - any hit objects that have been hit after it
            // - next tracking update
            // - and so on
            while (trackingHistory.Count > 0)
            {
                var history = trackingHistory.Dequeue();

                if (history.time < DrawableObject.LifetimeStart || history.time > DrawableObject.LifetimeEnd)
                    continue;

                using (BeginAbsoluteSequence(Math.Max(history.time, DrawableObject.HitObject?.StartTime ?? 0)))
                {
                    if (history.tracking)
                        OnSliderPress();
                    else
                        OnSliderRelease();
                }

                while (
                    nested.Count > 0
                    && (trackingHistory.Count == 0 || nested.Peek().Result.TimeAbsolute <= trackingHistory.Peek().time))
                {
                    var dho = nested.Dequeue();
                    updateStateTransforms(dho, dho.IsHit ? ArmedState.Hit : ArmedState.Miss);
                }
            }

            updateStateTransforms(DrawableObject.TailCircle, DrawableObject.TailCircle.IsHit ? ArmedState.Hit : ArmedState.Miss);
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
