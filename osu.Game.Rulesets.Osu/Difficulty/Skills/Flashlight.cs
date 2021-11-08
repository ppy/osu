// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Represents the skill required to memorise and hit every object in a map with the Flashlight mod enabled.
    /// </summary>
    public class Flashlight : OsuStrainSkill
    {
        public Flashlight(Mod[] mods)
            : base(mods)
        {
        }

        private double skillMultiplier => 0.15;
        private double strainDecayBase => 0.15;
        protected override double DecayWeight => 1.0;
        protected override int HistoryLength => 10; // Look back for 10 notes is added for the sake of flashlight calculations.

        private double currentStrain;

        private double strainValueOf(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner)
                return 0;

            var osuCurrent = (OsuDifficultyHitObject)current;
            var osuHitObject = (OsuHitObject)(osuCurrent.BaseObject);

            double scalingFactor = 52.0 / osuHitObject.Radius;
            double smallDistNerf = 1.0;
            double cumulativeStrainTime = 0.0;

            double result = 0.0;

            for (int i = 0; i < Previous.Count; i++)
            {
                var osuPrevious = (OsuDifficultyHitObject)Previous[i];
                var osuPreviousHitObject = (OsuHitObject)(osuPrevious.BaseObject);

                if (!(osuPrevious.BaseObject is Spinner))
                {
                    double jumpDistance = (osuHitObject.StackedPosition - osuPreviousHitObject.EndPosition).Length;

                    cumulativeStrainTime += osuPrevious.StrainTime;

                    // We want to nerf objects that can be easily seen within the Flashlight circle radius.
                    if (i == 0)
                        smallDistNerf = Math.Min(1.0, jumpDistance / 75.0);

                    // We also want to nerf stacks so that only the first object of the stack is accounted for.
                    double stackNerf = Math.Min(1.0, (osuPrevious.JumpDistance / scalingFactor) / 25.0);

                    result += Math.Pow(0.8, i) * stackNerf * scalingFactor * jumpDistance / cumulativeStrainTime;
                }
            }

            return Math.Pow(smallDistNerf * result, 2.0);
        }

        private double strainDecay(double ms) => Math.Pow(strainDecayBase, ms / 1000);

        protected override double CalculateInitialStrain(double time) => currentStrain * strainDecay(time - Previous[0].StartTime);

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            currentStrain *= strainDecay(current.DeltaTime);
            currentStrain += strainValueOf(current) * skillMultiplier;

            return currentStrain;
        }
    }
}
