using System;
using System.Collections.Generic;
using System.Text;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills.Pre
{
    public class AimVelocity : PreStrainSkill
    {
        protected override double SkillMultiplier => 1.0;

        protected override double StrainDecayBase => 0.0;

        private bool withSliders;

        public AimVelocity(Mod[] mods, bool withSliders) : base(mods)
        {
            this.withSliders = withSliders;
        }

        protected override double StrainValueOf(int index, DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner || Previous.Count == 0)
                return 0;

            var osuCurrObj = (OsuDifficultyHitObject)current;
            var osuLastObj = (OsuDifficultyHitObject)Previous[0];

            double currVelocity = osuCurrObj.LazyJumpDistance / osuCurrObj.StrainTime;

            // But if the last object is a slider, then we extend the travel velocity through the slider into the current object.
            if (osuLastObj.BaseObject is Slider && withSliders)
            {
                double travelVelocity = osuLastObj.TravelDistance / osuLastObj.TravelTime; // calculate the slider velocity from slider head to slider end.
                double movementVelocity = osuCurrObj.MinimumJumpDistance / osuCurrObj.MinimumJumpTime; // calculate the movement velocity from slider end to current object

                currVelocity = Math.Max(currVelocity, movementVelocity + travelVelocity); // take the larger total combined velocity.
            }

            return currVelocity;
        }
    }
}
