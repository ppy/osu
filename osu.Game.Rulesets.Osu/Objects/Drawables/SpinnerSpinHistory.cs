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
        /// The list of all turning points where either:
        /// <list type="bullet">
        /// <item>The spinning direction was changed.</item>
        /// <item>A full spin of 360 degrees was performed in either direction.</item>
        /// </list>
        /// </summary>
        private readonly Stack<Turn> turningPoints = new Stack<Turn>();

        /// <summary>
        /// The current partial spin - the maximum absolute rotation among all turning points since the last spin.
        /// </summary>
        private float currentMaxRotation;

        /// <summary>
        /// The current turn.
        /// </summary>
        private Turn currentTurn;

        /// <summary>
        /// Report a delta update based on user input.
        /// </summary>
        /// <param name="currentTime">The current time.</param>
        /// <param name="deltaAngle">The delta of the angle moved through since the last report.</param>
        public void ReportDelta(double currentTime, float deltaAngle)
        {
            if (currentTime >= currentTurn.StartTime)
                addDelta(currentTime, deltaAngle);
            else
                rewindDelta(currentTime, deltaAngle);
        }

        private void addDelta(double currentTime, float delta)
        {
            if (delta == 0)
                return;

            int direction = Math.Sign(delta);

            // Start a new turn if this is the first delta, to track the correct direction.
            if (currentTurn.Direction == 0)
                beginNewTurn(double.NegativeInfinity, direction);

            // Start a new turn if we've changed direction.
            if (currentTurn.Direction != direction)
                beginNewTurn(currentTime, direction);

            currentTurn.Angle += delta;

            float rotation = Math.Abs(currentTurn.Angle);

            TotalRotation += Math.Max(0, rotation - currentMaxRotation);

            // Start a new turn if we've completed a spin.
            while (rotation >= 360)
            {
                rotation -= 360;

                // Make sure the current turn doesn't exceed a full spin.
                currentTurn.Angle = Math.Clamp(currentTurn.Angle, -360, 360);
                Debug.Assert(MathF.Abs(currentTurn.Angle) == 360);

                beginNewTurn(currentTime, direction);

                // The new turn should be in the same direction and with the excess of the previous turn.
                currentTurn.Angle = rotation * direction;
                currentMaxRotation = 0;
            }

            currentMaxRotation = Math.Max(currentMaxRotation, rotation);
        }

        private void rewindDelta(double currentTime, float delta)
        {
            while (currentTime < currentTurn.StartTime)
            {
                // When crossing over a turn, we need to adjust the delta so that it's relative to the end point of the next turn.
                //
                // This is done by ADDING the delta between the current turn and the next turn.
                // To understand why this is, notice that delta is a rate-independent value. Suppose the turn values are { 90, 45 } (i.e. CW then CCW spin)...
                // - If delta < 0 (e.g. -15) (i.e. CCW rotation), then the next turn should be <45 (therefore delta = -15 + (45 - 90) = -60, next = 30).
                // - If delta = 0, then the next turn should be =45 (therefore delta = 0 + (45 - 90) = -45, next = 45).
                // - If delta > 0 (e.g. +15) (i.e. CW rotation), then the next turn should be >45 (therefore delta = 15 + (45 - 90) = -30, next = 60).
                //
                // There is a special case when crossing a complete spin, because the turn following it starts at 0 rather than the previous turn's value.
                // In this case, only the remaining delta in the current turn needs to be considered.

                Turn nextTurn = turningPoints.Pop();

                if (nextTurn.IsCompleteSpin)
                    delta += currentTurn.Angle;
                else
                    delta += currentTurn.Angle - nextTurn.Angle;

                currentTurn = nextTurn;
            }

            currentTurn.Angle += delta;

            // Note: Enumerating through a stack is already reverse order.
            currentMaxRotation = turningPoints.Prepend(currentTurn).TakeWhile(t => !t.IsCompleteSpin).Select(t => Math.Abs(t.Angle)).Max();
            TotalRotation = 360 * turningPoints.Count(t => t.IsCompleteSpin) + currentMaxRotation;
        }

        /// <summary>
        /// Finishes the current turn and starts a new one from its end point.
        /// </summary>
        /// <param name="currentTime">The start time of the new turn.</param>
        /// <param name="direction">The turning direction.</param>
        private void beginNewTurn(double currentTime, int direction)
        {
            turningPoints.Push(currentTurn);
            currentTurn = new Turn(currentTime, direction) { Angle = currentTurn.Angle };
        }

        /// <summary>
        /// Represents a single direction turn of the spinner.
        /// </summary>
        private struct Turn
        {
            /// <summary>
            /// The start time of this turn.
            /// </summary>
            public readonly double StartTime;

            /// <summary>
            /// The turning direction.
            /// </summary>
            public readonly int Direction;

            /// <summary>
            /// The absolute angle.
            /// </summary>
            public float Angle;

            /// <summary>
            /// Whether this turn represents a complete spin.
            /// </summary>
            public bool IsCompleteSpin => Angle == -360 || Angle == 360;

            public Turn(double startTime, int direction)
            {
                Debug.Assert(direction == -1 || direction == 1);

                StartTime = startTime;
                Direction = direction;
                Angle = 0;
            }
        }
    }
}
