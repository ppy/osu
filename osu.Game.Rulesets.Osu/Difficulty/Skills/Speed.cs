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
        private const double rhythm_multiplier = 0.75;
        private const int history_time_max = 5000; // 5 seconds of calculatingRhythmBonus max.
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

        /// <summary>
        /// Calculates a rhythm multiplier for the difficulty of the tap associated with historic data of the current <see cref="OsuDifficultyHitObject"/>.
        /// </summary>
        private double calculateRhythmBonus(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner)
                return 0;

            int previousIslandSize = 0;

            double rhythmComplexitySum = 0;
            int islandSize = 1;
            double startRatio = 0; // store the ratio of the current start of an island to buff for tighter rhythms

            bool firstDeltaSwitch = false;

            for (int i = Previous.Count - 2; i > 0; i--)
            {
                OsuDifficultyHitObject currObj = (OsuDifficultyHitObject)Previous[i - 1];
                OsuDifficultyHitObject prevObj = (OsuDifficultyHitObject)Previous[i];
                OsuDifficultyHitObject lastObj = (OsuDifficultyHitObject)Previous[i + 1];

                double currHistoricalDecay = Math.Max(0, (history_time_max - (current.StartTime - currObj.StartTime))) / history_time_max; // scales note 0 to 1 from history to now

                if (currHistoricalDecay != 0)
                {
                    currHistoricalDecay = Math.Min((double)(Previous.Count - i) / Previous.Count, currHistoricalDecay); // either we're limited by time or limited by object count.

                    double currDelta = currObj.StrainTime;
                    double prevDelta = prevObj.StrainTime;
                    double lastDelta = lastObj.StrainTime;
                    double currRatio = 1.0 + 6.0 * Math.Min(0.5, Math.Pow(Math.Sin(Math.PI / (Math.Min(prevDelta, currDelta) / Math.Max(prevDelta, currDelta))), 2)); // fancy function to calculate rhythmbonuses.

                    double windowPenalty = Math.Min(1, Math.Max(0, Math.Abs(prevDelta - currDelta) - greatWindow * 0.6) / (greatWindow * 0.6));

                    windowPenalty = Math.Min(1, windowPenalty);

                    double effectiveRatio = windowPenalty * currRatio;

                    if (firstDeltaSwitch)
                    {
                        if (!(prevDelta > 1.25 * currDelta || prevDelta * 1.25 < currDelta))
                        {
                            if (islandSize < 7)
                                islandSize++; // island is still progressing, count size.
                        }
                        else
                        {
                            if (Previous[i - 1].BaseObject is Slider) // bpm change is into slider, this is easy acc window
                                effectiveRatio *= 0.125;

                            if (Previous[i].BaseObject is Slider) // bpm change was from a slider, this is easier typically than circle -> circle
                                effectiveRatio *= 0.25;

                            if (previousIslandSize == islandSize) // repeated island size (ex: triplet -> triplet)
                                effectiveRatio *= 0.25;

                            if (previousIslandSize % 2 == islandSize % 2) // repeated island polartiy (2 -> 4, 3 -> 5)
                                effectiveRatio *= 0.50;

                            if (lastDelta > prevDelta + 10 && prevDelta > currDelta + 10) // previous increase happened a note ago, 1/1->1/2-1/4, dont want to buff this.
                                effectiveRatio *= 0.125;

                            rhythmComplexitySum += Math.Sqrt(effectiveRatio * startRatio) * currHistoricalDecay * Math.Sqrt(4 + islandSize) / 2 * Math.Sqrt(4 + previousIslandSize) / 2;

                            startRatio = effectiveRatio;

                            previousIslandSize = islandSize; // log the last island size.

                            if (prevDelta * 1.25 < currDelta) // we're slowing down, stop counting
                                firstDeltaSwitch = false; // if we're speeding up, this stays true and  we keep counting island size.

                            islandSize = 1;
                        }
                    }
                    else if (prevDelta > 1.25 * currDelta) // we want to be speeding up.
                    {
                        // Begin counting island until we change speed again.
                        firstDeltaSwitch = true;
                        startRatio = effectiveRatio;
                        islandSize = 1;
                    }
                }
            }

            return Math.Sqrt(4 + rhythmComplexitySum * rhythm_multiplier) / 2; //produces multiplier that can be applied to strain. range [1, infinity) (not really though)
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

            double distance = Math.Min(single_spacing_threshold, osuCurrObj.TravelDistance + osuCurrObj.JumpDistance);

            return (speedBonus + speedBonus * Math.Pow(distance / single_spacing_threshold, 3.5)) / strainTime;
        }

        private double strainDecay(double ms) => Math.Pow(strainDecayBase, ms / 1000);

        protected override double CalculateInitialStrain(double time) => (currentStrain * currentRhythm) * strainDecay(time - Previous[0].StartTime);

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            currentStrain *= strainDecay(current.DeltaTime);
            currentStrain += strainValueOf(current) * skillMultiplier;

            currentRhythm = calculateRhythmBonus(current);

            return currentStrain * currentRhythm;
        }
    }
}
