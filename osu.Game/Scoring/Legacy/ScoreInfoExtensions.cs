// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Scoring;

namespace osu.Game.Scoring.Legacy
{
    public static class ScoreInfoExtensions
    {
        public static int? GetCountGeki(this ScoreInfo scoreInfo)
        {
            switch (scoreInfo.Ruleset?.ID ?? scoreInfo.RulesetID)
            {
                case 3:
                    return scoreInfo.Statistics[HitResult.Perfect];
            }

            return null;
        }

        public static void SetCountGeki(this ScoreInfo scoreInfo, int value)
        {
            switch (scoreInfo.Ruleset?.ID ?? scoreInfo.RulesetID)
            {
                case 3:
                    scoreInfo.Statistics[HitResult.Perfect] = value;
                    break;
            }
        }

        public static int? GetCount300(this ScoreInfo scoreInfo)
        {
            switch (scoreInfo.Ruleset?.ID ?? scoreInfo.RulesetID)
            {
                case 0:
                case 1:
                case 3:
                    return scoreInfo.Statistics[HitResult.Great];

                case 2:
                    return scoreInfo.Statistics[HitResult.Perfect];
            }

            return null;
        }

        public static void SetCount300(this ScoreInfo scoreInfo, int value)
        {
            switch (scoreInfo.Ruleset?.ID ?? scoreInfo.RulesetID)
            {
                case 0:
                case 1:
                case 3:
                    scoreInfo.Statistics[HitResult.Great] = value;
                    break;

                case 2:
                    scoreInfo.Statistics[HitResult.Perfect] = value;
                    break;
            }
        }

        public static int? GetCountKatu(this ScoreInfo scoreInfo)
        {
            switch (scoreInfo.Ruleset?.ID ?? scoreInfo.RulesetID)
            {
                case 3:
                    return scoreInfo.Statistics[HitResult.Good];
            }

            return null;
        }

        public static void SetCountKatu(this ScoreInfo scoreInfo, int value)
        {
            switch (scoreInfo.Ruleset?.ID ?? scoreInfo.RulesetID)
            {
                case 3:
                    scoreInfo.Statistics[HitResult.Good] = value;
                    break;
            }
        }

        public static int? GetCount100(this ScoreInfo scoreInfo)
        {
            switch (scoreInfo.Ruleset?.ID ?? scoreInfo.RulesetID)
            {
                case 0:
                case 1:
                    return scoreInfo.Statistics[HitResult.Good];

                case 3:
                    return scoreInfo.Statistics[HitResult.Ok];
            }

            return null;
        }

        public static void SetCount100(this ScoreInfo scoreInfo, int value)
        {
            switch (scoreInfo.Ruleset?.ID ?? scoreInfo.RulesetID)
            {
                case 0:
                case 1:
                    scoreInfo.Statistics[HitResult.Good] = value;
                    break;

                case 3:
                    scoreInfo.Statistics[HitResult.Ok] = value;
                    break;
            }
        }

        public static int? GetCount50(this ScoreInfo scoreInfo)
        {
            switch (scoreInfo.Ruleset?.ID ?? scoreInfo.RulesetID)
            {
                case 0:
                case 3:
                    return scoreInfo.Statistics[HitResult.Meh];
            }

            return null;
        }

        public static void SetCount50(this ScoreInfo scoreInfo, int value)
        {
            switch (scoreInfo.Ruleset?.ID ?? scoreInfo.RulesetID)
            {
                case 0:
                case 3:
                    scoreInfo.Statistics[HitResult.Meh] = value;
                    break;
            }
        }

        public static int? GetCountMiss(this ScoreInfo scoreInfo) =>
            scoreInfo.Statistics[HitResult.Miss];

        public static void SetCountMiss(this ScoreInfo scoreInfo, int value) =>
            scoreInfo.Statistics[HitResult.Miss] = value;
    }
}
