// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Scoring;

namespace osu.Game.Scoring.Legacy
{
    public class LegacyScoreInfo : ScoreInfo
    {
        private int countGeki;

        public int CountGeki
        {
            get => countGeki;
            set
            {
                countGeki = value;

                switch (Ruleset?.ID ?? RulesetID)
                {
                    case 3:
                        Statistics[HitResult.Perfect] = value;
                        break;
                }
            }
        }

        private int count300;

        public int Count300
        {
            get => count300;
            set
            {
                count300 = value;

                switch (Ruleset?.ID ?? RulesetID)
                {
                    case 0:
                    case 1:
                    case 3:
                        Statistics[HitResult.Great] = value;
                        break;

                    case 2:
                        Statistics[HitResult.Perfect] = value;
                        break;
                }
            }
        }

        private int countKatu;

        public int CountKatu
        {
            get => countKatu;
            set
            {
                countKatu = value;

                switch (Ruleset?.ID ?? RulesetID)
                {
                    case 3:
                        Statistics[HitResult.Good] = value;
                        break;
                }
            }
        }

        private int count100;

        public int Count100
        {
            get => count100;
            set
            {
                count100 = value;

                switch (Ruleset?.ID ?? RulesetID)
                {
                    case 0:
                    case 1:
                        Statistics[HitResult.Good] = value;
                        break;

                    case 3:
                        Statistics[HitResult.Ok] = value;
                        break;
                }
            }
        }

        private int count50;

        public int Count50
        {
            get => count50;
            set
            {
                count50 = value;

                switch (Ruleset?.ID ?? RulesetID)
                {
                    case 0:
                    case 3:
                        Statistics[HitResult.Meh] = value;
                        break;
                }
            }
        }

        public int CountMiss
        {
            get => Statistics[HitResult.Miss];
            set => Statistics[HitResult.Miss] = value;
        }
    }
}
