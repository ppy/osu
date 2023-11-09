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
        private double skillMultiplier => 1375;
        private double strainDecayBase => 0.3;

        private double currentStrain;
        private double currentRhythm;

        private double currentStrainNoDistance;

        protected override int ReducedSectionCount => 5;
        protected override double DifficultyMultiplier => 1.04;

        private readonly List<double> objectStrainsNoDistance = new List<double>();

        public Speed(Mod[] mods)
            : base(mods)
        {
        }

        private double strainDecay(double ms) => Math.Pow(strainDecayBase, ms / 1000);

        protected override double CalculateInitialStrain(double time, DifficultyHitObject current) => (currentStrain * currentRhythm) * strainDecay(time - current.Previous(0).StartTime);

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            currentStrain *= strainDecay(((OsuDifficultyHitObject)current).StrainTime);
            currentStrain += SpeedEvaluator.EvaluateDifficultyOf(current) * skillMultiplier;

            // Disregard distance when computing the number of speed notes.
            double travelDistance = current.Index > 0 ? ((OsuDifficultyHitObject)current.Previous(0)).TravelDistance : 0;
            double distance = Math.Min(125, travelDistance + ((OsuDifficultyHitObject)current).MinimumJumpDistance);

            currentStrainNoDistance *= strainDecay(((OsuDifficultyHitObject)current).StrainTime);
            currentStrainNoDistance += SpeedEvaluator.EvaluateDifficultyOf(current) / (1 + Math.Pow(distance / 125, 3.5)) * skillMultiplier;

            currentRhythm = RhythmEvaluator.EvaluateDifficultyOf(current);

            double totalStrain = currentStrain * currentRhythm;
            double totalStrainNoDistance = currentStrainNoDistance * currentRhythm;

            objectStrainsNoDistance.Add(totalStrainNoDistance);

            return totalStrain;
        }

        public double RelevantNoteCount()
        {
            if (objectStrainsNoDistance.Count == 0)
                return 0;

            double maxStrain = objectStrainsNoDistance.Max();

            if (maxStrain == 0)
                return 0;

            return objectStrainsNoDistance.Sum(strain => strain / maxStrain);
        }
    }
}
