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

        private readonly List<(double time, Action transform)> transformQueue = new List<(double, Action)>();

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
                    // `queueTransformsFromState` in such a situation.
                    if ((Clock as IGameplayClock)?.IsRewinding == true)
                        return;

                    if (tracking.NewValue)
                        transformQueue.Add((clampSliderHeadTime(Time.Current), OnSliderPress));
                    else
                        transformQueue.Add((Time.Current, OnSliderRelease));

                    applyQueuedTransforms();
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

            transformQueue.Clear();

            // Immediately play out any pending transforms from press/release
            FinishTransforms(true);

            repopulateTransformsFromState();
            applyQueuedTransforms();
        }

        private void queueStateTransforms(DrawableHitObject d, ArmedState state)
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
                            transformQueue.Add((DrawableObject.HitStateUpdateTime, OnSliderEnd));
                            break;

                        case DrawableSliderTick:
                        case DrawableSliderRepeat:
                            transformQueue.Add((d.HitStateUpdateTime, OnSliderTick));
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
                            transformQueue.Add((d.HitStateUpdateTime, OnSliderBreak));
                            break;
                    }

                    break;
            }
        }

        private void updateStateTransforms(DrawableHitObject d, ArmedState state)
        {
            queueStateTransforms(d, state);
            applyQueuedTransforms();
        }

        private void applyQueuedTransforms()
        {
            foreach (var transform in transformQueue.OrderBy(t => t.time))
            {
                using (BeginAbsoluteSequence(transform.time))
                    transform.transform.Invoke();
            }

            transformQueue.Clear();
        }

        private void repopulateTransformsFromState()
        {
            Debug.Assert(DrawableObject != null);

            foreach (var trackingChange in DrawableObject.Result.TrackingHistory)
            {
                // This avoids some events from affecting the transforms which wouldn't be considered in regular gameplay:
                // * (-inf, false) update that's always pushed onto a new tracking updates stack (see `OsuSliderJudgementResult.TrackingHistory)
                // * (..., false) update triggered right after hitting the tail circle (ignored by the `if (DrawableObject.Judged)` check in `load()`
                if (trackingChange.time <= DrawableObject.LifetimeStart || trackingChange.time >= DrawableObject.HitObject.GetEndTime())
                    continue;

                if (trackingChange.tracking)
                    transformQueue.Add((clampSliderHeadTime(trackingChange.time), OnSliderPress));
                else
                    transformQueue.Add((trackingChange.time, OnSliderRelease));
            }

            foreach (var nested in DrawableObject.NestedHitObjects.Where(d => d is DrawableSliderTick or DrawableSliderRepeat))
            {
                queueStateTransforms(nested, nested.State.Value);
            }

            queueStateTransforms(DrawableObject.TailCircle, DrawableObject.TailCircle.State.Value);
        }

        /// <summary>
        /// Clamps provided time to the start time of the slider head circle.
        /// This prevents the slider press transform from starting before the start time
        /// if it was hit early.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private double clampSliderHeadTime(double time) => Math.Max(time, DrawableObject?.HitObject?.StartTime ?? 0);

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
