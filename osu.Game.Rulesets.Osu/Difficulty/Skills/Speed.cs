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
    /// Represents the skill required to press keys with regards to keeping up with the speed at which objects need to be hit.
    /// </summary>
    public class Speed : OsuStrainSkill
    {
        private const double single_spacing_threshold = 125;

        private const double angle_bonus_begin = 5 * Math.PI / 6;
        private const double pi_over_4 = Math.PI / 4;
        private const double pi_over_2 = Math.PI / 2;

        private const double rhythmMultiplier = 1.5;

        private double skillMultiplier => 1400;
        private double strainDecayBase => 0.3;
        private double difficultyMultiplier => 1.04;

        private double currentTapStrain = 1;
        private double currentMovementStrain = 1;

        protected override int ReducedSectionCount => 5;

        private const double min_speed_bonus = 75; // ~200BPM
        private const double max_speed_bonus = 45; // ~330BPM
        private const double speed_balancing_factor = 40;

        protected override int HistoryLength => 32;

        private const int HistoryTimeMax = 3000; // 4 seconds of calculatingRhythmBonus max.

        public Speed(Mod[] mods)
            : base(mods)
        {
        }

        private bool isRatioEqual(double ratio, double a, double b)
        {
            return a + 15 > ratio * b && a - 15 < ratio * b;
        }

        /// <summary>
        /// Calculates a rhythm multiplier for the difficulty of the tap associated with historic data of the current <see cref="OsuDifficultyHitObject"/>.
        /// </summary>
        private double calculateRhythmBonus(double startTime)
        {
            // {doubles, triplets, quads, quints, 6-tuplets, 7 Tuplets, greater}
            int previousIslandSize = -1;
            double[] islandTimes = {0, 0, 0, 0, 0, 0, 0};
            int islandSize = 0;

            bool firstDeltaSwitch = false;

            for (int i = Previous.Count - 1; i > 0; i--)
            {
                double currDelta = ((OsuDifficultyHitObject)Previous[i - 1]).StrainTime;
                double prevDelta = ((OsuDifficultyHitObject)Previous[i]).StrainTime;
                double effectiveRatio = Math.Min(prevDelta, currDelta) / Math.Max(prevDelta, currDelta);

                effectiveRatio *= Math.Sqrt(100 / ((currDelta + prevDelta) / 2)); // scale with bpm.

                if (effectiveRatio > 0.5)
                    effectiveRatio = 0.5 + (effectiveRatio - 0.5) * 5; // extra buff for 1/3 -> 1/4 etc transitions.

                double currHistoricalDecay = Math.Max(0, (HistoryTimeMax - (startTime - Previous[i - 1].StartTime))) / HistoryTimeMax;

                if (firstDeltaSwitch)
                {
                    if (isRatioEqual(1.0, prevDelta, currDelta))
                    {
                        islandSize++; // island is still progressing, count size.
                    }

                    else
                    {
                        if (islandSize > 6)
                            islandSize = 6;

                        if (Previous[i - 1].BaseObject is Slider) // bpm change is into slider, this is easy acc window
                            effectiveRatio *= 0.5;

                        if (Previous[i].BaseObject is Slider) // bpm change was from a slider, this is easier typically than circle -> circle
                            effectiveRatio *= 0.75;

                        if (previousIslandSize == islandSize) // repeated island size (ex: triplet -> triplet)
                            effectiveRatio *= 0.5;

                        islandTimes[islandSize] = islandTimes[islandSize] + effectiveRatio * currHistoricalDecay;

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

            double rhythmComplexitySum = 0.0;

            for (int i = 0; i < islandTimes.Length; i++)
            {
                rhythmComplexitySum += islandTimes[i]; // sum the total amount of rhythm variance
            }

// Console.WriteLine(Math.Sqrt(4 + rhythmComplexitySum * rhythmMultiplier) / 2);

            return Math.Sqrt(4 + rhythmComplexitySum * rhythmMultiplier) / 2;
        }

        private void setStrainValues(DifficultyHitObject current)
        {
            // if (current.BaseObject is Spinner)
            // {
            //     currentTapStrain *= strainDecay(current.DeltaTime);
            //
            //     currentMovementStrain *= strainDecay(current.DeltaTime);
            //
            //     return;
            // }

            var osuCurrent = (OsuDifficultyHitObject)current;

            double distance = Math.Min(single_spacing_threshold, osuCurrent.TravelDistance + osuCurrent.JumpDistance);
            double deltaTime = Math.Max(max_speed_bonus, current.DeltaTime);

            double speedBonus = 1.0;
            if (deltaTime < min_speed_bonus)
                speedBonus = 1 + Math.Pow((min_speed_bonus - deltaTime) / speed_balancing_factor, 2);

            double angleBonus = 1.0;

            if (osuCurrent.Angle != null && osuCurrent.Angle.Value < angle_bonus_begin)
            {
                angleBonus = 1 + Math.Pow(Math.Sin(1.5 * (angle_bonus_begin - osuCurrent.Angle.Value)), 2) / 3.57;

                if (osuCurrent.Angle.Value < pi_over_2)
                {
                    angleBonus = 1.28;
                    if (distance < 90 && osuCurrent.Angle.Value < pi_over_4)
                        angleBonus += (1 - angleBonus) * Math.Min((90 - distance) / 10, 1);
                    else if (distance < 90)
                        angleBonus += (1 - angleBonus) * Math.Min((90 - distance) / 10, 1) * Math.Sin((pi_over_2 - osuCurrent.Angle.Value) / pi_over_4);
                }
            }

            // return (1 + (speedBonus - 1) * 0.75) * angleBonus * (0.95 + speedBonus * Math.Pow(distance / single_spacing_threshold, 3.5)) / osuCurrent.StrainTime;

            double tapStrain = ((1 + (speedBonus - 1) * 0.75) * 0.95) / osuCurrent.StrainTime;
            double movementStrain = ((1 + (speedBonus - 1) * 0.75) * angleBonus * speedBonus * Math.Pow(distance / single_spacing_threshold, 3.5)) / osuCurrent.StrainTime;

            currentTapStrain *= strainDecay(current.DeltaTime);
            currentTapStrain += tapStrain * skillMultiplier;

            currentMovementStrain *= strainDecay(current.DeltaTime);
            currentMovementStrain += movementStrain * skillMultiplier;
        }

        private double strainDecay(double ms) => Math.Pow(strainDecayBase, ms / 1000);

        protected override double CalculateInitialStrain(double time) => (currentMovementStrain + currentTapStrain) * strainDecay(time - Previous[0].StartTime);

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            setStrainValues(current);

// Console.WriteLine(currentMovementStrain + currentTapStrain);

            return currentMovementStrain + currentTapStrain * calculateRhythmBonus(current.StartTime);
        }
    }
}
