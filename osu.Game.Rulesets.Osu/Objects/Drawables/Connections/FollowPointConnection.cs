// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Pooling;
using osuTK;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Connections
{
    /// <summary>
    /// Visualises the <see cref="FollowPoint"/>s between two <see cref="DrawableOsuHitObject"/>s.
    /// </summary>
    public partial class FollowPointConnection : PoolableDrawableWithLifetime<FollowPointLifetimeEntry>
    {
        // Todo: These shouldn't be constants
        public const int SPACING = 32;
        public const double PREEMPT = 800;

        public DrawablePool<FollowPoint>? Pool { private get; set; }

        protected override void OnApply(FollowPointLifetimeEntry entry)
        {
            base.OnApply(entry);

            entry.Invalidated += scheduleRefresh;

            // Our clock may not be correct at this point if `LoadComplete` has not run yet.
            // Without a schedule, animations referencing FollowPoint's clock (see `IAnimationTimeReference`) would be incorrect on first pool usage.
            scheduleRefresh();
        }

        protected override void OnFree(FollowPointLifetimeEntry entry)
        {
            base.OnFree(entry);

            entry.Invalidated -= scheduleRefresh;
            // Return points to the pool.
            ClearInternal(false);
        }

        private void scheduleRefresh() => Scheduler.AddOnce(() =>
        {
            Debug.Assert(Pool != null);

            ClearInternal(false);

            var entry = Entry;

            if (entry?.End == null) return;

            OsuHitObject start = entry.Start;
            OsuHitObject end = entry.End;

            double startTime = start.GetEndTime();

            Vector2 startPosition = start.StackedEndPosition;
            Vector2 endPosition = end.StackedPosition;

            Vector2 distanceVector = endPosition - startPosition;
            int distance = (int)distanceVector.Length;
            float rotation = (float)(Math.Atan2(distanceVector.Y, distanceVector.X) * (180 / Math.PI));

            double finalTransformEndTime = startTime;

            for (int d = (int)(SPACING * 1.5); d < distance - SPACING; d += SPACING)
            {
                float fraction = (float)d / distance;
                Vector2 pointStartPosition = startPosition + (fraction - 0.1f) * distanceVector;
                Vector2 pointEndPosition = startPosition + fraction * distanceVector;

                GetFadeTimes(start, end, (float)d / distance, out double fadeInTime, out double fadeOutTime);

                FollowPoint fp;

                AddInternal(fp = Pool.Get());

                fp.ClearTransforms();
                fp.Position = pointStartPosition;
                fp.Rotation = rotation;
                fp.Alpha = 0;
                fp.Scale = new Vector2(1.5f * end.Scale);

                fp.AnimationStartTime.Value = fadeInTime;

                using (fp.BeginAbsoluteSequence(fadeInTime))
                {
                    fp.FadeIn(end.TimeFadeIn);
                    fp.ScaleTo(end.Scale, end.TimeFadeIn, Easing.Out);
                    fp.MoveTo(pointEndPosition, end.TimeFadeIn, Easing.Out);
                    fp.Delay(fadeOutTime - fadeInTime).FadeOut(end.TimeFadeIn).Expire();

                    finalTransformEndTime = fp.LifetimeEnd;
                }
            }

            entry.LifetimeEnd = finalTransformEndTime;
        });

        /// <summary>
        /// Computes the fade time of follow point positioned between two hitobjects.
        /// </summary>
        /// <param name="start">The first <see cref="OsuHitObject"/>, where follow points should originate from.</param>
        /// <param name="end">The second <see cref="OsuHitObject"/>, which follow points should target.</param>
        /// <param name="fraction">The fractional distance along <paramref name="start"/> and <paramref name="end"/> at which the follow point is to be located.</param>
        /// <param name="fadeInTime">The fade-in time of the follow point/</param>
        /// <param name="fadeOutTime">The fade-out time of the follow point.</param>
        public static void GetFadeTimes(OsuHitObject start, OsuHitObject end, float fraction, out double fadeInTime, out double fadeOutTime)
        {
            double startTime = start.GetEndTime();
            double duration = end.StartTime - startTime;

            // Preempt time can go below 800ms. Normally, this is achieved via the DT mod which uniformly speeds up all animations game wide regardless of AR.
            // This uniform speedup is hard to match 1:1, however we can at least make AR>10 (via mods) feel good by extending the upper linear preempt function (see: OsuHitObject).
            // Note that this doesn't exactly match the AR>10 visuals as they're classically known, but it feels good.
            double preempt = PREEMPT * Math.Min(1, start.TimePreempt / OsuHitObject.PREEMPT_MIN);

            fadeOutTime = startTime + fraction * duration;
            fadeInTime = fadeOutTime - preempt;
        }
    }
}
