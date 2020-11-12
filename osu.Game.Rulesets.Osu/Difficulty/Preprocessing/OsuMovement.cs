// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Osu.Difficulty.Preprocessing
{
    internal class OsuMovement
    {
        /// <summary>
        /// Uncorrected time taken to execute the movement.
        /// </summary>
        public double RawMovementTime { get; set; }

        /// <summary>
        /// Corrected distance between objects.
        /// </summary>
        public double Distance { get; set; }

        /// <summary>
        /// Corrected movement time.
        /// </summary>
        public double MovementTime { get; set; }

        /// <summary>
        /// The calculated throughput of the player for this movement.
        /// </summary>
        public double Throughput { get; set; }

        /// <summary>
        /// Estimated cheesablility of the movement.
        /// </summary>
        public double Cheesablility { get; set; }

        /// <summary>
        /// The "cheese window" of the movement
        /// (how much allowance is gained by hitting first note early and the second late).
        /// </summary>
        public double CheeseWindow { get; set; }

        /// <summary>
        /// The start time of the movement, in seconds.
        /// </summary>
        public double StartTime { get; set; }

        /// <summary>
        /// Whether the movement ends on a slider.
        /// </summary>
        public bool EndsOnSlider { get; set; }

        public static OsuMovement Empty(double time)
        {
            return new OsuMovement
            {
                Distance = 0,
                MovementTime = 1,
                CheeseWindow = 0,
                Cheesablility = 0,
                RawMovementTime = 0,
                Throughput = 0,
                StartTime = time
            };
        }
    }
}
