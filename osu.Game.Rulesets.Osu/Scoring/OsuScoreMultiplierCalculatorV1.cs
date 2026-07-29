// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Scoring
{
    public class OsuScoreMultiplierCalculatorV1 : ScoreMultiplierCalculator
    {
        public OsuScoreMultiplierCalculatorV1(ScoreMultiplierContext context)
            : base(context)
        {
            #region Difficulty Reduction

            Single<OsuModEasy>(hasMultiplier: 0.5);
            Single<OsuModNoFail>(hasMultiplier: 0.5);
            Single<OsuModHalfTime>(hasMultiplier: halfTime => rateAdjustMultiplier(halfTime.SpeedChange.Value));
            Single<OsuModDaycore>(hasMultiplier: daycore => rateAdjustMultiplier(daycore.SpeedChange.Value));

            #endregion

            #region Difficulty Increase

            Single<OsuModHardRock>(hasMultiplier: hardRock => hardRock.UsesDefaultConfiguration ? 1.06 : 1);
            // Sudden Death
            // Perfect
            Single<OsuModDoubleTime>(hasMultiplier: doubleTime => rateAdjustMultiplier(doubleTime.SpeedChange.Value));
            Single<OsuModNightcore>(hasMultiplier: nightcore => rateAdjustMultiplier(nightcore.SpeedChange.Value));
            Single<OsuModHidden>(hasMultiplier: hidden => hidden.UsesDefaultConfiguration ? 1.06 : 1);
            // Traceable
            Single<OsuModFlashlight>(hasMultiplier: flashlight => flashlight.UsesDefaultConfiguration ? 1.12 : 1);
            Single<OsuModBlinds>(hasMultiplier: blinds => blinds.UsesDefaultConfiguration ? 1.12 : 1);
            // Strict Tracking
            // Accuracy Challenge

            #endregion

            #region Conversion

            Single<OsuModTargetPractice>(hasMultiplier: 0.1);
            Single<OsuModDifficultyAdjust>(hasMultiplier: 0.5);
            Single<OsuModClassic>(hasMultiplier: 0.96);
            // Random
            // Mirror
            // Alternate
            // Single Tap

            #endregion

            #region Automation

            // Autoplay
            // Cinema
            Single<OsuModRelax>(hasMultiplier: 0.1);
            Single<OsuModAutopilot>(hasMultiplier: 0.1);
            Single<OsuModSpunOut>(hasMultiplier: 0.9);

            #endregion

            #region Fun

            // Transform
            // Wiggle
            // Spin In
            // Grow
            // Deflate
            Single<ModWindUp>(hasMultiplier: 0.5);
            Single<ModWindDown>(hasMultiplier: 0.5);
            // Barrel Roll
            // Approach Different
            // Muted
            // No Scope
            Single<OsuModMagnetised>(hasMultiplier: 0.5);
            // Repel
            Single<ModAdaptiveSpeed>(hasMultiplier: 0.5);
            // Freeze Frame
            // Bubbles
            Single<OsuModSynesthesia>(hasMultiplier: 0.8);
            // Depth
            // Bloom

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
