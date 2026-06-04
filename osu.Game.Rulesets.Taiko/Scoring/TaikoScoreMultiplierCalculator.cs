// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Mods;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Taiko.Scoring
{
    public class TaikoScoreMultiplierCalculator : ScoreMultiplierCalculator
    {
        public TaikoScoreMultiplierCalculator(ScoreMultiplierContext context)
            : base(context)
        {
            #region Difficulty Reduction

            Single<TaikoModEasy>(hasMultiplier: 0.5);
            Single<TaikoModNoFail>(hasMultiplier: 0.5);
            Single<TaikoModHalfTime>(hasMultiplier: halfTime => rateAdjustMultiplier(halfTime.SpeedChange.Value));
            Single<TaikoModDaycore>(hasMultiplier: daycore => rateAdjustMultiplier(daycore.SpeedChange.Value));
            Single<TaikoModSimplifiedRhythm>(hasMultiplier: 0.6);

            #endregion

            #region Difficulty Increase

            Single<TaikoModHardRock>(hasMultiplier: hardRock => hardRock.UsesDefaultConfiguration ? 1.06 : 1);
            // Sudden Death
            // Perfect
            Single<TaikoModDoubleTime>(hasMultiplier: doubleTime => rateAdjustMultiplier(doubleTime.SpeedChange.Value));
            Single<TaikoModNightcore>(hasMultiplier: nightcore => rateAdjustMultiplier(nightcore.SpeedChange.Value));
            Single<TaikoModHidden>(hasMultiplier: hidden => hidden.UsesDefaultConfiguration ? 1.06 : 1);
            Single<TaikoModFlashlight>(hasMultiplier: flashlight => flashlight.UsesDefaultConfiguration ? 1.12 : 1);
            // Accuracy Challenge

            #endregion

            #region Conversion

            // Random
            Single<TaikoModDifficultyAdjust>(hasMultiplier: 0.5);
            Single<TaikoModClassic>(hasMultiplier: _ => classicMultiplier(Context.Score));
            // Swap
            // Single Tap
            Single<TaikoModConstantSpeed>(hasMultiplier: 0.9);

            #endregion

            #region Automation

            // Autoplay
            // Cinema
            Single<TaikoModRelax>(hasMultiplier: 0.1);

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

        private static double classicMultiplier(ScoreInfo? score)
        {
            if (score != null && score.TotalScoreVersion < 30000017)
                return 0.96;

            return 1;
        }
    }
}
