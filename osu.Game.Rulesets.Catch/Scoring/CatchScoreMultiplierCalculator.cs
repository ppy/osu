// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Catch.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Catch.Scoring
{
    public class CatchScoreMultiplierCalculator : ScoreMultiplierCalculator
    {
        public CatchScoreMultiplierCalculator(ScoreMultiplierContext context)
            : base(context)
        {
            #region Difficulty Reduction

            Single<CatchModEasy>(hasMultiplier: 0.5);
            Single<CatchModNoFail>(hasMultiplier: 0.5);
            Single<CatchModHalfTime>(hasMultiplier: halfTime => rateAdjustMultiplier(halfTime.SpeedChange.Value));
            Single<CatchModDaycore>(hasMultiplier: daycore => rateAdjustMultiplier(daycore.SpeedChange.Value));

            #endregion

            #region Difficulty Increase

            Single<CatchModHardRock>(hasMultiplier: hardRock => hardRock.UsesDefaultConfiguration ? 1.12 : 1);
            // Sudden Death
            // Perfect
            Single<CatchModDoubleTime>(hasMultiplier: doubleTime => rateAdjustMultiplier(doubleTime.SpeedChange.Value));
            Single<CatchModNightcore>(hasMultiplier: nightcore => rateAdjustMultiplier(nightcore.SpeedChange.Value));
            Single<CatchModHidden>(hasMultiplier: hidden => hidden.UsesDefaultConfiguration ? 1.06 : 1);
            Single<CatchModFlashlight>(hasMultiplier: flashlight => flashlight.UsesDefaultConfiguration ? 1.12 : 1);
            // Accuracy Challenge

            #endregion

            #region Conversion

            Single<CatchModDifficultyAdjust>(hasMultiplier: 0.5);
            Single<CatchModClassic>(hasMultiplier: _ => classicMultiplier(context.Score));
            // Mirror

            #endregion

            #region Automation

            // Autoplay
            // Cinema
            Single<CatchModRelax>(hasMultiplier: 0.1);

            #endregion

            #region Fun

            Single<ModWindUp>(hasMultiplier: 0.5);
            Single<ModWindDown>(hasMultiplier: 0.5);
            // Floating Fruits
            // Muted
            // No Scope
            // Moving Fast
            Single<CatchModSynesthesia>(hasMultiplier: 0.8);

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
