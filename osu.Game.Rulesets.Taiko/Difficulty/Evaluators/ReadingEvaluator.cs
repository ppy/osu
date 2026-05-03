// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Mods;

namespace osu.Game.Rulesets.Taiko.Difficulty.Evaluators
{
    public static class ReadingEvaluator
    {
        private readonly struct VelocityRange
        {
            private double min { get; }
            private double max { get; }
            public double Center => (max + min) / 2;
            public double Range => max - min;

            public VelocityRange(double min, double max)
            {
                this.min = min;
                this.max = max;
            }
        }

        /// <summary>
        /// Calculates the influence of higher slider velocities on hitobject difficulty.
        /// The bonus is determined based on the EffectiveBPM, object density and the effects of mods.
        /// </summary>
        /// <param name="noteObject">The hit object to evaluate.</param>
        /// <param name="mods">The mods which were applied to the beatmap.</param>
        /// <returns>The reading difficulty value for the given hit object.</returns>
        public static double EvaluateDifficultyOf(TaikoDifficultyHitObject noteObject, Mod[] mods)
        {
            bool isHidden = mods.Any(m => m is TaikoModHidden);
            bool isFlashlight = mods.Any(m => m is TaikoModFlashlight);

            // With HDFL, all notes are invisible and give the maximum reading difficulty
            if (isHidden && isFlashlight)
                return 1.0;

            double velocityDifficulty = calculateVelocityDifficulty(noteObject, mods);
            double densityDifficulty = calculateDensityDifficulty(noteObject);

            double difficulty = Math.Max(velocityDifficulty, densityDifficulty);

            // With hidden, all notes award a base difficulty
            if (isHidden)
                difficulty = 0.4 + 0.6 * Math.Pow(difficulty, 1.25);

            return difficulty;
        }

        /// <summary>
        /// Calculate the difficulty from a note being at high or low velocity.
        /// </summary>
        private static double calculateVelocityDifficulty(TaikoDifficultyHitObject noteObject, Mod[] mods)
        {
            double highVelocityDifficulty = 0.0;
            double timeInvisibleDifficulty = 0.0;

            // To allow high velocity sections at lower actual BPM to award similar difficulty to high BPM sections with more frequent objects,
            // a bonus is applied to the high velocity range at lower object density
            double densityBonus = calculateHighVelocityDensityBonus(noteObject);

            var highVelocity = new VelocityRange(
                500 - 200 * densityBonus,
                1000 - 275 * densityBonus
            );

            highVelocityDifficulty = DifficultyCalculationUtils.Logistic(
                noteObject.EffectiveBPM * calculateHighVelocityModMultiplier(mods),
                highVelocity.Center,
                10.0 / highVelocity.Range
            );

            bool isHidden = mods.Any(m => m is TaikoModHidden);

            // With hidden, notes that stay invisible for longer before being hit are harder to read
            if (!isHidden) return Math.Max(highVelocityDifficulty, timeInvisibleDifficulty);

            var lowVelocity = new VelocityRange(280, 140);

            timeInvisibleDifficulty = DifficultyCalculationUtils.Logistic(
                noteObject.EffectiveBPM * calculateTimeInvisibleModMultiplier(mods),
                lowVelocity.Center,
                10.0 / lowVelocity.Range
            );

            return Math.Max(highVelocityDifficulty, timeInvisibleDifficulty);
        }

        /// <summary>
        /// Calculate the bonus to EffectiveBPM in high velocity calculation for a note being at low density.
        /// </summary>
        private static double calculateHighVelocityDensityBonus(TaikoDifficultyHitObject noteObject)
        {
            double density = calculateObjectDensity(noteObject);

            // Single note gaps in otherwise dense sections would overly award the bonus for low density
            // As a result, the higher density out of both the current and previous note is used
            var prevNoteObject = (TaikoDifficultyHitObject)noteObject.Previous(0);

            if (prevNoteObject == null) return DifficultyCalculationUtils.Smoothstep(density, 0.9, 0.35);

            double prevDensity = calculateObjectDensity(prevNoteObject);
            return DifficultyCalculationUtils.Smoothstep(Math.Max(density, prevDensity), 0.9, 0.35);
        }

        /// <summary>
        /// Calculate the effect on EffectiveBPM in high velocity calculation from reading mods.
        /// </summary>
        private static double calculateHighVelocityModMultiplier(Mod[] mods)
        {
            bool isHidden = mods.Any(m => m is TaikoModHidden);
            bool isFlashlight = mods.Any(m => m is TaikoModFlashlight);
            bool isEasy = mods.Any(m => m is TaikoModEasy);

            double multiplier = 1.0;

            if (isHidden)
            {
                // With hidden enabled, the playfield is limited from the expected 1560px wide (equivalent to 16:9) to only 1080px (4:3)
                // This is not the case with the classic mod enabled, but due to current limitations this is penalised in performance calculation instead
                // Considerations for HDHRCL are currently out of scope
                multiplier *= 1560.0 / 1080.0;

                // Notes fading out after a short time with hidden means their velocity is essentially higher. With easy enabled, notes take longer to fade out.
                // Both of these values are arbitrary and based on feedback
                if (isEasy)
                    multiplier *= 1.1;
                else
                    multiplier *= 1.2;
            }

            // With flashlight enabled, the visible playfield becomes more obscured as combo increases
            // As this is unrealistic to consider, an arbitrary value is used based on feedback
            if (isFlashlight)
                multiplier *= 4.5;

            return multiplier;
        }

        /// <summary>
        /// Calculate the effect on EffectiveBPM in time invisible calculation from reading mods.
        /// </summary>
        private static double calculateTimeInvisibleModMultiplier(Mod[] mods)
        {
            bool isEasy = mods.Any(m => m is TaikoModEasy);

            double multiplier = 1.0;

            // With easy enabled, notes fade out later and are invisible for less time. This is equivalent to their effective BPM being higher
            // This is not the case on lazer, but due to current limitations this cannot be rewarded
            if (isEasy)
                multiplier *= 1.35;

            return multiplier;
        }

        /// <summary>
        /// Calculate the difficulty from a note being at high density.
        /// </summary>
        private static double calculateDensityDifficulty(TaikoDifficultyHitObject noteObject) =>
            DifficultyCalculationUtils.Logistic(calculateObjectDensity(noteObject), 3.5, 1.5);

        /// <summary>
        /// Calculate the object density of a note.
        /// </summary>
        private static double calculateObjectDensity(TaikoDifficultyHitObject noteObject)
        {
            if (noteObject.EffectiveBPM == 0 || noteObject.DeltaTime == 0)
                return 1.0;

            // Expected DeltaTime is the DeltaTime this note would need to be spaced equally to a base slider velocity 1/4 note.
            double expectedDeltaTime = 21000.0 / noteObject.EffectiveBPM;

            return expectedDeltaTime / noteObject.DeltaTime;
        }
    }
}
