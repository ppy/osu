// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.UI.Scrolling.Algorithms
{
    public interface IScrollAlgorithm
    {
        /// <summary>
        /// Given a point in time associated with an object's origin
        /// and the spatial distance between the edge and the origin of the object along the scrolling axis,
        /// computes the time at which the object initially enters the time range.
        /// </summary>
        /// <example>
        /// Let's assume the following parameters:
        /// <list type="bullet">
        ///     <item><paramref name="originTime"/> = 7000ms,</item>
        ///     <item><paramref name="offset"/> = 100px,</item>
        ///     <item><paramref name="timeRange"/> = 5000ms,</item>
        ///     <item><paramref name="scrollLength"/> = 1000px</item>
        /// </list>
        /// and a constant scrolling rate.
        /// To arrive at the end of the scrolling container, the object's origin has to cover
        /// <code>1000 + 100 = 1100px</code>
        /// so that the edge starts at the end of the scrolling container.
        /// One scroll length of 1000px covers 5000ms of time, so the time required to cover 1100px is equal to
        /// <code>5000 * (1100 / 1000) = 5500ms,</code>
        /// and therefore the object should start being visible at
        /// <code>7000 - 5500 = 1500ms.</code>
        /// </example>
        /// <param name="originTime">The time point at which the object origin should enter the time range.</param>
        /// <param name="offset">The spatial distance between the object's edge and its origin along the scrolling axis.</param>
        /// <param name="timeRange">The amount of visible time.</param>
        /// <param name="scrollLength">The absolute spatial length through <paramref name="timeRange"/>.</param>
        /// <returns>The time at which the object should enter the time range.</returns>
        double GetDisplayStartTime(double originTime, float offset, double timeRange, float scrollLength);

        /// <summary>
        /// Computes the spatial length within a start and end time.
        /// </summary>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        /// <param name="timeRange">The amount of visible time.</param>
        /// <param name="scrollLength">The absolute spatial length through <paramref name="timeRange"/>.</param>
        /// <returns>The absolute spatial length.</returns>
        float GetLength(double startTime, double endTime, double timeRange, float scrollLength);

        /// <summary>
        /// Given the current time, computes the spatial position of a point in time.
        /// </summary>
        /// <param name="time">The time to compute the spatial position of.</param>
        /// <param name="currentTime">The current time.</param>
        /// <param name="timeRange">The amount of visible time.</param>
        /// <param name="scrollLength">The absolute spatial length through <paramref name="timeRange"/>.</param>
        /// <param name="originTime">The time to be used for control point lookups (ie. the parent's start time for nested hit objects).</param>
        /// <returns>The absolute spatial position.</returns>
        float PositionAt(double time, double currentTime, double timeRange, float scrollLength, double? originTime = null);

        /// <summary>
        /// Computes the time which brings a point to a provided spatial position given the current time.
        /// </summary>
        /// <param name="position">The absolute spatial position.</param>
        /// <param name="currentTime">The current time.</param>
        /// <param name="timeRange">The amount of visible time.</param>
        /// <param name="scrollLength">The absolute spatial length through <paramref name="timeRange"/>.</param>
        /// <returns>The time at which <see cref="PositionAt(double,double,double,float, double?)"/> == <paramref name="position"/>.</returns>
        double TimeAt(float position, double currentTime, double timeRange, float scrollLength);

        /// <summary>
        /// Resets this <see cref="IScrollAlgorithm"/> to a default state.
        /// </summary>
        void Reset();
    }
}
