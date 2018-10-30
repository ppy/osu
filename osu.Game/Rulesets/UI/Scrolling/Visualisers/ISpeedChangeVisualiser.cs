// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Rulesets.UI.Scrolling.Visualisers
{
    public interface ISpeedChangeVisualiser
    {
        /// <summary>
        /// Given a point in time, computes the time at which the point enters the visible time range of this <see cref="ISpeedChangeVisualiser"/>.
        /// </summary>
        /// <remarks>
        /// E.g. For a constant visible time range of 5000ms, the time at which t=7000ms enters the visible time range is 2000ms.
        /// </remarks>
        /// <param name="time">The time value.</param>
        /// <returns>The time at which <paramref name="time"/> enters the visible time range of this <see cref="ISpeedChangeVisualiser"/>.</returns>
        double GetDisplayStartTime(double time);

        /// <summary>
        /// Computes the spatial length within a start and end time.
        /// </summary>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        /// <returns>The absolute spatial length.</returns>
        float GetLength(double startTime, double endTime);

        /// <summary>
        /// Given the current time, computes the spatial position of a point in time.
        /// </summary>
        /// <param name="time">The time to compute the spatial position of.</param>
        /// <param name="currentTime">The current time.</param>
        /// <returns>The absolute spatial position.</returns>
        float PositionAt(double time, double currentTime);
    }
}
