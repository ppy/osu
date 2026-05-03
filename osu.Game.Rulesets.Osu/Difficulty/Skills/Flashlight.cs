// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Evaluators;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Mods;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Represents the skill required to memorise and hit every object in a map with the Flashlight mod enabled.
    /// </summary>
    public class Flashlight : StrainSkill
    {
        private readonly int totalObjects;

        public Flashlight(Mod[] mods, int totalObjects)
            : base(mods)
        {
            this.totalObjects = totalObjects;
        }

        private double skillMultiplier => 0.056;
        private double strainDecayBase => 0.15;

        private double currentStrain;

        private double strainDecay(double ms) => Math.Pow(strainDecayBase, ms / 1000);

        protected override double CalculateInitialStrain(double time, DifficultyHitObject current) => currentStrain * strainDecay(time - current.Previous(0).StartTime);

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            if (!Mods.Any(m => m is OsuModFlashlight))
                return 0;

            currentStrain *= strainDecay(current.DeltaTime);
            currentStrain += calculateAdjustedDifficulty(current) * skillMultiplier;

            return currentStrain;
        }

        private double calculateAdjustedDifficulty(DifficultyHitObject current)
        {
            double difficulty = FlashlightEvaluator.EvaluateDifficultyOf(current, Mods);

            if (Mods.Any(m => m is OsuModTouchDevice))
                difficulty = Math.Pow(difficulty, 0.9);

            if (Mods.Any(m => m is OsuModMagnetised))
            {
                float magnetisedStrength = Mods.OfType<OsuModMagnetised>().First().AttractionStrength.Value;
                difficulty *= 1.0 - magnetisedStrength;
            }

            if (Mods.Any(m => m is OsuModDeflate))
            {
                float deflateInitialScale = Mods.OfType<OsuModDeflate>().First().StartScale.Value;
                difficulty *= Math.Clamp(DifficultyCalculationUtils.ReverseLerp(deflateInitialScale, 11, 1), 0.1, 1);
            }

            if (Mods.Any(m => m is OsuModRelax))
                difficulty *= 0.7;

            if (Mods.Any(m => m is OsuModAutopilot))
                difficulty *= 0.4;

            difficulty *= 0.99 + Math.Pow(Math.Max(0, ((OsuDifficultyHitObject)current).OverallDifficulty), 2) / 5500;

            return difficulty;
        }

        public override double DifficultyValue()
        {
            double sum = GetCurrentStrainPeaks().Sum();

            // Account for shorter maps having a higher ratio of 0 combo/100 combo flashlight radius.
            sum *= Math.Sqrt(0.7 + 0.1 * Math.Min(1.0, totalObjects / 200.0) +
                             (totalObjects > 200 ? 0.2 * Math.Min(1.0, (totalObjects - 200) / 200.0) : 0.0));

            return sum;
        }

        public static double DifficultyToPerformance(double difficulty) => 25 * Math.Pow(difficulty, 2);
    }
}
