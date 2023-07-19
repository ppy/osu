// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Evaluators;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Utils;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Represents the skill required to press keys with regards to keeping up with the speed at which objects need to be hit.
    /// </summary>
    public class Speed : OsuStrainSkill
    {
        private const double skill_multiplier = 1375;
        private const double strain_decay_base = 0.3;

        private double currentRhythm;

        private readonly DecayingValue currentStrain = DecayingValue.FromDecayMultiplierPerSecond(strain_decay_base);

        protected override int ReducedSectionCount => 5;
        protected override double DifficultyMultiplier => 1.04;

        private readonly List<double> objectStrains = new List<double>();

        public Speed(Mod[] mods)
            : base(mods)
        {
        }

        protected override double StrainAtTime(double time) => currentRhythm * currentStrain.ValueAtTime(time);

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            currentStrain.IncrementValueAtTime(current.StartTime, SpeedEvaluator.EvaluateDifficultyOf(current) * skill_multiplier);

            currentRhythm = RhythmEvaluator.EvaluateDifficultyOf(current);

            double totalStrain = currentStrain.Value * currentRhythm;

            objectStrains.Add(totalStrain);

            return totalStrain;
        }

        public double RelevantNoteCount()
        {
            if (objectStrains.Count == 0)
                return 0;

            double maxStrain = objectStrains.Max();

            if (maxStrain == 0)
                return 0;

            return objectStrains.Sum(strain => 1.0 / (1.0 + Math.Exp(-(strain / maxStrain * 12.0 - 6.0))));
        }
    }
}
