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
        public float TotalRotation => 360 * completedSpins.Count + currentSpinMaxRotation;

        private readonly Stack<CompletedSpin> completedSpins = new Stack<CompletedSpin>();

        /// <summary>
        /// The total accumulated (absolute) rotation.
        /// </summary>
        private float totalAccumulatedRotation;

        private float totalAccumulatedRotationAtLastCompletion;

        /// <summary>
        /// For the current spin, represents the maximum absolute rotation (from 0..360) achieved by the user.
        /// </summary>
        /// <remarks>
        /// This is used to report <see cref="TotalRotation"/> in the case a user spins backwards.
        /// Basically it allows us to not reduce the total rotation in such a case.
        ///
        /// This also stops spinner "cheese" where a user may rapidly change directions and cause an increase
        /// in rotations.
        /// </remarks>
        private float currentSpinMaxRotation;

        /// <summary>
        /// The current spin, from -360..360.
        /// </summary>
        private float currentSpinRotation => totalAccumulatedRotation - totalAccumulatedRotationAtLastCompletion;

        private double lastReportTime = double.NegativeInfinity;

        /// <summary>
        /// Report a delta update based on user input.
        /// </summary>
        /// <param name="currentTime">The current time.</param>
        /// <param name="delta">The delta of the angle moved through since the last report.</param>
        public void ReportDelta(double currentTime, float delta)
        {
            if (delta == 0)
                return;

            // Importantly, outside of tests the max delta entering here is 180 degrees.
            // If it wasn't for tests, we could add this line:
            //
            // Debug.Assert(Math.Abs(delta) < 180);
            //
            // For this to be 101% correct, we need to add the ability for important frames to be
            // created based on gameplay intrinsics (ie. there should be one frame for any spinner delta 90 < n < 180 degrees).
            //
            // But this can come later.

            totalAccumulatedRotation += delta;

            if (currentTime >= lastReportTime)
            {
                currentSpinMaxRotation = Math.Max(currentSpinMaxRotation, Math.Abs(currentSpinRotation));

                // Handle the case where the user has completed another spin.
                // Note that this does could be an `if` rather than `while` if the above assertion held true.
                // It is a `while` loop to handle tests which throw larger values at this method.
                while (currentSpinMaxRotation >= 360)
                {
                    int direction = Math.Sign(currentSpinRotation);

                    completedSpins.Push(new CompletedSpin(currentTime, direction));

                    // Incrementing the last completion point will cause `currentSpinRotation` to
                    // hold the remaining spin that needs to be considered.
                    totalAccumulatedRotationAtLastCompletion += direction * 360;

                    // Reset the current max as we are entering a new spin.
                    // Importantly, carry over the remainder (which is now stored in `currentSpinRotation`).
                    currentSpinMaxRotation = Math.Abs(currentSpinRotation);
                }
            }
            else
            {
                // When rewinding, the main thing we care about is getting `totalAbsoluteRotationsAtLastCompletion`
                // to the correct value. We can used the stored history for this.
                while (completedSpins.TryPeek(out var segment) && segment.CompletionTime > currentTime)
                {
                    completedSpins.Pop();
                    totalAccumulatedRotationAtLastCompletion -= segment.Direction * 360;
                }

                // This is a best effort. We may not have enough data to match this 1:1, but that's okay.
                // We know that the player is somewhere in a spin.
                // In the worst case, this will be lower than expected, and recover in forward playback.
                currentSpinMaxRotation = Math.Abs(currentSpinRotation);
            }

            lastReportTime = currentTime;
        }

        /// <summary>
        /// Represents a single completed spin.
        /// </summary>
        private class CompletedSpin
        {
            /// <summary>
            /// The time at which this spin completion occurred.
            /// </summary>
            public readonly double CompletionTime;

            /// <summary>
            /// The direction this spin completed in.
            /// </summary>
            public readonly int Direction;

            public CompletedSpin(double completionTime, int direction)
            {
                Debug.Assert(direction == -1 || direction == 1);

                CompletionTime = completionTime;
                Direction = direction;
            }
        }
    }
}
