// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Osu.Difficulty
{
    public record OsuDifficultyTuning
    {
        public static OsuDifficultyTuning Default { get; } = new OsuDifficultyTuning();
        /// <summary>
        /// Scales the aim component of performance.
        /// </summary>
        public double AimPerformanceScale { get; init; } = 1.0;

        /// <summary>
        /// Scales the speed component of performance.
        /// </summary>
        public double SpeedPerformanceScale { get; init; } = 1.0;

        /// <summary>
        /// Scales the accuracy component of performance.
        /// </summary>
        public double AccuracyPerformanceScale { get; init; } = 1.0;

        /// <summary>
        /// Scales the flashlight component of performance.
        /// </summary>
        public double FlashlightPerformanceScale { get; init; } = 1.0;

        /// <summary>
        /// Scales the combined performance value.
        /// </summary>
        public double TotalPerformanceScale { get; init; } = 1.0;

        public double AimSkillStrainScale { get; init; } = 1.0;
        public double SpeedSkillStrainScale { get; init; } = 1.0;
        public double FlashlightSkillStrainScale { get; init; } = 1.0;

        public double AimWideAngleBonusScale { get; init; } = 1.5;
        public double AimAcuteAngleScale { get; init; } = 2.55;
        public double AimSliderBonusScale { get; init; } = 1.35;
        public double AimVelocityChangeBonusScale { get; init; } = 0.75;
        public double AimWiggleBonusScale { get; init; } = 1.02;

        public double FlashlightMaxOpacityBonusScale { get; init; } = 0.4;
        public double FlashlightHiddenBonusScale { get; init; } = 0.2;
        public double FlashlightMinVelocityScale { get; init; } = 0.5;
        public double FlashlightSliderBonusScale { get; init; } = 1.3;
        public double FlashlightMinAngleScale { get; init; } = 0.2;

        public int RhythmHistoryTimeMax { get; init; } = 5 * 1000; // 5 seconds
        public int RhythmHistoryObjectsMax { get; init; } = 32;
        public double RhythmOverallScale { get; init; } = 1.0;
        public double RhythmRatioScale { get; init; } = 15.0;

        public double SpeedSingleSpacingThreshold { get; init; } = 125;
        public double SpeedMinBonusBpm { get; init; } = 200;
        public double SpeedBalancingFactor { get; init; } = 40;
        public double SpeedDistanceScale { get; init; } = 0.8;
    }
}
