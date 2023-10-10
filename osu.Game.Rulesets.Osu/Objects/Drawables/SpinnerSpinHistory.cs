// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Logging;

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

        private double lastReportTime = double.NegativeInfinity;

        /// <summary>
        /// Report a delta update based on user input.
        /// </summary>
        /// <param name="currentTime">The current time.</param>
        /// <param name="deltaAngle">The delta of the angle moved through since the last report.</param>
        public void ReportDelta(double currentTime, float deltaAngle)
        {
            if (currentTime >= lastReportTime)
                addDelta(currentTime, deltaAngle);
            else
                rewindDelta(currentTime, deltaAngle);

            lastReportTime = currentTime;
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

            Logger.Log($"ADD | Time: {currentTime} Delta: {delta}, Segment: {currentSpinSegment.CurrentRotation}, Total: {TotalRotation}");
        }

        private void rewindDelta(double currentTime, float delta)
        {
            Debug.Assert(currentSpinSegment.Direction != 0);

            // The current, absolute (i.e. 0-to-360) rotation.
            float currentPosAbsolute = (currentSpinSegment.CurrentRotation + delta) % 360;
            if (Math.Sign(currentPosAbsolute) < 0)
                currentPosAbsolute += 360;

            // Exclude any segments that we've rewound past.
            while (segments.Count > 0 && currentTime <= currentSpinSegment.StartTime)
                currentSpinSegment = segments.Pop();

            // Compute the rotation for the current segment based on the absolute rotatiobn.
            currentSpinSegment.CurrentRotation = currentSpinSegment.Direction < 0
                ? Math.Min(0, currentPosAbsolute - 360)
                : Math.Max(0, currentPosAbsolute);

            // Check if we've rewound exactly onto a complete spin, and insert a new segment.
            if (currentSpinSegment.IsCompleteSpin)
            {
                // The direction doesn't really matter here - a new segment will be inserted during following forward playback if incorrect.
                beginNewSegment(currentTime, currentSpinSegment.Direction);
                currentSpinSegment.CurrentRotation = 0;
            }

            // Note: Enumerating through a stack is already reverse order.
            IEnumerable<SpinSegment> allSegments = segments.Prepend(currentSpinSegment);

            currentMaxRotation = allSegments
                                 .TakeWhile(t => !t.IsCompleteSpin)
                                 .Select(t => Math.Abs(t.CurrentRotation))
                                 .Max();

            TotalRotation = 360 * allSegments.Count(t => t.IsCompleteSpin)
                            + currentMaxRotation;

            Logger.Log($"REMOVE | Time: {currentTime} Delta: {delta}, Segment: {currentSpinSegment.CurrentRotation}, Total: {TotalRotation}");
        }

        /// <summary>
        /// Finishes the current segment and starts a new one from its end point.
        /// </summary>
        /// <param name="currentTime">The start time of the new segment.</param>
        /// <param name="direction">The segment's direction.</param>
        private void beginNewSegment(double currentTime, int direction)
        {
            if (currentSpinSegment.Direction != 0)
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
