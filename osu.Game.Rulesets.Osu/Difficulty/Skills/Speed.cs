// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;
using osu.Framework.Utils;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Osu.Difficulty.Skills.Pre;

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
        protected override int ReducedSectionCount => 5;
        protected override double DifficultyMultiplier => 1.04;

        private double currentStrain;

        private double currentRhythm;

        private readonly double greatWindow;

        private readonly SpeedRhythmBonus speedRhythmBonus;

        private readonly SpeedStrainTime speedStrainTime;

        private readonly SpeedBonus speedBonus;

        public Speed(Mod[] mods, double greatWindow)
            : base(mods)
        {
            this.greatWindow = greatWindow;

            speedRhythmBonus = new SpeedRhythmBonus(mods, greatWindow);
            speedStrainTime = new SpeedStrainTime(mods, greatWindow);
            speedBonus = new SpeedBonus(mods, speedStrainTime);
        }
        private double strainValueOf(int index, DifficultyHitObject current)
        {
            speedStrainTime.ProcessInternal(index, current);
            speedBonus.ProcessInternal(index, current);

            return speedBonus[index];
        }

        private double strainDecay(double ms) => Math.Pow(strainDecayBase, ms / 1000);

        protected override double CalculateInitialStrain(double time) => (currentStrain * currentRhythm) * strainDecay(time - Previous[0].StartTime);

        protected override double StrainValueAt(int index, DifficultyHitObject current)
        {
            speedRhythmBonus.ProcessInternal(index, current);

            currentStrain *= strainDecay(current.DeltaTime);
            currentStrain += strainValueOf(index, current) * skillMultiplier;

            currentRhythm = speedRhythmBonus[index];

            return currentStrain * currentRhythm;
        }
    }
}
