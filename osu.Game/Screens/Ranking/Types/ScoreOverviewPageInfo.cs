// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking.Pages;

namespace osu.Game.Screens.Ranking.Types
{
    public class ScoreOverviewPageInfo : IResultPageInfo
    {
        public IconUsage Icon => FontAwesome.Solid.Asterisk;

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
