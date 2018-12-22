// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking.Pages;

namespace osu.Game.Screens.Ranking.Types
{
    public class ScoreOverviewPageInfo : IResultPageInfo
    {
        public FontAwesome Icon => FontAwesome.fa_asterisk;

        public string Name => "Overview";
        private readonly ScoreInfo score;
        private readonly WorkingBeatmap beatmap;

        public ScoreOverviewPageInfo(ScoreInfo score, WorkingBeatmap beatmap)
        {
            this.score = score;
            this.beatmap = beatmap;
        }

        public ResultsPage CreatePage()
        {
            return new ScoreResultsPage(score, beatmap);
        }
    }
}
