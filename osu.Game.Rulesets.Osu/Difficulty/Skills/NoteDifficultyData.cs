// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    public struct NoteDifficultyData
    {
        public double PowDifficulty { get; set; }
        public double CumulativePowDifficulty { get; set; }
        public double Timestamp { get; set; }
        public double PrevTimestamp => Timestamp - DeltaTime;
        public double DeltaTime { get; }

        public static NoteDifficultyData SliderTick(DifficultyHitObject hitObject, double totalPowDifficulty)
        {
            return new NoteDifficultyData
            {
                CumulativePowDifficulty = totalPowDifficulty,
                Timestamp = hitObject.StartTime
            };
        }

        public NoteDifficultyData(DifficultyHitObject hitObject, double strain, double durationScalingFactor, double difficultyExponent, ref double totalPowDifficulty)
        {
            // Uses legacy formula to convert from strain into star rating
            double difficulty = Math.Sqrt(strain * 10) * 0.0675;

            PowDifficulty = Math.Pow(difficulty, difficultyExponent) * durationScalingFactor;
            totalPowDifficulty += PowDifficulty;
            CumulativePowDifficulty = totalPowDifficulty;

            Timestamp = hitObject.StartTime;
            DeltaTime = hitObject.DeltaTime;
        }
    }
}
