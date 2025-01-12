// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Evaluators;
using osu.Game.Rulesets.Osu.Mods;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Represents the skill required to memorise and hit every object in a map with the Flashlight mod enabled.
    /// </summary>
    public class Flashlight : StrainSkill
    {
        private readonly bool hasHiddenMod;
        private readonly OsuModFlashlight osuModFlashlight;

        public Flashlight(Mod[] mods)
            : base(mods)
        {
            hasHiddenMod = mods.Any(m => m is OsuModHidden);
            osuModFlashlight = (OsuModFlashlight)mods.Single(m => m is OsuModFlashlight);
        }

        private double skillMultiplier => 0.0727;
        private double strainDecayBase => 0.15;
        protected override double DecayWeight => 0.99984;

        private double currentStrain;

        private double strainDecay(double ms) => Math.Pow(strainDecayBase, ms / 1000);

        protected override double CalculateInitialStrain(double time, DifficultyHitObject current) => currentStrain * strainDecay(time - current.Previous(0).StartTime);

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            currentStrain *= strainDecay(current.DeltaTime);
            currentStrain += FlashlightEvaluator.EvaluateDifficultyOf(current, hasHiddenMod, osuModFlashlight) * skillMultiplier;

            return currentStrain;
        }

        public static double DifficultyToPerformance(double difficulty) => 28.727 * Math.Pow(difficulty, 2);
    }
}
