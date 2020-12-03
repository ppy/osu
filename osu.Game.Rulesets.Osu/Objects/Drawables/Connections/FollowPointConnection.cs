// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Connections
{
    /// <summary>
    /// Visualises the <see cref="FollowPoint"/>s between two <see cref="DrawableOsuHitObject"/>s.
    /// </summary>
    public class FollowPointConnection : PoolableDrawable
    {
        // Todo: These shouldn't be constants
        public const int SPACING = 32;
        public const double PREEMPT = 800;

        public FollowPointLifetimeEntry Entry;
        public DrawablePool<FollowPoint> Pool;

        protected override void PrepareForUse()
        {
            base.PrepareForUse();

            Entry.Invalidated += onEntryInvalidated;

            refreshPoints();
        }

        protected override void FreeAfterUse()
        {
            base.FreeAfterUse();

            Entry.Invalidated -= onEntryInvalidated;

            // Return points to the pool.
            ClearInternal(false);

            Entry = null;
        }

        private void onEntryInvalidated() => refreshPoints();

        private void refreshPoints()
        {
            ClearInternal(false);

            OsuHitObject start = Entry.Start;
            OsuHitObject end = Entry.End;

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

                GetFadeTimes(start, end, (float)d / distance, out var fadeInTime, out var fadeOutTime);

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
                    fp.Delay(fadeOutTime - fadeInTime).FadeOut(end.TimeFadeIn);

                    finalTransformEndTime = fadeOutTime + end.TimeFadeIn;
                }
            }

            // todo: use Expire() on FollowPoints and take lifetime from them when https://github.com/ppy/osu-framework/issues/3300 is fixed.
            Entry.LifetimeEnd = finalTransformEndTime;
        }

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

            fadeOutTime = startTime + fraction * duration;
            fadeInTime = fadeOutTime - PREEMPT;
        }
    }
}
