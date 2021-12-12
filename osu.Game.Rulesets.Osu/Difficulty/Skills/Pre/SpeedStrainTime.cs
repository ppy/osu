// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Utils;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills.Pre
{
    public class SpeedStrainTime : PreStrainSkill
    {
        private readonly double greatWindow;

        protected override double StrainDecayBase => 0.0;

        protected override double SkillMultiplier => 1.0;

        public SpeedStrainTime(Mod[] mods, double greatWindow)
            : base(mods)
        {
            this.greatWindow = greatWindow;
        }

        protected override double StrainValueAt(int index, DifficultyHitObject current)
        {
            var osuCurrObj = (OsuDifficultyHitObject)current;
            var osuPrevObj = Previous.Count > 0 ? (OsuDifficultyHitObject)Previous[0] : null;

            double strainTime = osuCurrObj.StrainTime;
            double greatWindowFull = greatWindow * 2;
            double speedWindowRatio = strainTime / greatWindowFull;

            // Aim to nerf cheesy rhythms (Very fast consecutive doubles with large deltatimes between)
            if (osuPrevObj != null && strainTime < greatWindowFull && osuPrevObj.StrainTime > strainTime)
                strainTime = Interpolation.Lerp(osuPrevObj.StrainTime, strainTime, speedWindowRatio);

            // Cap deltatime to the OD 300 hitwindow.
            // 0.93 is derived from making sure 260bpm OD8 streams aren't nerfed harshly, whilst 0.92 limits the effect of the cap.
            strainTime /= Math.Clamp((strainTime / greatWindowFull) / 0.93, 0.92, 1);

            return strainTime;
        }
    }
}
