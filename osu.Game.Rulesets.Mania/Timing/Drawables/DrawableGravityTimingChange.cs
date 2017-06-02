using System;
using osu.Framework.Graphics;
using osu.Framework.MathUtils;
using osu.Framework.Physics;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Mania.Timing.Drawables
{
    public class DrawableGravityTimingChange : DrawableTimingChange
    {
        // Amount of time to travel this container such that
        // at time = 0, gravityTime = timeSpan and
        // at time = travel_time, gravityTime = 0
        // Where gravityTime is used as the position of the content
        private const double travel_time = 1000;

        public DrawableGravityTimingChange(TimingChange timingChange)
            : base(timingChange)
        {
        }

        protected override void Update()
        {
            base.Update();

            // The gravity-adjusted start position
            float startY = (float)computeGravityTime(TimingChange.Time);
            // The gravity-adjusted end position
            float endY = (float)computeGravityTime(TimingChange.Time + Content.RelativeCoordinateSpace.Y);

            Content.Y = startY;
            Content.Height = endY - startY;
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

            return timeSpan - acceleration / 2 * relativeTime * relativeTime * sign;
        }

        /// <summary>
        /// The time spanned by this container.
        /// </summary>
        private double timeSpan => RelativeCoordinateSpace.Y;

        /// <summary>
        /// The acceleration due to "gravity" of the content of this container.
        /// </summary>
        private double acceleration => 2 * timeSpan / travel_time / travel_time;

        /// <summary>
        /// Computes the current time relative to <paramref name="time"/>, accounting for <see cref="travel_time"/>.
        /// </summary>
        /// <param name="time">The non-offset time.</param>
        /// <returns>The current time relative to <paramref name="time"/> - <see cref="travel_time"/>. </returns>
        private double relativeTimeAt(double time) => Time.Current - time + travel_time;

        /// <summary>
        /// The velocity of the content of this container at a time.
        /// </summary>
        /// <param name="time">The non-offset time.</param>
        /// <returns>The velocity at <paramref name="time"/>.</returns>
        private double velocityAt(double time) => acceleration * relativeTimeAt(time);
    }
}