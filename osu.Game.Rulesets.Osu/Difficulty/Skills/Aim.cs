// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Represents the skill required to correctly aim at every object in the map with a uniform CircleSize and normalized distances.
    /// </summary>
    public class Aim : OsuSkill
    {
        protected override double StarsPerDouble => 1.2;
        protected override int HistoryLength => 4;

        private Tapping tappingSkill;

        public Aim(Mod[] mods)
            : base(mods)
        {
        }
        private double strainValueAt(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner)
                return 0;

            var osuCurrent = (OsuDifficultyHitObject)current;

            double tappingStrain = tappingSkill.TappingStrain;
            double flowProb = osuCurrent.FlowProbability;
            double snapProb = osuCurrent.SnapProbability;

            return osuCurrent.DistanceVector.Length / osuCurrent.StrainTime;
        }

        public void SetTappingSkill(Tapping tapping) => tappingSkill = tapping;

        protected override void Process(DifficultyHitObject current)
        {
            AddStrain(strainValueAt(current));
        }
    }
}
