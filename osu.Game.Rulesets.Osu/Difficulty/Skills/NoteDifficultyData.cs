// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    internal struct NoteDifficultyData
    {
        /// <summary>
        /// The start time of this note.
        /// </summary>
        public double StartTime { get; private set; }

        /// <summary>
        /// The time between the preceding and this note.
        /// </summary>
        public double DeltaTime { get; }

        /// <summary>
        /// The start time of the preceding note.
        /// </summary>
        public double PreviousStartTime => StartTime - DeltaTime;

        /// <summary>
        /// The exponentiated difficulty of this note.
        /// </summary>
        public double ExponentiatedDifficulty { get; }

        /// <summary>
        /// The sum of all exponentiated note difficulties up to and including this one.
        /// </summary>
        public double TotalExponentiatedDifficulty { get; private set; }

        /// <summary>
        /// Creates a <see cref="NoteDifficultyData"/> object corresponding to a slider tick.
        /// Slider ticks do not carry any difficulty.
        /// </summary>
        /// <param name="hitObject">The <see cref="DifficultyHitObject"/> corresponding to the slider tick.</param>
        /// <param name="totalExponentiatedDifficulty">The total exponentiated difficulty of all notes up to and including this one.</param>
        /// <returns></returns>
        public static NoteDifficultyData SliderTick(DifficultyHitObject hitObject, double totalExponentiatedDifficulty)
        {
            return new NoteDifficultyData
            {
                TotalExponentiatedDifficulty = totalExponentiatedDifficulty,
                StartTime = hitObject.StartTime
            };
        }

        /// <summary>
        /// Creates a <see cref="NoteDifficultyData"/> object corresponding to a map object.
        /// </summary>
        /// <param name="hitObject">The <see cref="DifficultyHitObject"/> corresponding to this note.</param>
        /// <param name="strain">The current strain for a given skill.</param>
        /// <param name="durationScalingFactor">Scaling factor for strain duration.</param>
        /// <param name="difficultyExponent">The difficulty exponent for a given skill.</param>
        /// <param name="totalExponentialDifficulty">
        /// The total exponentiated difficulty of all notes up to and excluding this one.
        /// Will be increased by this note's difficulty due to being passed by reference.
        /// </param>
        public NoteDifficultyData(
            DifficultyHitObject hitObject,
            double strain,
            double durationScalingFactor,
            double difficultyExponent,
            ref double totalExponentialDifficulty)
        {
            // Uses legacy formula to convert from strain into star rating
            double difficulty = Math.Sqrt(strain * 10) * 0.0675;

            ExponentiatedDifficulty = Math.Pow(difficulty, difficultyExponent) * durationScalingFactor;
            totalExponentialDifficulty += ExponentiatedDifficulty;
            TotalExponentiatedDifficulty = totalExponentialDifficulty;

            StartTime = hitObject.StartTime;
            DeltaTime = hitObject.DeltaTime;
        }
    }
}
