// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking;
using osu.Game.Screens.Ranking.Types;

namespace osu.Game.Screens.Play
{
    public class SoloResults : Results
    {
        public SoloResults(ScoreInfo score)
            : base(score)
        {
        }

        protected override IEnumerable<IResultPageInfo> CreateResultPages() => new IResultPageInfo[]
        {
            new ScoreOverviewPageInfo(Score, Beatmap),
            new LocalLeaderboardPageInfo(Score, Beatmap)
        };
    }
}
