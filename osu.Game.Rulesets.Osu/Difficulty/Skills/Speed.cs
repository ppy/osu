// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Evaluators;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using System.Linq;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Represents the skill required to press keys with regards to keeping up with the speed at which objects need to be hit.
    /// </summary>
    public class Speed : OsuStrainSkill
    {
        public Speed(Mod[] mods)
            : base(mods)
        {
        }

        public static double MaxDifficulty; //Max difficulty of a single object

        public static double SumDifficulty; //Summed difficulty of a single object
        protected override int ReducedSectionCount => 5;
        private double skillMultiplier => 1.430;
        private double strainDecayBase => 0.3;

        private double currentStrain;
        private double currentRhythm;

        private double maxDifficulty;

        private double sumDifficulty;

        private double strainDecay(double ms) => Math.Pow(strainDecayBase, ms / 1000);

        protected override double CalculateInitialStrain(double time, DifficultyHitObject current) => (currentStrain * currentRhythm) * strainDecay(time - current.Previous(0).StartTime);

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            currentStrain *= strainDecay(((OsuDifficultyHitObject)current).StrainTime);

            double currentHitObjectStrain = SpeedEvaluator.EvaluateDifficultyOf(current) * skillMultiplier;

            currentStrain += currentHitObjectStrain;

            currentRhythm = RhythmEvaluator.EvaluateDifficultyOf(current);

            double totalStrain = currentStrain * currentRhythm;

            sumDifficulty += currentHitObjectStrain * currentRhythm;

            if (maxDifficulty < currentHitObjectStrain)
                maxDifficulty = currentHitObjectStrain;

            if (current.Next(1) is null)
            {
                MaxDifficulty = maxDifficulty;
                SumDifficulty = sumDifficulty;
            }

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
    }
}
