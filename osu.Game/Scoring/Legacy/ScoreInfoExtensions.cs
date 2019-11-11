// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Scoring.Legacy
{
    public static class ScoreInfoExtensions
    {
        private static int? tryGetCount(Dictionary<HitResult, int> statistics, HitResult? hitResult)
            => hitResult is HitResult h
                ? statistics[h]
                : (int?)null;

        private static void trySetCount(Dictionary<HitResult, int> statistics, HitResult? hitResult, int value)
        {
            if (hitResult is HitResult hr)
                statistics[hr] = value;
        }

        private static int getId(this ScoreInfo scoreInfo) => scoreInfo.Ruleset?.ID ?? scoreInfo.RulesetID;

        private static HitResult? mapGeki(int rulesetId)
            => rulesetId switch
            {
                3 => HitResult.Perfect,
                _ => null,
            };

        public static int? GetCountGeki(this ScoreInfo scoreInfo) => tryGetCount(scoreInfo.Statistics, mapGeki(scoreInfo.getId()));

        public static void SetCountGeki(this ScoreInfo scoreInfo, int value) => trySetCount(scoreInfo.Statistics, mapGeki(scoreInfo.getId()), value);

        private static HitResult? map300(int rulesetId)
            => rulesetId switch
            {
                0 => HitResult.Great,
                1 => HitResult.Great,
                2 => HitResult.Perfect,
                3 => HitResult.Great,
                _ => null,
            };

        public static int? GetCount300(this ScoreInfo scoreInfo) => tryGetCount(scoreInfo.Statistics, map300(scoreInfo.getId()));

        public static void SetCount300(this ScoreInfo scoreInfo, int value) => trySetCount(scoreInfo.Statistics, map300(scoreInfo.getId()), value);

        private static HitResult? mapKatu(int rulesetId)
            => rulesetId switch
            {
                3 => HitResult.Good,
                _ => null,
            };

        public static int? GetCountKatu(this ScoreInfo scoreInfo) => tryGetCount(scoreInfo.Statistics, mapKatu(scoreInfo.getId()));

        public static void SetCountKatu(this ScoreInfo scoreInfo, int value) => trySetCount(scoreInfo.Statistics, mapKatu(scoreInfo.getId()), value);

        private static HitResult? map100(int rulesetId)
            => rulesetId switch
            {
                0 => HitResult.Good,
                1 => HitResult.Good,
                3 => HitResult.Ok,
                _ => null,
            };

        public static int? GetCount100(this ScoreInfo scoreInfo) => tryGetCount(scoreInfo.Statistics, map100(scoreInfo.getId()));

        public static void SetCount100(this ScoreInfo scoreInfo, int value) => trySetCount(scoreInfo.Statistics, map100(scoreInfo.getId()), value);

        private static HitResult? map50(int rulesetId)
            => rulesetId switch
            {
                0 => HitResult.Meh,
                3 => HitResult.Meh,
                _ => null,
            };

        public static int? GetCount50(this ScoreInfo scoreInfo) => tryGetCount(scoreInfo.Statistics, map50(scoreInfo.getId()));

        public static void SetCount50(this ScoreInfo scoreInfo, int value) => trySetCount(scoreInfo.Statistics, map50(scoreInfo.getId()), value);

        public static int? GetCountMiss(this ScoreInfo scoreInfo) =>
            scoreInfo.Statistics[HitResult.Miss];

        public static void SetCountMiss(this ScoreInfo scoreInfo, int value) =>
            scoreInfo.Statistics[HitResult.Miss] = value;
    }
}
