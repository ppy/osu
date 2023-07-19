// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Evaluators;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Represents the skill required to correctly aim at every object in the map with a uniform CircleSize and normalized distances.
    /// </summary>
    public class Aim : OsuStrainSkill
    {
        public Aim(Mod[] mods, bool withSliders)
            : base(mods)
        {
            this.withSliders = withSliders;
        }

        private readonly bool withSliders;

        private readonly DecayingValue currentStrain = DecayingValue.FromDecayMultiplierPerSecond(strain_decay_base);

        private const double skill_multiplier = 23.55;
        private const double strain_decay_base = 0.15;

        protected override double StrainAtTime(double time) => currentStrain.ValueAtTime(time);

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            return currentStrain.IncrementValueAtTime(
                current.StartTime,
                AimEvaluator.EvaluateDifficultyOf(current, withSliders) * skill_multiplier
            );
        }
    }
}
