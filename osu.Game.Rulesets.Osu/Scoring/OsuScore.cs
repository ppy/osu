// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Scoring
{
    public class OsuScore : Score
    {
        public int Count300;
        public int Count100;
        public int Count50;
        public int CountMiss;

        public override IEnumerable<ScoreStatistic> Statistics => new[] {
            new ScoreStatistic(@"300", Count300),
            new ScoreStatistic(@"100", Count100),
            new ScoreStatistic(@"50", Count50),
            new ScoreStatistic(@"x", CountMiss),
        };
    }
}
