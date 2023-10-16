// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    /// <summary>
    /// Stores the spinning history of a single spinner.<br />
    /// Instants of movement deltas may be added or removed from this in order to calculate the total rotation for the spinner.
    /// </summary>
    /// <remarks>
    /// A single, full rotation of the spinner is defined as a 360-degree rotation of the spinner, starting from 0, going in a single direction.<br />
    /// </remarks>
    /// <example>
    /// If the player spins 90-degrees clockwise, then changes direction, they need to spin 90-degrees counter-clockwise to return to 0
    /// and then continue rotating the spinner for another 360-degrees in the same direction.
    /// </example>
    public class SpinnerSpinHistory
    {
        /// <summary>
        /// The sum of all complete spins and any current partial spin, in degrees.
        /// </summary>
        /// <remarks>
        /// This is the final scoring value.
        /// </remarks>
        public float TotalRotation => 360 * segments.Count + currentMaxRotation;

        /// <summary>
        /// The list of all segments where either:
        /// <list type="bullet">
        /// <item>The spinning direction was changed.</item>
        /// <item>A full spin of 360 degrees was performed in either direction.</item>
        /// </list>
        /// </summary>
        private readonly Stack<SpinSegment> segments = new Stack<SpinSegment>();

        /// <summary>
        /// The total accumulated rotation.
        /// </summary>
        private float currentAbsoluteRotation;

        private float lastCompletionAbsoluteRotation;

        /// <summary>
        /// For the current spin, represents the maximum rotation (from 0..360) achieved by the user.
        /// </summary>
        private float currentMaxRotation;

        /// <summary>
        /// The current spin, from -360..360.
        /// </summary>
        private float currentRotation => currentAbsoluteRotation - lastCompletionAbsoluteRotation;

        private double lastReportTime = double.NegativeInfinity;

        /// <summary>
        /// Report a delta update based on user input.
        /// </summary>
        /// <param name="currentTime">The current time.</param>
        /// <param name="delta">The delta of the angle moved through since the last report.</param>
        public void ReportDelta(double currentTime, float delta)
        {
            // TODO: Debug.Assert(Math.Abs(delta) < 180);
            // This will require important frame guarantees.

            currentAbsoluteRotation += delta;

            if (currentTime >= lastReportTime)
                addDelta(currentTime, delta);
            else
                rewindDelta(currentTime, delta);

            lastReportTime = currentTime;
        }

        private void addDelta(double currentTime, float delta)
        {
            if (delta == 0)
                return;

            currentMaxRotation = Math.Max(currentMaxRotation, Math.Abs(currentRotation));

            while (currentMaxRotation >= 360)
            {
                int direction = Math.Sign(currentRotation);

                segments.Push(new SpinSegment(currentTime, direction));

                lastCompletionAbsoluteRotation += direction * 360;
                currentMaxRotation = Math.Abs(currentRotation);
            }
        }

        private void rewindDelta(double currentTime, float delta)
        {
            while (segments.TryPeek(out var segment) && segment.StartTime > currentTime)
            {
                segments.Pop();
                lastCompletionAbsoluteRotation -= segment.Direction * 360;
                currentMaxRotation = Math.Abs(currentRotation);
            }

            currentMaxRotation = Math.Abs(currentRotation);
        }

        /// <summary>
        /// Represents a single segment of history.
        /// </summary>
        /// <remarks>
        /// Each time the player changes direction, a new segment is recorded.
        /// A segment stores the current absolute angle of rotation. Generally this would be either -360 or 360 for a completed spin, or
        /// a number representing the last incomplete spin.
        /// </remarks>
        private class SpinSegment
        {
            /// <summary>
            /// The start time of this segment, when the direction change occurred.
            /// </summary>
            public readonly double StartTime;

            /// <summary>
            /// The direction this segment started in.
            /// </summary>
            public readonly int Direction;

            public SpinSegment(double startTime, int direction)
            {
                Debug.Assert(direction == -1 || direction == 1);

                StartTime = startTime;
                Direction = direction;
            }
        }
    }
}
