// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Evaluators;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Represents the skill required to press keys with regards to keeping up with the speed at which objects need to be hit.
    /// </summary>
    public class Speed : OsuStrainSkill
    {
        protected double SkillMultiplier => 1375;
        protected override double StrainDecayBase => 0.3;

        protected double CurrentStrain;
        protected double CurrentRhythm;

        protected override int ReducedSectionCount => 5;
        protected override double DifficultyMultiplier => 1.04;

        private readonly List<double> objectStrains = new List<double>();

        public Speed(Mod[] mods)
            : base(mods)
        {
        }

        protected override double CalculateInitialStrain(double time, DifficultyHitObject current) => (CurrentStrain * CurrentRhythm) * StrainDecay(time - current.Previous(0).StartTime);

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            OsuDifficultyHitObject currODHO = (OsuDifficultyHitObject)current;

            CurrentStrain *= StrainDecay(currODHO.StrainTime);
            CurrentStrain += SpeedEvaluator.EvaluateDifficultyOf(current) * SkillMultiplier;

            CurrentRhythm = currODHO.RhythmDifficulty;
            double totalStrain = CurrentStrain * CurrentRhythm;

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
