// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.UI.Scrolling.Algorithms
{
    public interface IScrollAlgorithm
    {
        /// <summary>
        /// Given a point in time, computes the time at which it enters the time range.
        /// </summary>
        /// <remarks>
        /// E.g. For a constant time range of 5000ms, the time at which t=7000ms enters the time range is 2000ms.
        /// </remarks>
        /// <param name="time">The point in time.</param>
        /// <param name="timeRange">The amount of visible time.</param>
        /// <returns>The time at which <paramref name="time"/> enters <see cref="timeRange"/>.</returns>
        double GetDisplayStartTime(double time, double timeRange);

        /// <summary>
        /// Computes the spatial length within a start and end time.
        /// </summary>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        /// <param name="timeRange">The amount of visible time.</param>
        /// <param name="scrollLength">The absolute spatial length through <see cref="timeRange"/>.</param>
        /// <returns>The absolute spatial length.</returns>
        float GetLength(double startTime, double endTime, double timeRange, float scrollLength);

        /// <summary>
        /// Given the current time, computes the spatial position of a point in time.
        /// </summary>
        /// <param name="time">The time to compute the spatial position of.</param>
        /// <param name="currentTime">The current time.</param>
        /// <param name="timeRange">The amount of visible time.</param>
        /// <param name="scrollLength">The absolute spatial length through <see cref="timeRange"/>.</param>
        /// <returns>The absolute spatial position.</returns>
        float PositionAt(double time, double currentTime, double timeRange, float scrollLength);

        /// <summary>
        /// Computes the time which brings a point to a provided spatial position given the current time.
        /// </summary>
        /// <param name="position">The absolute spatial position.</param>
        /// <param name="currentTime">The current time.</param>
        /// <param name="timeRange">The amount of visible time.</param>
        /// <param name="scrollLength">The absolute spatial length through <see cref="timeRange"/>.</param>
        /// <returns>The time at which <see cref="PositionAt(double,double,double,float)"/> == <paramref name="position"/>.</returns>
        double TimeAt(float position, double currentTime, double timeRange, float scrollLength);

        /// <summary>
        /// Resets this <see cref="IScrollAlgorithm"/> to a default state.
        /// </summary>
        void Reset();
    }
}
