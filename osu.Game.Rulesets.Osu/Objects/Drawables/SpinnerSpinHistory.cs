// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
        public float TotalRotation { get; private set; }

        /// <summary>
        /// The list of all segments where either:
        /// <list type="bullet">
        /// <item>The spinning direction was changed.</item>
        /// <item>A full spin of 360 degrees was performed in either direction.</item>
        /// </list>
        /// </summary>
        private readonly Stack<SpinSegment> segments = new Stack<SpinSegment>();

        /// <summary>
        /// The current partial spin - the maximum absolute rotation among all segments since the last spin.
        /// </summary>
        private float currentMaxRotation;

        /// <summary>
        /// The current segment.
        /// </summary>
        private SpinSegment currentSpinSegment;

        /// <summary>
        /// Report a delta update based on user input.
        /// </summary>
        /// <param name="currentTime">The current time.</param>
        /// <param name="deltaAngle">The delta of the angle moved through since the last report.</param>
        public void ReportDelta(double currentTime, float deltaAngle)
        {
            if (currentTime >= currentSpinSegment.StartTime)
                addDelta(currentTime, deltaAngle);
            else
                rewindDelta(currentTime, deltaAngle);
        }

        private void addDelta(double currentTime, float delta)
        {
            if (delta == 0)
                return;

            int direction = Math.Sign(delta);

            // Start a new segment if this is the first delta, to track the correct direction.
            if (currentSpinSegment.Direction == 0)
                beginNewSegment(double.NegativeInfinity, direction);

            // Start a new segment if we've changed direction.
            if (currentSpinSegment.Direction != direction)
                beginNewSegment(currentTime, direction);

            currentSpinSegment.CurrentRotation += delta;

            float rotation = Math.Abs(currentSpinSegment.CurrentRotation);

            TotalRotation += Math.Max(0, rotation - currentMaxRotation);

            // Start a new segment if we've completed a spin.
            while (rotation >= 360)
            {
                rotation -= 360;

                // Make sure the current segment doesn't exceed a full spin.
                currentSpinSegment.CurrentRotation = Math.Clamp(currentSpinSegment.CurrentRotation, -360, 360);
                Debug.Assert(MathF.Abs(currentSpinSegment.CurrentRotation) == 360);

                beginNewSegment(currentTime, direction);

                // The new segment should be in the same direction and with the excess of the previous segment.
                currentSpinSegment.CurrentRotation = rotation * direction;
                currentMaxRotation = 0;
            }

            currentMaxRotation = Math.Max(currentMaxRotation, rotation);
        }

        private void rewindDelta(double currentTime, float delta)
        {
            while (currentTime < currentSpinSegment.StartTime)
            {
                // When crossing over a segment, we need to adjust the delta so that it's relative to the end point of the next segment.
                //
                // This is done by ADDING the delta between the current segment and the next segment.
                // To understand why this is, notice that delta is a rate-independent value. Suppose the segment values are { 90, 45 } (i.e. CW then CCW spin)...
                // - If delta < 0 (e.g. -15) (i.e. CCW rotation), then the next segment should be <45 (therefore delta = -15 + (45 - 90) = -60, next = 30).
                // - If delta = 0, then the next segment should be =45 (therefore delta = 0 + (45 - 90) = -45, next = 45).
                // - If delta > 0 (e.g. +15) (i.e. CW rotation), then the next segment should be >45 (therefore delta = 15 + (45 - 90) = -30, next = 60).
                //
                // There is a special case when crossing a complete spin, because the segment following it starts at 0 rather than the previous segment's value.
                // In this case, only the remaining delta in the current segment needs to be considered.

                SpinSegment nextSpinSegment = segments.Pop();

                if (nextSpinSegment.IsCompleteSpin)
                    delta += currentSpinSegment.CurrentRotation;
                else
                    delta += currentSpinSegment.CurrentRotation - nextSpinSegment.CurrentRotation;

                currentSpinSegment = nextSpinSegment;
            }

            currentSpinSegment.CurrentRotation += delta;

            // Note: Enumerating through a stack is already reverse order.
            currentMaxRotation = segments.Prepend(currentSpinSegment)
                                         .TakeWhile(t => !t.IsCompleteSpin)
                                         .Select(t => Math.Abs(t.CurrentRotation))
                                         .DefaultIfEmpty(0)
                                         .Max();

            TotalRotation = 360 * segments.Count(t => t.IsCompleteSpin) + currentMaxRotation;
        }

        /// <summary>
        /// Finishes the current segment and starts a new one from its end point.
        /// </summary>
        /// <param name="currentTime">The start time of the new segment.</param>
        /// <param name="direction">The segment's direction.</param>
        private void beginNewSegment(double currentTime, int direction)
        {
            segments.Push(currentSpinSegment);
            currentSpinSegment = new SpinSegment(currentTime, direction, currentSpinSegment.CurrentRotation);
        }

        /// <summary>
        /// Represents a single segment of history.
        /// </summary>
        /// <remarks>
        /// Each time the player changes direction, a new segment is recorded.
        /// A segment stores the current absolute angle of rotation. Generally this would be either -360 or 360 for a completed spin, or
        /// a number representing the last incomplete spin.
        /// </remarks>
        private struct SpinSegment
        {
            /// <summary>
            /// The start time of this segment, when the direction change occurred.
            /// </summary>
            public readonly double StartTime;

            /// <summary>
            /// The direction this segment started in.
            /// </summary>
            public readonly int Direction;

            /// <summary>
            /// The current rotation at the last known point in this segment.
            /// </summary>
            /// <remarks>
            /// - In the case of a completed spin, this is either -360 or 360.
            /// - For the final (or ongoing) segment, this is a value representing how close to completing the spin we are.
            /// </remarks>
            public float CurrentRotation;

            /// <summary>
            /// Whether this segment represents a complete spin.
            /// </summary>
            public bool IsCompleteSpin => CurrentRotation == -360 || CurrentRotation == 360;

            public SpinSegment(double startTime, int direction, float currentRotation)
            {
                Debug.Assert(direction == -1 || direction == 1);

                StartTime = startTime;
                Direction = direction;
                CurrentRotation = currentRotation;
            }
        }
    }
}
