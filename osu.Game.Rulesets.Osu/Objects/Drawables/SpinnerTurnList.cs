// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class SpinnerTurnList
    {
        /// <summary>
        /// The list of all turning points where either:
        /// <list type="bullet">
        /// <item>The spinning direction was changed.</item>
        /// <item>A full spin of 360 degrees was performed in either direction.
        /// Note that if the user first spun 359deg counter-clockwise, the user has to then spin 720deg clockwise to meet this criteria (-359deg -> +360deg).</item>
        /// </list>
        /// </summary>
        private readonly Stack<Turn> turningPoints = new Stack<Turn>();

        /// <summary>
        /// The total rotation - a summation of all complete spins and the current partial spin.
        /// </summary>
        /// <remarks>
        /// This is the final scoring value.
        /// </remarks>
        private float totalRotation;

        /// <summary>
        /// The current partial spin - the maximum absolute rotation among all turning points since the last spin.
        /// </summary>
        private float currentMaxRotation;

        /// <summary>
        /// The current turn.
        /// </summary>
        private Turn currentTurn;

        /// <summary>
        /// Adds a spinning delta.
        /// </summary>
        /// <param name="currentTime">The current time.</param>
        /// <param name="delta">The rate-independent, instantaneous delta of the angle moved through. Negative values represent counter-clockwise movements, positive values represent clockwise movements.</param>
        /// <returns>The total rotation after applying the delta.</returns>
        public float AddDelta(double currentTime, float delta)
        {
            if (delta == 0)
                return totalRotation;

            int direction = Math.Sign(delta);

            // Start a new turn if this is the first delta, to track the correct direction.
            if (currentTurn.Direction == 0)
                beginNewTurn(double.NegativeInfinity, direction);

            // Start a new turn if we've changed direction.
            if (currentTurn.Direction != direction)
                beginNewTurn(currentTime, direction);

            currentTurn.Current += delta;

            float rotation = Math.Abs(currentTurn.Current);

            totalRotation += Math.Max(0, rotation - currentMaxRotation);

            // Start a new turn if we've completed a spin.
            while (rotation >= 360)
            {
                rotation -= 360;

                // Make sure the current turn doesn't exceed a full spin.
                currentTurn.Current = Math.Clamp(currentTurn.Current, -360, 360);
                currentTurn.IsCompleteSpin = true;

                beginNewTurn(currentTime, direction);

                // The new turn should be in the same direction and with the excess of the previous turn.
                currentTurn.Current = rotation * direction;
                currentMaxRotation = 0;
            }

            currentMaxRotation = Math.Max(currentMaxRotation, rotation);

            return totalRotation;
        }

        /// <summary>
        /// Removes a spinning delta.
        /// </summary>
        /// <param name="currentTime">The current time.</param>
        /// <param name="delta">The rate-independent, instantaneous delta of the angle moved through. Negative values represent counter-clockwise movements, positive values represent clockwise movements.</param>
        /// <returns>The total rotation after removing the delta.</returns>
        public float RemoveDelta(double currentTime, float delta)
        {
            while (currentTime < currentTurn.StartTime)
            {
                // When crossing over a turn, we need to adjust the delta so that it's relative to the end point of the next turn.
                //
                // This is done by ADDING the delta between the current turn and the next turn.
                // To understand this why this is, notice that delta is a rate-independent value. Suppose the turn values: { 90, 45 } (i.e. CW then CCW spin)...
                // - If delta < 0 (e.g. -15) (i.e. CCW rotation), then the next turn should be <45 (therefore delta = -15 + (45 - 90) = -60, next = 30).
                // - If delta = 0, then the next turn should be =45 (therefore delta = 0 + (45 - 90) = -45, next = 45).
                // - If delta > 0 (e.g. +15) (i.e. CW rotation), then the next turn should be >45 (therefore delta = 15 + (45 - 90) = -30, next = 60).
                //
                // There is a special case when crossing a complete spin, because the turn following it starts at 0 rather than +/- 360.
                // In this case, only the remaining delta in the current turn needs to be considered.

                Turn nextTurn = turningPoints.Pop();

                if (nextTurn.IsCompleteSpin)
                    delta += currentTurn.Current;
                else
                    delta += currentTurn.Current - nextTurn.Current;

                currentTurn = nextTurn;
            }

            currentTurn.Current += delta;
            currentTurn.IsCompleteSpin = false;

            // Note: Enumerating through a stack is already reverse order.
            currentMaxRotation = turningPoints.Prepend(currentTurn).TakeWhile(t => !t.IsCompleteSpin).Select(t => Math.Abs(t.Current)).Max();
            totalRotation = 360 * turningPoints.Count(t => t.IsCompleteSpin) + currentMaxRotation;

            return totalRotation;
        }

        private void beginNewTurn(double currentTime, int direction)
        {
            turningPoints.Push(currentTurn);
            currentTurn = new Turn(currentTime, direction) { Current = currentTurn.Current };
        }

        private struct Turn
        {
            /// <summary>
            /// Time start time of this turn.
            /// </summary>
            public readonly double StartTime;

            /// <summary>
            /// The turn direction.
            /// </summary>
            public readonly int Direction;

            /// <summary>
            /// The amount turned in this direction.
            /// </summary>
            public float Current;

            /// <summary>
            /// Whether this turn represents a complete spin.
            /// </summary>
            public bool IsCompleteSpin;

            public Turn(double startTime, int direction)
            {
                Debug.Assert(direction is -1 or 1);

                StartTime = startTime;
                Direction = direction;
                Current = 0;
                IsCompleteSpin = false;
            }
        }
    }
}
