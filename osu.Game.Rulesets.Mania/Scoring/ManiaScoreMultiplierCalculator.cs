// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Scoring
{
    public class ManiaScoreMultiplierCalculator : ScoreMultiplierCalculator
    {
        public ManiaScoreMultiplierCalculator()
        {
            #region Difficulty Reduction

            Single<ManiaModEasy>(hasMultiplier: 0.5);
            Single<ManiaModNoFail>(hasMultiplier: 0.5);
            Single<ManiaModHalfTime>(hasMultiplier: halfTime => rateAdjustMultiplier(halfTime.SpeedChange.Value));
            Single<ManiaModDaycore>(hasMultiplier: daycore => rateAdjustMultiplier(daycore.SpeedChange.Value));
            Single<ManiaModNoRelease>(hasMultiplier: 0.9);

            #endregion

            #region Difficulty Increase

            // Hard Rock
            // Sudden Death
            // Perfect
            // Double Time
            // Nightcore
            // Fade In
            // Hidden
            // Cover
            // Flashlight
            // Accuracy Challenge

            #endregion

            #region Conversion

            // Random
            // Dual Stages
            // Mirror
            Single<ManiaModDifficultyAdjust>(hasMultiplier: 0.5);
            Single<ManiaModClassic>(hasMultiplier: 0.96);
            // Invert
            Single<ManiaModConstantSpeed>(hasMultiplier: 0.9);
            Single<ManiaModHoldOff>(hasMultiplier: 0.9);
            Single<ManiaModKey1>(hasMultiplier: 0.9);
            Single<ManiaModKey2>(hasMultiplier: 0.9);
            Single<ManiaModKey3>(hasMultiplier: 0.9);
            Single<ManiaModKey4>(hasMultiplier: 0.9);
            Single<ManiaModKey5>(hasMultiplier: 0.9);
            Single<ManiaModKey6>(hasMultiplier: 0.9);
            Single<ManiaModKey7>(hasMultiplier: 0.9);
            Single<ManiaModKey8>(hasMultiplier: 0.9);
            Single<ManiaModKey9>(hasMultiplier: 0.9);
            Single<ManiaModKey10>(hasMultiplier: 0.9);

            #endregion

            #region Automation

            // Autoplay
            // Cinema

            #endregion

            #region Fun

            Single<ModWindUp>(hasMultiplier: 0.5);
            Single<ModWindDown>(hasMultiplier: 0.5);
            // Muted
            Single<ModAdaptiveSpeed>(hasMultiplier: 0.5);

            #endregion

            #region System

            // Score V2

            #endregion
        }

        private static double rateAdjustMultiplier(double speedChange)
        {
            // Round to the nearest multiple of 0.1.
            double value = (int)(speedChange * 10) / 10.0;

            // Offset back to 0.
            value -= 1;

            if (speedChange >= 1)
                return 1 + value / 5;
            else
                return 0.6 + value;
        }
    }
}
