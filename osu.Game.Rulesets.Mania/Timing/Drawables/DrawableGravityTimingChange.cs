// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Timing;

namespace osu.Game.Rulesets.Mania.Timing.Drawables
{
    public class DrawableGravityTimingChange : DrawableManiaTimingChange
    {
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
            float endY = (float)computeGravityTime(TimingChange.Time + Content.RelativeChildSize.Y);

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

            return timeSpan - acceleration * relativeTime * relativeTime * sign;
        }

        /// <summary>
        /// The time spanned by this container.
        /// </summary>
        private double timeSpan => RelativeChildSize.Y;

        /// <summary>
        /// The acceleration due to "gravity" of the content of this container.
        /// </summary>
        private double acceleration => 1 / timeSpan;

        /// <summary>
        /// Computes the current time relative to <paramref name="time"/>, accounting for <see cref="timeSpan"/>.
        /// </summary>
        /// <param name="time">The non-offset time.</param>
        /// <returns>The current time relative to <paramref name="time"/> - <see cref="timeSpan"/>. </returns>
        private double relativeTimeAt(double time) => Time.Current - time + timeSpan;
    }
}