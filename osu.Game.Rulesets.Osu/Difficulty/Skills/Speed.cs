// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Evaluators;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;
using System.Linq;
using osu.Game.Rulesets.Osu.Difficulty.Utils;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Represents the skill required to press keys with regards to keeping up with the speed at which objects need to be hit.
    /// </summary>
    public class Speed : OsuStrainSkill
    {
        protected override double SkillMultiplier => 1.46;
        protected override double StrainDecayBase => 0.3;

        private double currentRhythm;

        private readonly List<double> sliderStrains = new List<double>();

        protected override int ReducedSectionCount => 5;

        public Speed(Mod[] mods)
            : base(mods)
        {
        }

        protected override double CalculateInitialStrain(double time, DifficultyHitObject current) => (CurrentStrain * currentRhythm) * StrainDecay(time - current.Previous(0).StartTime);

        protected override double StrainValueOf(DifficultyHitObject current) => SpeedEvaluator.EvaluateDifficultyOf(current, Mods);

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            CurrentStrain *= StrainDecay(((OsuDifficultyHitObject)current).StrainTime);
            CurrentStrain += StrainValueOf(current) * SkillMultiplier;

            currentRhythm = RhythmEvaluator.EvaluateDifficultyOf(current);

            double totalStrain = CurrentStrain * currentRhythm;

            if (current.BaseObject is Slider)
                sliderStrains.Add(totalStrain);

            return totalStrain;
        }

        public double RelevantNoteCount()
        {
            if (ObjectStrains.Count == 0)
                return 0;

            double maxStrain = ObjectStrains.Max();
            if (maxStrain == 0)
                return 0;

            return ObjectStrains.Sum(strain => 1.0 / (1.0 + Math.Exp(-(strain / maxStrain * 12.0 - 6.0))));
        }

        public double CountTopWeightedSliders() => OsuStrainUtils.CountTopWeightedSliders(sliderStrains, DifficultyValue());
    }
}
