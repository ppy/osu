// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Scoring.Legacy
{
    public static class ScoreInfoExtensions
    {
        public static long GetDisplayScore(this ScoreProcessor scoreProcessor, ScoringMode mode)
            => getDisplayScore(scoreProcessor.Ruleset.RulesetInfo.OnlineID, scoreProcessor.TotalScore.Value, mode, scoreProcessor.MaximumStatistics);

        public static long GetDisplayScore(this ScoreInfo scoreInfo, ScoringMode mode)
            => getDisplayScore(scoreInfo.Ruleset.OnlineID, scoreInfo.TotalScore, mode, scoreInfo.MaximumStatistics);

        private static long getDisplayScore(int rulesetId, long score, ScoringMode mode, IReadOnlyDictionary<HitResult, int> maximumStatistics)
        {
            if (mode == ScoringMode.Standardised)
                return score;

            int maxBasicJudgements = maximumStatistics
                                     .Where(k => k.Key.IsBasic())
                                     .Select(k => k.Value)
                                     .DefaultIfEmpty(0)
                                     .Sum();

            // This gives a similar feeling to osu!stable scoring (ScoreV1) while keeping classic scoring as only a constant multiple of standardised scoring.
            // The invariant is important to ensure that scores don't get re-ordered on leaderboards between the two scoring modes.
            double scaledRawScore = score / ScoreProcessor.MAX_SCORE;

            return (long)Math.Round(Math.Pow(scaledRawScore * Math.Max(1, maxBasicJudgements), 2) * getStandardisedToClassicMultiplier(rulesetId));
        }

        /// <summary>
        /// Returns a ballpark multiplier which gives a similar "feel" for how large scores should get when displayed in "classic" mode.
        /// This is different per ruleset to match the different algorithms used in the scoring implementation.
        /// </summary>
        private static double getStandardisedToClassicMultiplier(int rulesetId)
        {
            double multiplier;

            switch (rulesetId)
            {
                // For non-legacy rulesets, just go with the same as the osu! ruleset.
                // This is arbitrary, but at least allows the setting to do something to the score.
                default:
                case 0:
                    multiplier = 36;
                    break;

                case 1:
                    multiplier = 22;
                    break;

                case 2:
                    multiplier = 28;
                    break;

                case 3:
                    multiplier = 16;
                    break;
            }

            return multiplier;
        }

        public static int? GetCountGeki(this ScoreInfo scoreInfo)
        {
            switch (scoreInfo.Ruleset.OnlineID)
            {
                case 1:
                    return getCount(scoreInfo, HitResult.LargeBonus);

                case 3:
                    return getCount(scoreInfo, HitResult.Perfect);
            }

            return null;
        }

        public static void SetCountGeki(this ScoreInfo scoreInfo, int value)
        {
            switch (scoreInfo.Ruleset.OnlineID)
            {
                // For legacy scores, Geki indicates hit300 + perfect strong note hit.
                // Lazer only has one result for a perfect strong note hit (LargeBonus).
                case 1:
                    scoreInfo.Statistics[HitResult.LargeBonus] = scoreInfo.Statistics.GetValueOrDefault(HitResult.LargeBonus) + value;
                    break;

                case 3:
                    scoreInfo.Statistics[HitResult.Perfect] = value;
                    break;
            }
        }

        public static int? GetCount300(this ScoreInfo scoreInfo) => getCount(scoreInfo, HitResult.Great);

        public static void SetCount300(this ScoreInfo scoreInfo, int value) => scoreInfo.Statistics[HitResult.Great] = value;

        public static int? GetCountKatu(this ScoreInfo scoreInfo)
        {
            switch (scoreInfo.Ruleset.OnlineID)
            {
                // For taiko, Katu is bundled into Geki.
                case 1:
                    break;

                case 2:
                    return getCount(scoreInfo, HitResult.SmallTickMiss);

                case 3:
                    return getCount(scoreInfo, HitResult.Good);
            }

            return null;
        }

        public static void SetCountKatu(this ScoreInfo scoreInfo, int value)
        {
            switch (scoreInfo.Ruleset.OnlineID)
            {
                // For legacy scores, Katu indicates hit100 + perfect strong note hit.
                // Lazer only has one result for a perfect strong note hit (LargeBonus).
                case 1:
                    scoreInfo.Statistics[HitResult.LargeBonus] = scoreInfo.Statistics.GetValueOrDefault(HitResult.LargeBonus) + value;
                    break;

                case 2:
                    scoreInfo.Statistics[HitResult.SmallTickMiss] = value;
                    break;

                case 3:
                    scoreInfo.Statistics[HitResult.Good] = value;
                    break;
            }
        }

        public static int? GetCount100(this ScoreInfo scoreInfo)
        {
            switch (scoreInfo.Ruleset.OnlineID)
            {
                case 0:
                case 1:
                case 3:
                    return getCount(scoreInfo, HitResult.Ok);

                case 2:
                    return getCount(scoreInfo, HitResult.LargeTickHit);
            }

            return null;
        }

        public static void SetCount100(this ScoreInfo scoreInfo, int value)
        {
            switch (scoreInfo.Ruleset.OnlineID)
            {
                case 0:
                case 1:
                case 3:
                    scoreInfo.Statistics[HitResult.Ok] = value;
                    break;

                case 2:
                    scoreInfo.Statistics[HitResult.LargeTickHit] = value;
                    break;
            }
        }

        public static int? GetCount50(this ScoreInfo scoreInfo)
        {
            switch (scoreInfo.Ruleset.OnlineID)
            {
                case 0:
                case 3:
                    return getCount(scoreInfo, HitResult.Meh);

                case 2:
                    return getCount(scoreInfo, HitResult.SmallTickHit);
            }

            return null;
        }

        public static void SetCount50(this ScoreInfo scoreInfo, int value)
        {
            switch (scoreInfo.Ruleset.OnlineID)
            {
                case 0:
                case 3:
                    scoreInfo.Statistics[HitResult.Meh] = value;
                    break;

                case 2:
                    scoreInfo.Statistics[HitResult.SmallTickHit] = value;
                    break;
            }
        }

        public static int? GetCountMiss(this ScoreInfo scoreInfo) =>
            getCount(scoreInfo, HitResult.Miss);

        public static void SetCountMiss(this ScoreInfo scoreInfo, int value) =>
            scoreInfo.Statistics[HitResult.Miss] = value;

        private static int? getCount(ScoreInfo scoreInfo, HitResult result)
        {
            if (scoreInfo.Statistics.TryGetValue(result, out int existing))
                return existing;

            return null;
        }
    }
}
