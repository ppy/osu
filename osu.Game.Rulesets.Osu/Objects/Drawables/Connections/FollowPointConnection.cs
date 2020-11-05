// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Connections
{
    /// <summary>
    /// Visualises the <see cref="FollowPoint"/>s between two <see cref="DrawableOsuHitObject"/>s.
    /// </summary>
    public class FollowPointConnection : CompositeDrawable
    {
        // Todo: These shouldn't be constants
        private const int spacing = 32;
        private const double preempt = 800;

        public override bool RemoveWhenNotAlive => false;

        /// <summary>
        /// The start time of <see cref="Start"/>.
        /// </summary>
        public readonly Bindable<double> StartTime = new BindableDouble();

        /// <summary>
        /// The <see cref="DrawableOsuHitObject"/> which <see cref="FollowPoint"/>s will exit from.
        /// </summary>
        [NotNull]
        public readonly OsuHitObject Start;

        /// <summary>
        /// Creates a new <see cref="FollowPointConnection"/>.
        /// </summary>
        /// <param name="start">The <see cref="DrawableOsuHitObject"/> which <see cref="FollowPoint"/>s will exit from.</param>
        public FollowPointConnection([NotNull] OsuHitObject start)
        {
            Start = start;

            RelativeSizeAxes = Axes.Both;

            StartTime.BindTo(start.StartTimeBindable);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            bindEvents(Start);
        }

        private OsuHitObject end;

        /// <summary>
        /// The <see cref="DrawableOsuHitObject"/> which <see cref="FollowPoint"/>s will enter.
        /// </summary>
        [CanBeNull]
        public OsuHitObject End
        {
            get => end;
            set
            {
                end = value;

                if (end != null)
                    bindEvents(end);

                if (IsLoaded)
                    scheduleRefresh();
                else
                    refresh();
            }
        }

        private void bindEvents(OsuHitObject obj)
        {
            obj.PositionBindable.BindValueChanged(_ => scheduleRefresh());
            obj.DefaultsApplied += _ => scheduleRefresh();
        }

        private void scheduleRefresh()
        {
            Scheduler.AddOnce(refresh);
        }

        private void refresh()
        {
            double startTime = Start.GetEndTime();

            LifetimeStart = startTime;

            if (End == null || End.NewCombo || Start is Spinner || End is Spinner)
            {
                // ensure we always set a lifetime for full LifetimeManagementContainer benefits
                LifetimeEnd = LifetimeStart;
                return;
            }

            Vector2 startPosition = Start.StackedEndPosition;
            Vector2 endPosition = End.StackedPosition;
            double endTime = End.StartTime;

            Vector2 distanceVector = endPosition - startPosition;
            int distance = (int)distanceVector.Length;
            float rotation = (float)(Math.Atan2(distanceVector.Y, distanceVector.X) * (180 / Math.PI));
            double duration = endTime - startTime;

            double? firstTransformStartTime = null;
            double finalTransformEndTime = startTime;

            int point = 0;

            ClearInternal();

            for (int d = (int)(spacing * 1.5); d < distance - spacing; d += spacing)
            {
                float fraction = (float)d / distance;
                Vector2 pointStartPosition = startPosition + (fraction - 0.1f) * distanceVector;
                Vector2 pointEndPosition = startPosition + fraction * distanceVector;
                double fadeOutTime = startTime + fraction * duration;
                double fadeInTime = fadeOutTime - preempt;

                FollowPoint fp;

                AddInternal(fp = new FollowPoint());

                Debug.Assert(End != null);

                fp.Position = pointStartPosition;
                fp.Rotation = rotation;
                fp.Alpha = 0;
                fp.Scale = new Vector2(1.5f * End.Scale);

                firstTransformStartTime ??= fadeInTime;

                fp.AnimationStartTime = fadeInTime;

                using (fp.BeginAbsoluteSequence(fadeInTime))
                {
                    fp.FadeIn(End.TimeFadeIn);
                    fp.ScaleTo(End.Scale, End.TimeFadeIn, Easing.Out);
                    fp.MoveTo(pointEndPosition, End.TimeFadeIn, Easing.Out);
                    fp.Delay(fadeOutTime - fadeInTime).FadeOut(End.TimeFadeIn);

                    finalTransformEndTime = fadeOutTime + End.TimeFadeIn;
                }

                point++;
            }

            int excessPoints = InternalChildren.Count - point;
            for (int i = 0; i < excessPoints; i++)
                RemoveInternal(InternalChildren[^1]);

            // todo: use Expire() on FollowPoints and take lifetime from them when https://github.com/ppy/osu-framework/issues/3300 is fixed.
            LifetimeStart = firstTransformStartTime ?? startTime;
            LifetimeEnd = finalTransformEndTime;
        }
    }
}
