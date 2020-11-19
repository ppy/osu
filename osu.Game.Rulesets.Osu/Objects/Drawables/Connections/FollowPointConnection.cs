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

        public FollowPointRenderer.FollowPointLifetimeEntry Entry;
        public DrawablePool<FollowPoint> Pool;

        protected override void FreeAfterUse()
        {
            base.FreeAfterUse();
            ClearInternal(false);
        }

        protected override void PrepareForUse()
        {
            base.PrepareForUse();

            OsuHitObject start = Entry.Start;
            OsuHitObject end = Entry.End;

            double startTime = start.GetEndTime();

            if (end == null || end.NewCombo || start is Spinner || end is Spinner)
                return;

            Vector2 startPosition = start.StackedEndPosition;
            Vector2 endPosition = end.StackedPosition;
            double endTime = end.StartTime;

            Vector2 distanceVector = endPosition - startPosition;
            int distance = (int)distanceVector.Length;
            float rotation = (float)(Math.Atan2(distanceVector.Y, distanceVector.X) * (180 / Math.PI));
            double duration = endTime - startTime;

            double finalTransformEndTime = startTime;

            for (int d = (int)(SPACING * 1.5); d < distance - SPACING; d += SPACING)
            {
                float fraction = (float)d / distance;
                Vector2 pointStartPosition = startPosition + (fraction - 0.1f) * distanceVector;
                Vector2 pointEndPosition = startPosition + fraction * distanceVector;
                double fadeOutTime = startTime + fraction * duration;
                double fadeInTime = fadeOutTime - PREEMPT;

                FollowPoint fp;

                AddInternal(fp = Pool.Get());

                fp.ClearTransforms();
                fp.Position = pointStartPosition;
                fp.Rotation = rotation;
                fp.Alpha = 0;
                fp.Scale = new Vector2(1.5f * end.Scale);

                fp.AnimationStartTime = fadeInTime;

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
    }
}
