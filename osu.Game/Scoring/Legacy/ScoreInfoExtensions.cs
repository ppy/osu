// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Scoring;

namespace osu.Game.Scoring.Legacy
{
    public static class ScoreInfoExtensions
    {
        public static int? GetCountGeki(this ScoreInfo scoreInfo)
        {
            switch (scoreInfo.Ruleset.OnlineID)
            {
                case 3:
                    return getCount(scoreInfo, HitResult.Perfect);
            }

            return null;
        }

        public static void SetCountGeki(this ScoreInfo scoreInfo, int value)
        {
            switch (scoreInfo.Ruleset.OnlineID)
            {
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
                case 3:
                    return getCount(scoreInfo, HitResult.Good);

                case 2:
                    return getCount(scoreInfo, HitResult.SmallTickMiss);
            }

            return null;
        }

        public static void SetCountKatu(this ScoreInfo scoreInfo, int value)
        {
            switch (scoreInfo.Ruleset.OnlineID)
            {
                case 3:
                    scoreInfo.Statistics[HitResult.Good] = value;
                    break;

                case 2:
                    scoreInfo.Statistics[HitResult.SmallTickMiss] = value;
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
