// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;
using osu.Framework.Utils;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Represents the skill required to press keys with regards to keeping up with the speed at which objects need to be hit.
    /// </summary>
    public class Speed : OsuStrainSkill
    {
        private const double single_spacing_threshold = 125;
        private const double min_speed_bonus = 75; // ~200BPM
        private const double speed_balancing_factor = 40;

        private double skillMultiplier => 1375;
        private double strainDecayBase => 0.3;

        private double currentStrain;
        private double currentRhythm;

        protected override int ReducedSectionCount => 5;
        protected override double DifficultyMultiplier => 1.04;
        protected override int HistoryLength => 32;

        private readonly double greatWindow;

        public Speed(Mod[] mods, double hitWindowGreat)
            : base(mods)
        {
            greatWindow = hitWindowGreat;
        }

        private double strainValueOf(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner)
                return 0;

            // derive strainTime for calculation
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

            // derive speedBonus for calculation
            double speedBonus = 1.0;

            if (strainTime < min_speed_bonus)
                speedBonus = 1 + 0.75 * Math.Pow((min_speed_bonus - strainTime) / speed_balancing_factor, 2);

            double travelDistance = osuPrevObj?.TravelDistance ?? 0;
            double distance = Math.Min(single_spacing_threshold, travelDistance + osuCurrObj.MinimumJumpDistance);

            return (speedBonus + speedBonus * Math.Pow(distance / single_spacing_threshold, 3.5)) / strainTime;
        }

        private double strainDecay(double ms) => Math.Pow(strainDecayBase, ms / 1000);

        protected override double CalculateInitialStrain(double time) => (currentStrain * currentRhythm) * strainDecay(time - Previous[0].StartTime);

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            currentStrain *= strainDecay(current.DeltaTime);
            currentStrain += strainValueOf(current) * skillMultiplier;

            if (current is OsuDifficultyHitObject osuCurrObj)
                currentRhythm = osuCurrObj.RhythmDifficulty ?? 1.0;

            return currentStrain * currentRhythm;
        }
    }
}
