// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Timing;

namespace osu.Game.Rulesets.Mania.Timing
{
    /// <summary>
    /// A <see cref="ScrollingContainer"/> that emulates a form of gravity where hit objects speed up over time.
    /// </summary>
    internal class GravityScrollingContainer : ScrollingContainer
    {
        private readonly MultiplierControlPoint controlPoint;

        public GravityScrollingContainer(MultiplierControlPoint controlPoint)
        {
            this.controlPoint = controlPoint;
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            // The gravity-adjusted start position
            float startPos = (float)computeGravityTime(controlPoint.StartTime);
            // The gravity-adjusted end position
            float endPos = (float)computeGravityTime(controlPoint.StartTime + RelativeChildSize.Y);

            Y = startPos;
            Height = endPos - startPos;
        }

        /// <summary>
        /// Applies gravity to a time value based on the current time.
        /// </summary>
        /// <param name="time">The time value gravity should be applied to.</param>
        /// <returns>The time after gravity is applied to <paramref name="time"/>.</returns>
        private double computeGravityTime(double time)
        {
            double relativeTime = relativeTimeAt(time);

            // The sign of the relative time, this is used to apply backwards acceleration leading into startTime
            double sign = relativeTime < 0 ? -1 : 1;

            return VisibleTimeRange - acceleration * relativeTime * relativeTime * sign;
        }

        /// <summary>
        /// The acceleration due to "gravity" of the content of this container.
        /// </summary>
        private double acceleration => 1 / VisibleTimeRange;

        /// <summary>
        /// Computes the current time relative to <paramref name="time"/>, accounting for <see cref="ScrollingContainer.VisibleTimeRange"/>.
        /// </summary>
        /// <param name="time">The non-offset time.</param>
        /// <returns>The current time relative to <paramref name="time"/> - <see cref="ScrollingContainer.VisibleTimeRange"/>. </returns>
        private double relativeTimeAt(double time) => Time.Current - time + VisibleTimeRange;
    }
}
