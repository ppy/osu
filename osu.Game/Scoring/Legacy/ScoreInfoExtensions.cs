// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Scoring.Legacy
{
    public static class ScoreInfoExtensions
    {
        public static long GetDisplayScore(this ScoreProcessor scoreProcessor, ScoringMode mode)
            => getDisplayScore(scoreProcessor.Ruleset.RulesetInfo.OnlineID, scoreProcessor.TotalScore.Value, mode, scoreProcessor.MaximumStatistics);

        public static long GetDisplayScore(this ScoreInfo scoreInfo, ScoringMode mode)
            => getDisplayScore(scoreInfo.Ruleset.OnlineID, scoreInfo.TotalScore, mode, scoreInfo.MaximumStatistics);

        public static long GetDisplayScore(this SoloScoreInfo soloScoreInfo, ScoringMode mode)
            => getDisplayScore(soloScoreInfo.RulesetID, soloScoreInfo.TotalScore, mode, soloScoreInfo.MaximumStatistics);

        private static long getDisplayScore(int rulesetId, long score, ScoringMode mode, IReadOnlyDictionary<HitResult, int> maximumStatistics)
        {
            if (mode == ScoringMode.Standardised)
                return score;

            int maxBasicJudgements = maximumStatistics
                                     .Where(k => k.Key.IsBasic())
                                     .Select(k => k.Value)
                                     .DefaultIfEmpty(0)
                                     .Sum();

            return convertStandardisedToClassic(rulesetId, score, maxBasicJudgements);
        }

        /// <summary>
        /// Returns a ballpark "classic" score which gives a similar "feel" to stable.
        /// This is different per ruleset to match the different algorithms used in the scoring implementation.
        /// </summary>
        /// <remarks>
        /// The coefficients chosen here were determined by a least-squares fit performed over all beatmaps
        /// with the goal of minimising the relative error of maximum possible base score (without bonus).
        /// The constant coefficients (100000, 1 / 10d) - while being detrimental to the least-squares fit - are forced,
        /// so that every 10 points in standardised mode converts to at least 1 point in classic mode.
        /// This is done to account for bonus judgements in a way that does not reorder scores.
        /// </remarks>
        private static long convertStandardisedToClassic(int rulesetId, long standardisedTotalScore, int objectCount)
        {
            switch (rulesetId)
            {
                case 0:
                    return (long)Math.Round((objectCount * objectCount * 32.57 + 100000) * standardisedTotalScore / ScoreProcessor.MAX_SCORE);

                case 1:
                    return (long)Math.Round((objectCount * 1109 + 100000) * standardisedTotalScore / ScoreProcessor.MAX_SCORE);

                case 2:
                    return (long)Math.Round(Math.Pow(standardisedTotalScore / ScoreProcessor.MAX_SCORE * objectCount, 2) * 21.62 + standardisedTotalScore / 10d);

                case 3:
                default:
                    return standardisedTotalScore;
            }
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
