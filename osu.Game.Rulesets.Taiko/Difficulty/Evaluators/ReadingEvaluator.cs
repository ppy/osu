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
            public double Min { get; }
            public double Max { get; }
            public double Center => (Max + Min) / 2;
            public double Range => Max - Min;

            public VelocityRange(double min, double max)
            {
                Min = min;
                Max = max;
            }
        }

        /// <summary>
        /// Calculates the influence of higher slider velocities on hitobject difficulty.
        /// The bonus is determined based on the EffectiveBPM, object density and the effects of mods.
        /// </summary>
        /// <param name="noteObject">The hit object to evaluate.</param>
        /// <param name="mods">The mods which were applied to the beatmap.</param>
        /// <returns>The reading difficulty value for the given hit object.</returns>
        public static double EvaluateDifficultyOf(TaikoDifficultyHitObject noteObject, Mod[] mods, bool isHidden)
        {
            bool isFlashlight = mods.Any(m => m is TaikoModFlashlight);

            // With HDFL, all note objects are invisible and give the maximum reading difficulty
            if (isHidden && isFlashlight)
                return 1.0;

            double velocityDifficulty = calculateVelocityDifficulty(noteObject, mods, isHidden);
            double densityDifficulty = calculateDensityDifficulty(noteObject);

            double difficulty = Math.Max(velocityDifficulty, densityDifficulty);

            // With hidden, all notes award a base difficulty
            if (isHidden)
                difficulty = 0.25 + 0.75 * difficulty;

            return difficulty;
        }

        private static double calculateVelocityDifficulty(TaikoDifficultyHitObject noteObject, Mod[] mods, bool isHidden)
        {
            double highVelocityDifficulty = 0.0;
            double timeInvisibleDifficulty = 0.0;

            // To allow high velocity sections at lower actual BPM to award similar difficulty to high BPM sections with more frequent objects,
            // a bonus is applied to the high velocity range at lower object density
            double densityBonus = calculateHighVelocityDensityBonus(noteObject);

            var highVelocity = new VelocityRange(
                420 - 140 * densityBonus,
                1000 - 320 * densityBonus
            );

            highVelocityDifficulty = DifficultyCalculationUtils.Logistic(
                noteObject.EffectiveBPM * calculateHighVelocityModMultiplier(mods, isHidden),
                highVelocity.Center,
                10.0 / highVelocity.Range
            );

            // With hidden, notes that stay invisible for longer before being hit are harder to read
            if (isHidden) {
                var lowVelocity = new VelocityRange(280, 125);

                timeInvisibleDifficulty = DifficultyCalculationUtils.Logistic(
                    noteObject.EffectiveBPM * calculateTimeInvisibleModMultiplier(mods),
                    lowVelocity.Center,
                    10.0 / lowVelocity.Range
                );
            }

            return Math.Max(highVelocityDifficulty, timeInvisibleDifficulty);
        }

        private static double calculateHighVelocityDensityBonus(TaikoDifficultyHitObject noteObject)
        {
            double density = calculateObjectDensity(noteObject);

            // Single note gaps in otherwise dense sections would overly award the bonus for low density
            // As a result, the higher density out of both the current and previous note is used
            var prevNoteObject = (TaikoDifficultyHitObject) noteObject.Previous(0);

            if (prevNoteObject != null)
            {
                double prevDensity = calculateObjectDensity(prevNoteObject);
                return DifficultyCalculationUtils.Smoothstep(Math.Max(density, prevDensity), 0.9, 0.35);
            }

            return DifficultyCalculationUtils.Smoothstep(density, 0.9, 0.35);
        }

        private static double calculateHighVelocityModMultiplier(Mod[] mods, bool isHidden)
        {
            bool isFlashlight = mods.Any(m => m is TaikoModFlashlight);
            bool isEasy = mods.Any(m => m is TaikoModEasy);

            double multiplier = 1.0;

            if (isHidden)
            {	
                // With hidden enabled, the playfield is limited from the expected 1560px wide (equivalent to 16:9) to only 1080px (4:3)
                // This is not the case with the classic mod enabled, but due to current limitations this is penalised in performance calculation instead
                // Considerations for HDHRCL are currently out of scope
                multiplier *= 1560.0 / 1080.0;

                // Notes fading out after a short time with hidden means their velocity is essentially higher. With easy enabled, notes fade out after longer.
                // Both of these values are arbitrary and based on feedback
                if (isEasy)
                    multiplier *= 1.1;
                else
                    multiplier *= 1.2;
            }

            // With flashlight, the visible playfield is limited from the expected 1560px wide to around 468px
            // Considerations for combo and smaller flashlights are currently out of scope
            if (isFlashlight)
                multiplier *= 1560.0 / 468.0;

            return multiplier;
        }

        private static double calculateTimeInvisibleModMultiplier(Mod[] mods)
        {
            bool isEasy = mods.Any(m => m is TaikoModEasy);

            double multiplier = 1.0;

            // With easy enabled, notes fade out later and are invisible for less time. This is equivalent to their effective BPM being higher
            if (isEasy)
                multiplier *= 1.35;

            return multiplier;
        }

        private static double calculateDensityDifficulty(TaikoDifficultyHitObject noteObject)
        {
            // Notes at very high density are harder to read
            return Math.Pow(
                DifficultyCalculationUtils.Logistic(calculateObjectDensity(noteObject), 3.5, 1.5),
                3.0
            );
        }

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
