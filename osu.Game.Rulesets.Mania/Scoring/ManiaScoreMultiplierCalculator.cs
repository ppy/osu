// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Mania.Scoring
{
    public class ManiaScoreMultiplierCalculator : ScoreMultiplierCalculator
    {
        public ManiaScoreMultiplierCalculator(ScoreMultiplierContext context)
            : base(context)
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
            Single<ManiaModClassic>(hasMultiplier: _ => classicMultiplier(Context.Score));
            // Invert
            Single<ManiaModConstantSpeed>(hasMultiplier: 0.9);
            Single<ManiaModHoldOff>(hasMultiplier: 0.9);
            Single<ManiaModKey1>(hasMultiplier: keyModMultiplier(Context.Score));
            Single<ManiaModKey2>(hasMultiplier: keyModMultiplier(Context.Score));
            Single<ManiaModKey3>(hasMultiplier: keyModMultiplier(Context.Score));
            Single<ManiaModKey4>(hasMultiplier: keyModMultiplier(Context.Score));
            Single<ManiaModKey5>(hasMultiplier: keyModMultiplier(Context.Score));
            Single<ManiaModKey6>(hasMultiplier: keyModMultiplier(Context.Score));
            Single<ManiaModKey7>(hasMultiplier: keyModMultiplier(Context.Score));
            Single<ManiaModKey8>(hasMultiplier: keyModMultiplier(Context.Score));
            Single<ManiaModKey9>(hasMultiplier: keyModMultiplier(Context.Score));
            Single<ManiaModKey10>(hasMultiplier: keyModMultiplier(Context.Score));

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

        private const double old_key_mod_multiplier = 1;
        private const double new_key_mod_multiplier = 0.9;

        /// <summary>
        /// <para>
        /// The mod multiplier was changed from 1.0x to 0.9x in https://github.com/ppy/osu/pull/30506
        /// which was included in the https://osu.ppy.sh/home/changelog/tachyon/2025.718.0 release.
        /// The replay version was not bumped in the change, meaning that the only usable indicator
        /// of the mod multiplier changing is the client version.
        /// </para>
        /// <para>
        /// Unfortunately not even the client version is available on server-side recorded replays
        /// recorded prior to https://github.com/ppy/osu-server-spectator/pull/290,
        /// which does not appear to have been deployed until August 1
        /// (https://github.com/ppy/osu-server-spectator/releases/tag/2025.801.0).
        /// </para>
        /// </summary>
        private double keyModMultiplier(ScoreInfo? scoreInfo)
        {
            if (scoreInfo == null)
                return new_key_mod_multiplier;

            string clientVersion = scoreInfo.ClientVersion;

            if (!string.IsNullOrEmpty(clientVersion))
            {
                string[] pieces = clientVersion.Split('.');

                if (int.TryParse(pieces[0], out int year) && int.TryParse(pieces[1], out int monthDay))
                {
                    if (year < 2025 || (year == 2025 && monthDay < 718))
                        return old_key_mod_multiplier;
                }

                return new_key_mod_multiplier;
            }

            // Client version not available, fallback to doing the best we can with the score's timestamp.
            if (scoreInfo.Date < new DateTimeOffset(2025, 7, 18, 0, 0, 0, TimeSpan.Zero))
                return old_key_mod_multiplier;

            return new_key_mod_multiplier;
        }

        private static double classicMultiplier(ScoreInfo? score)
        {
            if (score != null && score.TotalScoreVersion < 30000017)
                return 0.96;

            return 1;
        }
    }
}
