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
            : base(mods, strainDecayBase: 0.15)
        {
        }

        private const double skill_multiplier = 0.15;
        protected override double DecayWeight => 1.0;
        protected override int HistoryLength => 10; // Look back for 10 notes is added for the sake of flashlight calculations.
        protected override void Process(DifficultyHitObject current)
        {
            var osuCurrent = (OsuDifficultyHitObject)current;
            osuCurrent.flashlight = new HitObjectAttributes(current, this);
        }

        public struct HitObjectAttributes
        {
            public double SmallDistNerf;
            public double Result;
            public double CumulativeStrainTime;
            public double StackNerf;

            public double Strain;
            public double CumulativeStrain;

            public HitObjectAttributes(DifficultyHitObject current, Flashlight state) : this()
            {
                if (current.BaseObject is Spinner)
                    return;

                var osuCurrent = (OsuDifficultyHitObject)current;
                var osuHitObject = (OsuHitObject)(osuCurrent.BaseObject);

                double scalingFactor = 52.0 / osuHitObject.Radius;
                SmallDistNerf = 1.0;
                CumulativeStrainTime = 0.0;

                Result = 0.0;

                for (int i = 0; i < state.Previous.Count; i++)
                {
                    var osuPrevious = (OsuDifficultyHitObject)state.Previous[i];
                    var osuPreviousHitObject = (OsuHitObject)(osuPrevious.BaseObject);

                    if (!(osuPrevious.BaseObject is Spinner))
                    {
                        double jumpDistance = (osuHitObject.StackedPosition - osuPreviousHitObject.EndPosition).Length;

                        CumulativeStrainTime += osuPrevious.StrainTime;

                        // We want to nerf objects that can be easily seen within the Flashlight circle radius.
                        if (i == 0)
                            SmallDistNerf = Math.Min(1.0, jumpDistance / 75.0);

                        // We also want to nerf stacks so that only the first object of the stack is accounted for.
                        double stackNerf = Math.Min(1.0, (osuPrevious.JumpDistance / scalingFactor) / 25.0);

                        Result += Math.Pow(0.8, i) * stackNerf * scalingFactor * jumpDistance / CumulativeStrainTime;
                    }
                }

                Strain = skill_multiplier * Math.Pow(SmallDistNerf * Result, 2.0);
                CumulativeStrain = state.AddStrain(current.StartTime, Strain);
            }
        }
    }
}
