// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Difficulty.Utils;
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

        public Flashlight(Mod[] mods)
            : base(mods)
        {
            hasHiddenMod = mods.Any(m => m is OsuModHidden);
        }

        private const double skill_multiplier = 0.052;

        private const double strain_decay_base = 0.15;

        private readonly DecayingValue currentStrain = DecayingValue.FromDecayMultiplierPerSecond(strain_decay_base);

        protected override double StrainAtTime(double time) => currentStrain.ValueAtTime(time);

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            double difficulty = FlashlightEvaluator.EvaluateDifficultyOf(current, hasHiddenMod) * skill_multiplier;
            return currentStrain.IncrementValueAtTime(current.StartTime, difficulty);
        }

        public override double DifficultyValue() => GetCurrentStrainPeaks().Sum() * OsuStrainSkill.DEFAULT_DIFFICULTY_MULTIPLIER;
    }
}
