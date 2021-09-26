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

        private const double angle_bonus_begin = 5 * Math.PI / 6;
        private const double pi_over_4 = Math.PI / 4;
        private const double pi_over_2 = Math.PI / 2;

        private const double rhythm_multiplier = 2.0;
        private const int history_time_max = 5000; // 5 seconds of calculatingRhythmBonus max.

        private double skillMultiplier => 1375;
        private double strainDecayBase => 0.3;

        private double currentTapStrain = 1;
        private double currentMovementStrain = 1;
        private double currentRhythm = 1;

        protected override int ReducedSectionCount => 5;
        protected override double DifficultyMultiplier => 1.04;

        private const double min_speed_bonus = 75; // ~200BPM
        private const double max_speed_bonus = 45;
        private const double speed_balancing_factor = 40;

        protected override int HistoryLength => 32;

        private readonly double greatWindow;

        public Speed(Mod[] mods, double hitWindowGreat)
            : base(mods)
        {
            greatWindow = hitWindowGreat;
        }

        /// <summary>
        /// Calculates a rhythm multiplier for the difficulty of the tap associated with historic data of the current <see cref="OsuDifficultyHitObject"/>.
        /// </summary>
        private double calculateRhythmBonus(DifficultyHitObject current, double greatWindowFull)
        {
            if (current.BaseObject is Spinner)
                return 0;

            int previousIslandSize = -1;
            double rhythmComplexitySum = 0;
            int islandSize = 0;

            bool firstDeltaSwitch = false;

            for (int i = Previous.Count - 2; i > 0; i--)
            {
                DifficultyHitObject currObj = Previous[i - 1];
                DifficultyHitObject prevObj = Previous[i];
                DifficultyHitObject prevPrevObj = Previous[i + 1];

                double currHistoricalDecay = Math.Max(0, (history_time_max - (current.StartTime - currObj.StartTime))) / history_time_max; // scales note 0 to 1 from history to now

                if (currHistoricalDecay != 0)
                {
                    currHistoricalDecay = Math.Min(currHistoricalDecay, (double)(Previous.Count - i) / Previous.Count); // either we're limited by time or limited by object count.

                    double currDelta = Math.Max(25, currObj.DeltaTime);
                    double prevDelta = Math.Max(25, prevObj.DeltaTime);
                    double prevPrevDelta = ((OsuDifficultyHitObject)prevPrevObj).StrainTime;
                    double effectiveRatio = Math.Min(prevDelta, currDelta) / Math.Max(prevDelta, currDelta);

                    if (effectiveRatio > 0.5)
                        effectiveRatio = 0.5 + (effectiveRatio - 0.5) * 5; // large buff for 1/3 -> 1/4 type transitions.

                    effectiveRatio *= currHistoricalDecay; // scale with time

                    if (firstDeltaSwitch)
                    {
                        if (Precision.AlmostEquals(prevDelta, currDelta, 15))
                        {
                            islandSize++; // island is still progressing, count size.
                        }
                        else
                        {


                            if (islandSize > 6)
                                islandSize = 6;

                            if (Previous[i - 1].BaseObject is Slider) // bpm change is into slider, this is easy acc window
                                effectiveRatio *= 0.25;

                            if (Previous[i].BaseObject is Slider) // bpm change was from a slider, this is easier typically than circle -> circle
                                effectiveRatio *= 0.5;

                            if (previousIslandSize == islandSize) // repeated island size (ex: triplet -> triplet)
                                effectiveRatio *= 0.25;

                            if (prevPrevDelta > prevDelta + 10 && prevDelta > currDelta + 10) // previous increase happened a note ago, 1/1->1/2-1/4, dont want to buff this.
                                effectiveRatio *= 0.125;

                            rhythmComplexitySum += effectiveRatio;

                            previousIslandSize = islandSize; // log the last island size.

                            if (prevDelta * 1.25 < currDelta) // we're slowing down, stop counting
                                firstDeltaSwitch = false; // if we're speeding up, this stays true and  we keep counting island size.

                            islandSize = 0;
                        }
                    }
                    else if (prevDelta > 1.25 * currDelta) // we want to be speeding up.
                    {
                        // Begin counting island until we change speed again.
                        firstDeltaSwitch = true;
                        islandSize = 0;
                    }
                }
            }

            return Math.Sqrt(4 + rhythmComplexitySum * rhythm_multiplier * Math.Sqrt(52 / greatWindowFull)) / 2; //produces multiplier that can be applied to strain. range [1, infinity) (not really though)
        }

        private double tapStrainOf(DifficultyHitObject current, double speedBonus, double strainTime)
        {
            if (current.BaseObject is Spinner)
                return 0;

            return speedBonus / strainTime;
        }

        private double movementStrainOf(DifficultyHitObject current, double speedBonus, double strainTime)
        {
            if (current.BaseObject is Spinner)
                return 0;

            var osuCurrObj = (OsuDifficultyHitObject)current;

            double distance = Math.Min(single_spacing_threshold, osuCurrObj.TravelDistance + osuCurrObj.JumpDistance);
            double angleBonus = 1.0;

            if (osuCurrObj.Angle != null)
            {
                double angle = osuCurrObj.Angle.Value;

                if (angle < pi_over_2)
                    angleBonus = 1.25;
                else if (angle < angle_bonus_begin)
                    angleBonus = 1 + Math.Pow(Math.Sin(1.5 * (angle_bonus_begin - angle)), 2) / 4;
            }

            return (angleBonus * speedBonus * Math.Pow(distance / single_spacing_threshold, 3.5)) / strainTime;
        }

        private double strainDecay(double ms) => Math.Pow(strainDecayBase, ms / 1000);

        protected override double CalculateInitialStrain(double time) => (currentMovementStrain + currentTapStrain * currentRhythm) * strainDecay(time - Previous[0].StartTime);

        protected override double StrainValueAt(DifficultyHitObject current)
        {
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

            currentRhythm = calculateRhythmBonus(current, greatWindowFull);

            double decay = strainDecay(current.DeltaTime);

            currentTapStrain *= decay;
            currentTapStrain += tapStrainOf(current, speedBonus, strainTime) * skillMultiplier;

            currentMovementStrain *= decay;
            currentMovementStrain += movementStrainOf(current, speedBonus, strainTime) * skillMultiplier;

            return currentMovementStrain + currentTapStrain * currentRhythm;
        }
    }
}
