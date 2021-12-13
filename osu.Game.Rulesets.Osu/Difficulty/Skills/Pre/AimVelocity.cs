// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills.Pre
{
    public class AimVelocity : PreStrainSkill
    {
        private readonly bool withSliders;

        protected override double StrainDecayBase => 0.0;

        protected override double SkillMultiplier => 1.0;

        public AimVelocity(Mod[] mods, bool withSliders)
            : base(mods)
        {
            this.withSliders = withSliders;
        }

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner || (Previous.Count > 0 && Previous[0].BaseObject is Spinner))
                return 0;

            var osuCurrObj = (OsuDifficultyHitObject)current;
            var osuLastObj = Previous.Count > 0 ? (OsuDifficultyHitObject)Previous[0] : null;

            double currVelocity = osuCurrObj.LazyJumpDistance / osuCurrObj.StrainTime;

            // But if the last object is a slider, then we extend the travel velocity through the slider into the current object.
            if (osuLastObj?.BaseObject is Slider && withSliders)
            {
                double travelVelocity = osuLastObj.TravelDistance / osuLastObj.TravelTime; // calculate the slider velocity from slider head to slider end.
                double movementVelocity = osuCurrObj.MinimumJumpDistance / osuCurrObj.MinimumJumpTime; // calculate the movement velocity from slider end to current object

                currVelocity = Math.Max(currVelocity, movementVelocity + travelVelocity); // take the larger total combined velocity.
            }

            return currVelocity;
        }
    }
}
